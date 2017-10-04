using NLog;
using Pelco.PDK.Media.Pipeline;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;

namespace Pelco.PDK.Media.RTSP
{
    /// <summary>
    /// Decodes an RTSP message.  An RTSP message can be a request, response, or
    /// interleaved RTP data.
    /// </summary>
    public class RtspMessageDecoder
    {
        private static readonly char INTERLEAVED_MARKER = '$';

        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private int _bufferIdx;
        private int _chunkSize;
        private ReadingState _state;
        private RtspMessage _message;
        private LineReader _lineReader;
        private InterleavedData _interleavedPacket;
        private BlockingCollection<ByteBuffer> _rtpQueue;

        public RtspMessageDecoder(BlockingCollection<ByteBuffer> rtpQueue)
        {
            _message = null;
            _chunkSize = 0;
            _bufferIdx = 0;
            _rtpQueue = rtpQueue;
            _interleavedPacket = null;
            _lineReader = new LineReader();
            _state = ReadingState.SkipControlChars;
        }
        
        /// <summary>
        /// Event handler for receiving <see cref="RtspMessage"/>s. Rtsp messages
        /// are either <see cref="RtspRequest"/>s or <see cref="RtspResponse"/>s. 
        /// </summary>
        public event EventHandler<RtspMessageEventArgs> RtspMessageReceived;

        public void Decode(MemoryStream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                switch (_state)
                {
                    case ReadingState.SkipControlChars:
                        {
                            SkipControlCharacters(reader);
                            _state = ReadingState.ReadInitial;
                            break;
                        }
                    case ReadingState.ReadInitial:
                        {
                            char currentChar = Convert.ToChar(reader.PeekChar());
                            if (currentChar == INTERLEAVED_MARKER)
                            {
                                _state = ReadingState.ReadInterleavedData;
                                return;
                            }
                            else
                            {
                                try
                                {
                                    string line = _lineReader.Read(reader);
                                    string[] parts = SplitInitialLine(line);
                                    if (parts.Length < 3)
                                    {
                                        // This is an invalid initial line just ignore it.
                                        LOG.Warn($"Invalid start of RTSP message: '{line}', ignoring...");
                                        _state = ReadingState.SkipControlChars;
                                        return;
                                    }
                                    else
                                    {
                                        _message = RtspMessage.CreateNewMessage(parts);
                                        _state = ReadingState.ReadHeader;
                                    }
                                }
                                catch (Exception e)
                                {
                                    LOG.Error($"Failed to parse RTSP message, reason: {e.Message}");
                                    _state = ReadingState.BadMessage;
                                }
                            }

                            break;
                        }
                    case ReadingState.ReadHeader:
                        {
                            _state = ReadHeaders(_message, reader);

                            // If the check size is greater than zero it will be decreased as the
                            // ReadFixedContentLength state reads the body chunk by chunk.
                            _chunkSize = _message.ContentLength;

                            if (_state == ReadingState.SkipControlChars || (_chunkSize <= 0))
                            {
                                // No content data expected.
                                // TODO(frank.lamar):  Add support for chunking.  Not important
                                // at the moment because I have yet to see a device use chunking.
                                RtspMessageReceived?.Invoke(this, new RtspMessageEventArgs(_message));
                                Reset();
                            }

                            break;
                        }
                    case ReadingState.ReadFixedContentLength:
                        {
                            if (_bufferIdx == 0)
                            {
                                // We are reading the first chunk of data. We need to first
                                // allocate the message's data array.
                                _message.Body = new byte[_chunkSize];
                            }

                            int bytesRead = reader.Read(_message.Body, _bufferIdx, _chunkSize);
                            _chunkSize -= bytesRead; // Decrement the bytes read from the chunk size.
                            _bufferIdx += bytesRead; // Increment the index to write next chunk to.

                            if (_chunkSize == 0)
                            {
                                RtspMessageReceived?.Invoke(this, new RtspMessageEventArgs(_message));
                                Reset();
                            }

                            break;
                        }
                    case ReadingState.ReadInterleavedData:
                        {
                            if (_chunkSize == 0 && _bufferIdx == 0)
                            {
                                reader.ReadByte(); // Consume the '$' marker left in the buffer.

                                byte channel = reader.ReadByte();
                                _chunkSize = GetUint16(reader);

                                _interleavedPacket = new InterleavedData(channel);
                                _interleavedPacket.Data = new byte[_chunkSize];
                            }

                            int bytesRead = reader.Read(_interleavedPacket.Data, _bufferIdx, _chunkSize);
                            _chunkSize -= bytesRead; // Decrement the bytes read from the chunk size.
                            _bufferIdx += bytesRead; // Increment the index to write next chunk to. 

                            if (_chunkSize == 0)
                            {
                                var buffer = new Pipeline.ByteBuffer(_interleavedPacket.Data, 0, _interleavedPacket.Data.Length, true);
                                buffer.Channel = _interleavedPacket.Channel;

                                _rtpQueue.Add(buffer);
                                Reset();
                            }

                            break;
                        }

                    case ReadingState.BadMessage:
                        {
                            LOG.Debug("Unable to decode RTSP message, ignoring messsage");
                            stream.Seek(0, SeekOrigin.End); // Skip the remaining buffer.
                            Reset();
                            break;
                        }

                    default:
                        {
                            LOG.Warn($"Unknown state: {_state}");
                            Reset();
                            break;
                        }
                }
            }
        }

        public ushort GetUint16(BinaryReader reader)
        {
            return BitConverter.ToUInt16(ToHostOrder(reader.ReadBytes(2)), 0);
        }


