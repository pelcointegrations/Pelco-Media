namespace Pelco.PDK.Media.Pipeline
{
    /// <summary>
    /// Pipeline union interface for Source and Sink.
    /// </summary>
    public interface ITransform : ISource, ISink
    {
    }
}
