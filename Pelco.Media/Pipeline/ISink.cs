//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
namespace Pelco.Media.Pipeline
{
    /// <summary>
    /// Pipeline interface for receiving data from a pipeline.
    /// </summary>
    public interface ISink
    {
        /// <summary>
        /// Sets the upstream pipline link.  The upstream link is used to pass
        /// events downstream.
        /// </summary>
        ISource UpstreamLink { set; }

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

        /// <summary>
        /// Pushes a media event downstream.
        /// </summary>
        /// <param name="e"></param>
        void PushEvent(MediaEvent e);
    }
}