        private byte[] ToHostOrder(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        private void SkipControlCharacters(BinaryReader reader)
        {
            for (;;)
            {
                char c = Convert.ToChar(reader.ReadByte());
                if (!Char.IsControl(c) && !Char.IsWhiteSpace(c))
                {
                    // Re-set the position as if we never read the character.
                    reader.BaseStream.Position = reader.BaseStream.Position - 1;
                    break;
                }
            }
        }

        private ReadingState ReadHeaders(RtspMessage msg, BinaryReader stream)
        {
            string name = null;
            string value = null;

            var line = _lineReader.Read(stream);
            if (line.Length > 0)
            {
                msg.Headers.Clear();

                do
                {
                    char firstChar = line[0];
                    if (name != null && (firstChar == ' ' || firstChar == '\t'))
                    {
                        value = new StringBuilder(value).Append(' ').Append(line.Trim()).ToString();
                    }
                    else
                    {
                        if (name != null)
                        {
                            msg.Headers[name] = value;
                        }

                        string[] header = SlitHeader(line);
                        name = header[0];
                        value = header[1];
                    }

                    line = _lineReader.Read(stream);
                }
                while (line.Length > 0);

                // Make sure we add the last header.
                if (name != null)
                {
                    msg.Headers[name] = value;
                }
            }

            var length = msg.ContentLength;

            return length >= 0 ? ReadingState.ReadFixedContentLength : ReadingState.SkipControlChars;
        }

        private void Reset(ReadingState state = ReadingState.SkipControlChars)
        {
            _message = null;
            _interleavedPacket = null;
            _chunkSize = 0;
            _bufferIdx = 0;
            _state = state;
        }

        /// <summary>
        /// Helper method to split an inital RTSP message start line into its parts.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string[] SplitInitialLine(string text)
        {
            int aStart, aEnd;
            int bStart, bEnd;
            int cStart, cEnd;

            aStart = FindNonWhitespace(text, 0);
            aEnd = FindWhitespace(text, aStart);

            bStart = FindNonWhitespace(text, aEnd);
            bEnd = FindWhitespace(text, bStart);

            cStart = FindNonWhitespace(text, bEnd);
            cEnd = FindEndOfString(text);

            var one = text.Substring(aStart, aEnd);
            var two = text.Substring(bStart, bEnd - bStart);
            var three = text.Substring(cStart, cEnd - cStart);

            return new string[]
            {
                text.Substring(aStart, aEnd),
                text.Substring(bStart, bEnd - bStart),
                cStart < cEnd ? text.Substring(cStart, cEnd - cStart) : string.Empty
            };
        }

        /// <summary>
        /// Helper method to split a string into a key value pair without needing to
        /// worry about spacing in the header.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string[] SlitHeader(string text)
        {
            int colonEnd;
            int nameStart, nameEnd;
            int valueStart, valueEnd;
            int length = text.Length;

            nameStart = FindNonWhitespace(text, 0);
            for (nameEnd = nameStart; nameEnd < length; nameEnd++)
            {
                char c = text[nameEnd];
                if (c == ':' || Char.IsWhiteSpace(c))
                {
                    break;
                }
            }

            for (colonEnd = nameEnd; colonEnd < length; colonEnd++)
            {
                if (text[colonEnd] == ':')
                {
                    colonEnd++;
                    break;
                }
            }

            valueStart = FindNonWhitespace(text, colonEnd);
            if (valueStart == length)
            {
                // No value for the header.
                return new string[] { text.Substring(nameStart, nameEnd), string.Empty };
            }

            valueEnd = FindEndOfString(text);
            return new string[]
            {
                text.Substring(nameStart, nameEnd),
                text.Substring(valueStart, valueEnd - valueStart)
            };
        }

        private int FindNonWhitespace(string text, int offset)
        {
            int result;
            for (result = offset; result < text.Length; ++result)
            {
                if (!Char.IsWhiteSpace(text[result]))
                {
                    break;
                }
            }

            return result;
        }

        private int FindWhitespace(string text, int offset)
        {
            int result;
            for (result = offset; result < text.Length; ++result)
            {
                if (Char.IsWhiteSpace(text[result]))
                {
                    break;
                }
            }

            return result;
        }

        private int FindEndOfString(string text)
        {
            int result;
            for (result = text.Length; result > 0; result--)
            {
                if (!Char.IsWhiteSpace(text[result - 1]))
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// The RtspMessage parsing state machine states.
        /// </summary>
        private enum ReadingState
        {
            SkipControlChars,
            ReadInitial,
            ReadHeader,
            ReadFixedContentLength,
            ReadInterleavedData,
            MessageComplete,
            BadMessage,
        }

        private class LineReader
        {
            int _size;
            private MemoryStream _lineBuffer;

            public LineReader(int capacity = 4096)
            {  // Lets limit the value to 4k, even though this is way too damn big.

                _size = 0;
                _lineBuffer = new MemoryStream(capacity);
            }

            public string Read(BinaryReader reader)
            {
                _size = 0;
                _lineBuffer.SetLength(0); // Clear the stream

                for(;;)
                {
                    byte b = reader.ReadByte();
                    char c = Convert.ToChar(b);

                    if (c == '\r')
                    {
                        break;
                    }
                    if (c != '\n')
                    {
                        if (_size > _lineBuffer.Capacity)
                        {
                            throw new RtspMessageParseException($"RTSP message line is larger than {_lineBuffer.Capacity} bytes");
                        }

                        ++_size;
                        _lineBuffer.WriteByte(b);
                    }
                }

                return Encoding.UTF8.GetString(_lineBuffer.ToArray());
            }
        }
    }
}
