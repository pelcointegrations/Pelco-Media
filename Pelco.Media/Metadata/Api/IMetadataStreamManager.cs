//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;
using System.Threading.Tasks;

namespace Pelco.Media.Metadata.Api
{
    public interface IMetadataStreamManager : IDisposable
    {
        /// <summary>
        /// Registers metadata stream to be managed. If the stream has not been started
        /// it will be started before it is registered.
        /// </summary>
        /// <param name="stream">The stream to register</param>
        /// <param name="startTime">The time to start streaming at</param>
        /// <returns></returns>
        Task<string> RegisterStream(IMetadataStream stream, DateTime? startTime = null);

        /// <summary>
        /// Stops the stream associated with the provided stream id.
        /// </summary>
        /// <param name="streamId">The stream id to stop.</param>
        /// <returns></returns>
        Task Stop(string streamId);

        /// <summary>
        /// Stops All currently running metadata streams.
        /// </summary>
        /// <returns></returns>
        Task StopAll();

        /// <summary>
        /// Jumps the stream associated with the provide stream id to a live stream.
        /// </summary>
        /// <param name="streamId">The stream id to jump to live.</param>
        /// <returns></returns>
        Task JumpToLive(string streamId);

        /// <summary>
        /// Jumps all registered metadata streams to a live stream.
        /// </summary>
        /// <returns></returns>
        Task JumpAllToLive();

        /// <summary>
        /// Callback handler to call when a playback event such as a seek or scale update occurs.
        /// </summary>
        /// <param name="anchorTime"></param>
        /// <param name="initiationTime"></param>
        /// <param name="scale">The streams scale (i.e. -4.0,-2.0,-1.0,0,1.0,2.0, etc...)</param>
        /// <returns></returns>
        Task OnPlayBackControlUpdate(DateTime anchorTime, DateTime initiationTime, double scale);
    }
}
