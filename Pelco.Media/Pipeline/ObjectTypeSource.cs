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
    /// Object pipeline source.  Creates an pushes objects up stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectTypeSource<T> : ISource
    {
        private readonly object FlushingLock = new object();

        private volatile bool _isFlushing;
        private ISink _downstreamLink;
        private ISource _upstreamLink;

        /// <summary>
        /// <see cref="ISource.DownstreamLink"/>
        /// </summary>
        public ISink DownstreamLink
        {
            get
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
                return _isFlushing;
            }

            set
            {
                lock (FlushingLock)
                {
                    _isFlushing = value;
                }
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
        /// Pushes an object upstream.
        /// </summary>
        /// <param name="obj">object to push.</param>
        /// <returns></returns>
        protected virtual bool PushObject(T obj)
        {
            lock (FlushingLock)
            {
                if (!Flushing && DownstreamLink != null)
                {
                    if (DownstreamLink is IObjectTypeSink<T>)
                    {
                        return ((IObjectTypeSink<T>)DownstreamLink).HandleObject(obj);
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// <see cref="ISource.OnMediaEvent(MediaEvent)"/>
        /// </summary>
        /// <param name="e"></param>
        public void OnMediaEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }
    }
}
