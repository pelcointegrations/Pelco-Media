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
    /// Abstract base class used to define a transform from a <see cref="ByteBuffer"/>
    /// to a specific object type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BufferToObjectTypeTransformBase<T> : ObjectTypeSource<T>, ITransform
    {
        public new void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// <see cref="ISink.PushEvent(MediaEvent)"/>
        /// </summary>
        /// <param name="e"></param>
        public virtual void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }

        /// <summary>
        /// <see cref="ISink.WriteBuffer(ByteBuffer)"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public abstract bool WriteBuffer(ByteBuffer buffer);
    }
}
