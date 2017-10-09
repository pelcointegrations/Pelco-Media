using System.Net;

namespace Pelco.Media.RTSP
{
    /// <summary>
    /// Interfaced for an RTSP connteciton.
    /// </summary>
    public interface IRtspConnection
    {
        /// <summary>
        /// Gets flag indicating if the <see cref="IRtspConnection"/> is connected or not.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the connection's IP endpoint.
        /// </summary>
        IPEndPoint Endpoint { get; }

        /// <summary>
        /// Gets the connection's remote address.
        /// </summary>
        string RemoteAddress { get; }

        /// <summary>
        /// Determines if the connection can be read from.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// Determines if the connection can be written to.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Reads a byte from the connection
        /// </summary>
        /// <returns>The number of bytes to read from the connection</returns>
        /// <exception cref="IOException">If the underlying socket is closed.</exception>
        /// <exception cref="ObjectDisposedException">
        /// If the underlying socket is closed or if there is a read failure
        /// </exception>
        int ReadByte();

        /// <summary>
        /// Reads data form the RtspConnection.
        /// </summary>
        /// <remarks>The read operation will return zero.</remarks>
        /// <param name="buffer">An array of bytes where data should be read into.</param>
        /// <param name="offset">The location in the buffer to start reading at.</param>
        /// <param name="size">The number of bytes to read</param>
        /// <returns>The number of bytes to read from the connection</returns>
        /// <exception cref="IOException">If the underlying socket is closed.</exception>
        /// <exception cref="ObjectDisposedException">
        /// If the underlying socket is closed or if there is a read failure
        /// </exception>
        int Read(byte[] buffer, int offset, int size);

        /// <summary>
        /// Writes a byte to the RTSP connection.
        /// </summary>
        /// <param name="value">The bute to write</param>
        /// <exception cref="IOException">If an I/O error occurs</exception>
        /// <exception cref="ObjectDisposedException">If called after the underlying socket is closed</exception>
        void WriteByte(byte value);

        /// <summary>
        /// Writes data to the RTSP connection
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write</param>
        /// <param name="offset">The location in the buffe to start writing data from</param>
        /// <param name="size">The number of bytes to write</param>
        /// <exception cref="IOException">If an I/O error occurs</exception>
        /// <exception cref="ObjectDisposedException">If called after the underlying socket is closed</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the offset param is 0 or The size param is 0 or the size param is greater than the length of
        /// the buffer minus the value of the offset param.
        /// </exception>
        void Write(byte[] buffer, int offset, int size);

        /// <summary>
        /// Writes an <see cref="RtspMessage"/>. One of <see cref="RtspRequest request"/> or
        /// <see cref="RtspResponse"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns>True if the write was successful; otherwise, false is returned</returns>
        /// <exception cref="ArgumentNullException">If the rtsp request is null</exception>
        bool WriteMessage(RtspMessage msg);

        /// <summary>
        /// Closes the <see cref="IRtspConnection"/> instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Reconnects the <see cref="IRtspConnection"/> if it is not currently connected.
        /// </summary>
        /// <remarks>If the transport is already connected then this method should do nothing.</remarks>
        /// <exception cref="System.Net.Sockets.SocketException">If an error occurs while reconnecting</exception>
        void Reconnect();
    }
}
