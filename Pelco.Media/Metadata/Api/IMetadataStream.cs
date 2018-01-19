using System;
using System.Threading.Tasks;

namespace Pelco.Media.Metadata.Api
{
    public interface IMetadataStream
    {
        /// <summary>
        /// Gets the RTSP endpoint of the metadata stream.
        /// </summary>
        Uri RtspEndpoint { get; }

        /// <summary>
        /// Gets flag indicating if the stream is the live stream or not. If
        /// not then it is assumed the stream is a recorded playback stream.
        /// </summary>
        bool IsLive { get; }

        /// <summary>
        /// Gets flag indicating if the stream is currenting running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Start receiving the metadata stream.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        Task Start(DateTime? startTime = null);

        /// <summary>
        /// Stop a started metadata stream.
        /// </summary>
        /// <returns></returns>
        Task Stop();

        /// <summary>
        /// Jump to the live stream.  If the stream is already live then this
        /// call will have no affect.
        /// </summary>
        /// <returns></returns>
        Task JumpToLive();

        /// <summary>
        /// Seek the metadata stream to the closest metadata associated with the 
        /// </summary>
        /// <param name="seekTo"></param>
        /// <returns></returns>
        Task Seek(DateTime seekTo);
    }
}
