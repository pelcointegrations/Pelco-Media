namespace Pelco.Media.RTSP.Server
{
    /// <summary>
    /// Dispatches an RTSP Request.
    /// </summary>
    public interface IRequestDispatcher
    {
        /// <summary>
        /// Dispatch an <see cref="RtspRequest"/>.
        /// </summary>
        /// <param name="request">The request to dispatch</param>
        /// <returns></returns>
        RtspResponse Dispatch(RtspRequest request);
    }
}
