namespace Pelco.PDK.Media.Pipeline
{
    /// <summary>
    /// Pipeline interface for pushing data into a pipeline (downstream)
    /// </summary>
    public interface ISource
    {
        /// <summary>
        /// Sets the downstream pipeline link.  This method can be used to
        /// link multiple pipeline instances together.
        /// </summary>
        ISink DownstreamLink { set; }

        /// <summary>
        /// Gets and sets the flushing flag.  This flag is set when a pipeline state
        /// change occurs.  This allows sources to purge data from the previous state.
        /// </summary>
        bool Flushing { get; set; }

        /// <summary>
        /// Starts the pipeline source.  Usually called before the pipeline data starts flowing.
        /// </summary>
        void Start();

        /// <summary>
        /// Called before the pipeline destruction occurs.  Can be used to clean up resources.
        /// </summary>
        void Stop();
    }
}
