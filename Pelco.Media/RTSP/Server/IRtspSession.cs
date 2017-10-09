namespace Pelco.Media.RTSP.Server
{
    /// <summary>
    /// Represents an RTSP session.
    /// </summary>
    public interface IRtspSession
    {
        /// <summary>
        /// Gets the session's id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the session's SSSRC.
        /// </summary>
        uint SSRC { get; }

        /// <summary>
        /// Indicates if the session has expired or not.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// Starts the session.  This method allows implementors to create resources
        /// required by the session.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the session.  This method allows implementors to clean up any
        /// created resources.
        /// </summary>
        void Stop();

        /// <summary>
        /// Refreshes the session.
        /// </summary>
        void Refresh();
    }
}
