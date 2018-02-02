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
    /// Abstract base sink class.
    /// </summary>
    public abstract class SinkBase : ISink
    {
        /// <summary>
        /// <see cref="ISink.UpstreamLink"/>
        /// </summary>
        public ISource UpstreamLink { get; set; }

        /// <summary>
        /// <see cref="ISink.PushEvent(MediaEvent)"/>
        /// </summary>
        /// <param name="e"></param>
        public void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }

        /// <summary>
        /// <see cref="ISink.Stop"/>
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// <see cref="ISink.WriteBuffer(ByteBuffer)"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public abstract bool WriteBuffer(ByteBuffer buffer);
    }
}
