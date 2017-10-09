using System;
using System.IO;
using System.Net;
using System.Text;

namespace Pelco.Media.Pipeline
{
    public class ByteBuffer : IDisposable
    {
        private static readonly Int32 MAX_BUFFER_LENGTH = Int32.MaxValue;

        private byte[] _buffer;
        private bool _isOpen;
        private bool _readOnly;
        private Int32 _length;
        private Int32 _capacity;
        private Int32 _position;
        private Int32 _startIndex;
        private bool _isExpandable;

        public ByteBuffer() : this(0)
        {

        }

        public ByteBuffer(Int32 capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot create buffer with a cacity of '{capacity}'");
            }

            _buffer = new byte[capacity];
            _capacity = capacity;
            _length = 0;
            _position = 0;
            _startIndex = 0;
            _isOpen = true;
            _readOnly = true;
            _isExpandable = true;
        }

        public ByteBuffer(byte[] buffer) : this(buffer, false)
        {

        }

        public ByteBuffer(byte[] buffer, bool isReadOnly)
        {
            _buffer = buffer ?? throw new ArgumentNullException("Cannot create buffer from null byte[]");

            _startIndex = 0;
            _position = 0;
            _length = buffer.Length;
            _capacity = buffer.Length;
            _readOnly = isReadOnly;
            _isOpen = true;
            _isExpandable = false;
        }

