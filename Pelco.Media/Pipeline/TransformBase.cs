namespace Pelco.Media.Pipeline
{
    public abstract class TransformBase : SourceBase, ITransform
    {
        /// <summary>
        /// Writes the buffer.
        /// </summary>
        /// <param name="stream">Buffer containing data</param>
        /// <returns>True if the buffer is accepted and processed down stream; otherwsie, False</returns>
        public abstract bool WriteBuffer(ByteBuffer buffer);
    }
}
