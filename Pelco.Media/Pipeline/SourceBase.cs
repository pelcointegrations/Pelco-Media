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
    /// Source base class.
    /// </summary>
    public class SourceBase : ISource
    {
        private readonly object _flushing_lock = new object();

        private volatile bool _isFlushing;
        private ISink _downstreamLink;
        private ISource _upstreamLink;

        /// <summary>
        /// <see cref="ISource.DownstreamLink"/>
        /// </summary>
        public ISink DownstreamLink
        {
            private get
            {
                return _downstreamLink;
            }

            set
            {
                _downstreamLink = value;
            }
        }

        /// <summary>
        /// <see cref="ISource.UpstreamLink"/>
        /// </summary>
        public ISource UpstreamLink
        {
            get
            {
                return _upstreamLink;
            }

            set
            {
                _upstreamLink = value;
            }
        }

        /// <summary>
        /// <see cref="ISource.Flushing"/>
        /// </summary>
        public bool Flushing
        {
            get
            {
                lock (_flushing_lock)
                {
                    return _isFlushing;
                }
            }

            set
            {
                lock (_flushing_lock)
                {
                    _isFlushing = false;
                }

            }
        }

        /// <summary>
        /// Pushes the buffer downstream if a DownstreamLink is set.
        /// </summary>
        /// <param name="stream">The buffer to push</param>
        /// <returns>True if the buffer is accepted and processed down stream; otherwsie, False</returns>
        protected bool PushBuffer(ByteBuffer buffer)
        {
            lock (_flushing_lock)
            {
                if (!Flushing && DownstreamLink != null)
                {
                    buffer.MarkReadOnly(); // Ensure it is readonly before sending down stream.
                    return DownstreamLink.WriteBuffer(buffer);
                }

                return true;
            }
        }

        /// <summary>
        /// <see cref="ISource.Start"/>
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        /// <see cref="ISource.Stop"/>
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// <see cref="ISource.OnMediaEvent(MediaEvent)"/>
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMediaEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }
    }
}