        public ByteBuffer(byte[] buffer, int index, int count, bool isReadonly = true)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("Cannot create buffer from null byte[]");
            }
            else if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Cannot create buffer with index less than 0");
            }
            else if (count < 0)
            {
                throw new ArgumentOutOfRangeException("Cannot create buffer with a byte cout less than zero");
            }
            else if (buffer.Length - index < count)
            {
                throw new ArgumentOutOfRangeException($"Cannot create buffer '{count}' bytes are not available starting at '{index}'");
            }

            _buffer = buffer;
            _startIndex = index;
            _position = index;
            _length = index + count;
            _capacity = index + count;
            _isOpen = true;
            _readOnly = isReadonly;
            _isExpandable = false;
        }

        #region Properties

        public bool IsReadOnly
        {
            get
            {
                return _readOnly;
            }
        }

        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
        }


        public Int32 Length
        {
            get
            {
                return _length - _startIndex;
            }
        }

        /// <summary>
        /// Returns the actual capacity, in bytes, of the buffer.  Capacity differs from length
        /// as the length is the amount of data that is actually stored in the buffer, but the
        /// physical size of the buffer.
        /// </summary>
        public Int32 Capacity
        {
            get
            {
                return _capacity - _startIndex;
            }

            set
            {
                EnsureBufferIsOpen();

                if (_isExpandable && (_capacity != value && value > _capacity))
                {
                    var buf = new byte[value];
                    if (_length > 0)
                    {
                        // If there is data in the buffer copy it over.
                        System.Buffer.BlockCopy(_buffer, 0, buf, 0, _length);
                    }

                    _buffer = buf;
                    _capacity = value;
                }
            }
        }

        /// <summary>
        /// Returns the current read/write position of the buffer.
        /// </summary>
        public Int32 Position
        {
            get
            {
                return _position - _startIndex;
            }
        }

        /// <summary>
        /// Returns the remaining bytes.  This is the number of bytes before reaching the end
        /// of the buffer.
        /// </summary>
        public Int32 RemainingBytes
        {
            get
            {
                return Length - Position;
            }
        }

        /// <summary>
        /// Gets the actual start index of the buffer.  This an only be used by
        /// classes within the context of this assembly.
        /// </summary>
        internal Int32 StartIndex
        {
            get
            {
                return _startIndex;
            }
        }

        /// <summary>
        /// Gets the underlying byte[].  This can only be used by classes within
        /// the context of this assembly.
        /// </summary>
        internal byte[] Raw
        {
            get
            {
                return _buffer;
            }
        }

        public DateTime TimeReference { get; set; }

        public Int32 Channel { get; set; }

        public object UserData { get; set; }

        #endregion

        public string ToString(Encoding encoding)
        {
            return encoding.GetString(_buffer, _startIndex, _length - _startIndex);
        }

        /// <summary>
        /// Returns a writeable copy of the buffer.  If the buffer is already
        /// writeable then the exiting buffer is returned without copying data.
        /// </summary>
        /// <param name="preservePostion">Preserve the current buffer's position in the new array</param>
        /// <returns></returns>
        public ByteBuffer GetWriteableBuffer(bool preservePostion = false)
        {
            if (_readOnly)
            {
                // The buffer is readonly lets make a copy and mark it as writeable
                return Copy(preservePostion);
            }

            return this;
        }

        /// <summary>
        /// Returns a writeable copy of the buffer
        /// </summary>
        /// <param name="preservePosition">Preserver the current buffer's postion in the new array</param>
        /// <returns></returns>
        public ByteBuffer Copy(bool preservePosition = false)
        {
            var copy = new byte[_length];
            System.Buffer.BlockCopy(_buffer, _startIndex, copy, 0, _length);

            var buffer = new ByteBuffer(copy);

            if (preservePosition)
            {
                buffer._position = _position - _startIndex;
            }

            return buffer;
        }

        /// <summary>
        /// Marks the buffer as readonly.  This is helpful if you have created a copy of a buffer for
        /// writing and now want to prevent it from being modified.
        /// </summary>
        public void MarkReadOnly()
        {
            _readOnly = true;
        }

        public byte ReadByte()
        {
            EnsureCanRead(1);

            return _buffer[_position++];
        }

        public Int16 ReadInt16()
        {
            EnsureCanRead(2);

            Int16 value = BitConverter.ToInt16(_buffer, _position);
            _position += 2;

            return value;
        }

        public Int16 ReadInt16AsHost()
        {
            return IPAddress.NetworkToHostOrder(ReadInt16());
        }

        public UInt16 ReadUInt16()
        {
            EnsureCanRead(2);

            UInt16 value = BitConverter.ToUInt16(_buffer, _position);
            _position += 2;

            return value;
        }

        public UInt16 ReadUInt16AsHost()
        {
            return (UInt16)ReadInt16AsHost();
        }

        public Int32 ReadInt32()
        {
            EnsureCanRead(4);

            Int32 value = BitConverter.ToInt32(_buffer, _position);
            _position += 4;

            return value;
        }

        public Int32 ReadInt32AsHost()
        {
            return IPAddress.NetworkToHostOrder(ReadInt32());
        }

        public UInt32 ReadUInt32()
        {
            EnsureCanRead(4);

            UInt32 value = BitConverter.ToUInt32(_buffer, _position);
            _position += 4;

            return value;
        }

        public UInt32 ReadUInt32AsHost()
        {
            return (UInt32)ReadInt32AsHost();
        }

        public Int64 ReadInt64()
        {
            EnsureCanRead(8);

            Int64 value = BitConverter.ToInt64(_buffer, _position);
            _position += 8;

            return value;
        }

        public Int64 ReadInt64AsHost()
        {
            return IPAddress.NetworkToHostOrder(ReadInt64());
        }

        public UInt64 ReadUInt64()
        {
            EnsureCanRead(8);

            UInt64 value = BitConverter.ToUInt64(_buffer, _position);
            _position += 8;

            return value;
        }

        public UInt64 ReadUInt64AsHost()
        {
            return (UInt64)ReadInt64AsHost();
        }

        public byte[] ReadBytes(Int32 count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot read '{count}' bytes from buffer");
            }

            var result = new byte[count];

            var bytesRead = Read(result, 0, count);
            
            if (bytesRead < count)
            {
                if (bytesRead == 0)
                {
                    // Not remaining data just return an empty array
                    return new byte[0];
                }

                // Lets trim the array since it was smaller
                var copy = new byte[bytesRead];
                Buffer.BlockCopy(result, 0, copy, 0, bytesRead);

                return copy;
            }

            return result;
        }

        public Int32 Read(byte[] buffer, Int32 offset, Int32 count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("Cannot read into null byte[]");
            }
            else if (offset < 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot read into byte[] with offset '{offset}'");
            }
            else if (count < 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot read into byte[] with count of '{count}'");
            }
            else if ((buffer.Length - offset) < count)
            {
                throw new ArgumentOutOfRangeException($"Cannot read into byte[] '{count}' bytes are not available starting at '{offset}'");
            }

            EnsureBufferIsOpen();

            Int32 n = _length - _position;
            n = (n > count) ? count : n;

            if (n <= 0)
            {
                // There are no more bytes to read.
                return 0;
            }

            Buffer.BlockCopy(_buffer, _position, buffer, offset, n);
            _position += n;

            return n;
        }

        public void Write(ByteBuffer buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("Cannot write to Buffer provided buffer is null");
            }

            Write(buffer._buffer, buffer._startIndex, buffer._length);
        }

        public void Write(byte[] buffer, Int32 offset, Int32 count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("Cannot write to Buffer provided byte[] is null");
            }
            else if (offset < 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot write to Buffer with offset '{offset}'");
            }
            else if (count < 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot write to Buffer with byte count of '{count}'");
            }
            else if ((buffer.Length - offset) < count)
            {
                throw new ArgumentOutOfRangeException($"Cannot write to Buffer '{count}' bytes are not available starting at '{offset}'");
            }

            EnsureBufferIsOpen();
            EnsureBufferIsWriteable();

            Int32 newPosition = _position + count;

            if (newPosition < 0)
            {
                // We have overflowed report to the user that we cannot write anymore.
                throw new IOException("Cannot write bytes expand past the maximum Buffer length.");
            }

            if (newPosition > _length)
            {
                if (newPosition > _capacity)
                {
                    ExpandCapacity(newPosition);
                }

                _length = newPosition;
            }

            Buffer.BlockCopy(buffer, offset, _buffer, _position, count);
            _position = newPosition;
        }

        public void WriteInt16(Int16 value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void WriteInt26NetworkOrder(Int16 value)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            Write(bytes, 0, bytes.Length);
        }

        public void WriteUInt16(UInt16 value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void WriteUInt16NetworkOrder(UInt16 value)
        {
            var bytes = BitConverter.GetBytes((UInt16)IPAddress.HostToNetworkOrder(value));
            Write(bytes, 0, bytes.Length);
        }

        public void WriteInt32(Int32 value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void WriteInt32NetworkOrder(Int32 value)
        {
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            Write(bytes, 0, bytes.Length);
        }

        public void WriteUInt32(UInt32 value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void WriteUint32NetworkOrder(UInt32 value)
        {
            var byets = BitConverter.GetBytes((UInt32)IPAddress.HostToNetworkOrder(value));
        }

        public void WriteByte(byte value)
        {
            EnsureBufferIsOpen();
            EnsureBufferIsWriteable();

            Int32 newPosition = _position + 1;

            if (newPosition < 0)
            {
                // We have overflowed report to the user that we cannot write anymore.
                throw new IOException("Cannot write bytes expand past the maximum Buffer length.");
            }

            if (newPosition > _length)
            {
                if (newPosition > _capacity)
                {
                    ExpandCapacity(newPosition);
                }

                _length = newPosition;
            }

            _buffer[++_position] = value;
        }

        /// <summary>
        /// Reads the remaining data into a new readonly <see cref="ByteBuffer"/> without copying data.
        /// buffer.
        /// </summary>
        /// <returns>buffer</returns>
        /// /// <exception cref="EndOfBufferException">There is no more data in the buffer</exception>
        public ByteBuffer ReadSlice()
        {
            EnsureBufferIsOpen();
            CheckForEndOfBuffer();

            var buf = new ByteBuffer(_buffer, _position, _length - _position, isReadonly: true);
            _position = Length - 1;

            return buf;
        }

        public ByteBuffer ReadSlice(Int32 count)
        {
            EnsureCanRead(count);
            _position += count;

            return new ByteBuffer(_buffer, _position, count, isReadonly: true);
        }

        /// <summary>
        /// Creates a new readonly <see cref="ByteBuffer"/> without copying data, without updating the buffers
        /// position.  The new buffer will contain data from the existing buffer starting at offset with a
        /// length and capacity of count.
        /// </summary>
        /// <param name="offset">The offset in the buffer to slice at (set as the new origin)</param>
        /// <param name="count">The number of bytes to include in the new buffer</param>
        /// <returns>buffer</returns>
        /// <exception cref="EndOfBufferException">There is no more data in the buffer</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If offset or count is less than 0, if the offset is greater than the buffer
        /// length, or if the offset + count is greater than or equal to the buffer length.
        /// </exception>
        public ByteBuffer Slice(Int32 offset, Int32 count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot slice buffer at offset {offset}");
            }
            else if (count < 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot slice buffer with a negative count '{count}'");
            }
            else if (offset >= Length - 1)
            {
                throw new ArgumentOutOfRangeException("Cannot slice buffer with offset greater than buffer length");
            }
            else if ((offset + count) >= Length)
            {
                throw new ArgumentOutOfRangeException($"Cannot slice buffer offset + count > length={_length}");
            }

            CheckForEndOfBuffer();

            return new ByteBuffer(_buffer, offset + _startIndex, count, isReadonly: true);
        }

        #region IDisposable

        public void Dispose()
        {
            _isOpen = false;
            _isExpandable = false;
            _readOnly = true;
        }

        #endregion

        private void EnsureBufferIsWriteable()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Buffer is readonly writes are not supported");
            }
        }

        private void CheckForEndOfBuffer()
        {
            if (_position >= _length)
            {
                throw new EndOfBufferException();
            }
        }

        private void EnsureCanRead(Int32 bytes)
        {
            EnsureBufferIsOpen();
            CheckForEndOfBuffer();

            if (RemainingBytes < bytes)
            {
                throw new IOException($"Buffer remaining capacity is less than '{bytes}' bytes");
            }
        }

        private void EnsureBufferIsOpen()
        {
            if (!_isOpen)
            {
                throw new InvalidOperationException("Buffer has been closed, unable to perform buffer I/0");
            }
        }

        private void ExpandCapacity(Int32 expandTo)
        {
            if (expandTo > _capacity)
            {
                Int32 newCapacity = expandTo < 256 ? 256 : expandTo; // Expand by a minumum of 256 bytes;
                newCapacity = newCapacity < (_capacity * 2) ? _capacity * 2 : newCapacity; // Make sure to expand by at least twice the size.

                // Handle the case where new capacity causes an Int32 overflow.
                newCapacity = (newCapacity < 0) ? MAX_BUFFER_LENGTH : newCapacity;

                Capacity = newCapacity;
            }
        }
    }
}
