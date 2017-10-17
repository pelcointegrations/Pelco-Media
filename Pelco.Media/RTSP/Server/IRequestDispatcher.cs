namespace Pelco.Media.RTSP.Server
{
    /// <summary>
    /// Dispatches an RTSP Request.
    /// </summary>
    public interface IRequestDispatcher
    {
        /// <summary>
        /// Allows extensions to initialize resources if requried.
        /// </summary>
        void Init();

        /// <summary>
        /// Allows extensions to closed created resources if required.
        /// </summary>
        void Close();

        /// <summary>
        /// Dispatch an <see cref="RtspRequest"/>.
        /// </summary>
        /// <param name="request">The request to dispatch</param>
        /// <returns></returns>
        RtspResponse Dispatch(RtspRequest request);
    }
}
