//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Pipeline;
using Pelco.Media.Pipeline.Transforms;
using System;

namespace Pelco.Media.Metadata
{
    /// <summary>
    /// A MetadataPlayer is responsible for managing a <see cref="VxMetadataSource"/>, processing
    /// the provided metadata, synchronizing it to an associated video stream, and providing it
    /// to a user provided view.
    /// 
    /// Becasuse not all metadata is equal users will be responsible for defining their own metadata
    /// processing, as well as defining how to present it to the user.
    /// 
    /// The MetadataPlayer provides support for seeking to and playing recorded metadata, pausing a stream, as well
    /// as playing a live metadata stream.
    /// </summary>
    public class VxMetadataPlayer : IDisposable
    {
        private static readonly object PlayerLock = new object();

        private bool _isLive;
        private bool _disposed;
        private bool _initialized;
        private MediaPipeline _pipeline;
        private VxMetadataSource _source;
        private PlayerConfiguration _config;
        private ITransform _transformSource;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The player's configuration</param>
        public VxMetadataPlayer(PlayerConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException("Cannot configure metadata player with null configuration");

            if (_config.Uri == null)
            {
                throw new ArgumentException("Player configuration must provide an RTSP uri.");
            }
            else if (_config.PipelineCreator == null)
            {
                throw new ArgumentException("Player configuration must provide a IPipelineCreator.");
            }

            _isLive = false;
            _disposed = false;
            _initialized = false;
            _source = new VxMetadataSource(_config.Uri, _config.Creds);
        }

        /// <summary>
        /// Initializes the player. Initialization consists of initializing the <see cref="VxMetadataSource"/>
        /// and selecting the source's media (track) to play from.
        /// </summary>
        /// <exception cref="TimeoutException">If RTSP request timedout waiting for response from server</exception>
        /// <exception cref="RtspClientException">If an error occurs while talking to the RTSP server</exception>
        /// <exception cref="PlayerInitializationException">If the player could not be initialized</exception>
        public virtual void Initialize()
        {
            lock (PlayerLock)
            {
                if (_initialized)
                {
                    return; // Already initialized
                }


                _source.Initialize();

                var tracks = _source.GetTracks();
                if (tracks.IsEmpty)
                {
                    throw new PlayerInitializationException("Unable to initialize player, source does not contain any media tracks");
                }

                _initialized = true;
            }
        }

        public void Start(DateTime? playAt = null)
        {
            lock (PlayerLock)
            {
                if (_pipeline != null)
                {
                    // Already started.
                    return;
                }

                _transformSource = new RtpPayloadTransform();

                _isLive = !playAt.HasValue;
                _source.Play(_transformSource, playAt);

                _pipeline = _config.PipelineCreator.CreatePipeline(_transformSource, _isLive);

                _pipeline.Start();
            }
        }

        public void Seek(DateTime seekTo)
        {
            lock (PlayerLock)
            {
                if (_transformSource == null)
                {
                    throw new ArgumentException("Unable to seek before starting player");
                }

                if (_isLive)
                {
                    _isLive = false;
                    _pipeline.SetFlushing(true);
                    _pipeline.Stop();

                    _pipeline = _config.PipelineCreator.CreatePipeline(_transformSource, _isLive);
                    _pipeline.Start();
                }

                _source.Seek(seekTo);
            }
        }

        public void Pause()
        {
            lock (PlayerLock)
            {
                _pipeline.SetFlushing(true);
                _source.Pause();
            }
        }

        public void UnPause()
        {
            lock (PlayerLock)
            {
                _pipeline.SetFlushing(false);
                _source.UnPause();
            }
        }

        public void JumpToLive()
        {
            lock (PlayerLock)
            {
                if (!_isLive)
                {
                    _isLive = true;

                    _pipeline.SetFlushing(true);
                    _pipeline.Stop();

                    _pipeline = _config.PipelineCreator.CreatePipeline(_transformSource, _isLive);
                    _pipeline.SetFlushing(false);
                    _pipeline.Start();

                    _source.JumpToLive();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _initialized = false;
                }

                _pipeline?.Stop();
                _source?.Dispose();

                _disposed = true;
            }
        }
    }
}
