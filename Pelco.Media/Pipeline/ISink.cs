namespace Pelco.Media.Pipeline
{
    /// <summary>
    /// Pipeline interface for receiving data from a pipeline.
    /// </summary>
    public interface ISink
    {
        /// <summary>
        /// Writes the buffer.
        /// </summary>
        /// <param name="stream">Buffer containing data</param>
        /// <returns>True if the buffer is accepted and processed down stream; otherwsie, False</returns>
        bool WriteBuffer(ByteBuffer buffer);

        /// <summary>
        /// Called before the pipeline is destructd.  This method can be used to
        /// perform resource cleanup.
        /// </summary>
        void Stop();
    }
}
