//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using Pelco.Media.Common;
using Pelco.Media.Metadata.Api;
using System;
using System.Threading.Tasks;

namespace Pelco.Media.Metadata
{
    public abstract class MetadataStreamBase : IMetadataStream, IDisposable
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private bool _disposed;
        private MimeType _filter;
        private VxMetadataPlayer _player;

        protected MetadataStreamBase(MimeType filter, Uri rtspEndpoint)
        {
            _disposed = false;
            _filter = filter ?? MimeType.ANY_APPLICATION;

            RtspEndpoint = rtspEndpoint ?? throw new ArgumentNullException("rtspEndpoint cannot be null");
        }

        ~MetadataStreamBase()
        {
            Dispose(false);
        }

        public Uri RtspEndpoint { get; private set; }

        public bool IsLive { get; private set; }

        public bool IsRunning { get; private set; }

        public abstract IPipelineFactory GetPipelineFactory();

        public async Task JumpToLive()
        {
            if (IsLive)
            {
                return;
            }

            try
            {
                await Task.Run(() => _player.JumpToLive());

                IsLive = true;
            }
            catch (Exception e)
            {
                LOG.Error($"Received error trying to jump to live stream, reason={e.Message}");
            }
        }

        public async Task Seek(DateTime seekTo)
        {
            try
            {
                await Task.Run(() => _player.Seek(seekTo));
            }
            catch (Exception e)
            {
                LOG.Error($"Received error trying to seek metadata stream, reason={e.Message}");
            }
        }

        public async Task Start(DateTime? startTime = null)
        {
            try
            {
                await Task.Run(() => ConfigureAndStartPlayer(RtspEndpoint, startTime));
                IsLive = !startTime.HasValue;
            }
            catch (Exception e)
            {
                LOG.Error($"Received error trying to play metadata stream, reason={e.Message}");
            }
        }

        public async Task Stop()
        {
            try
            {
                await Task.Run(() => _player.Dispose());
            }
            catch (Exception e)
            {
                LOG.Error($"Received error shuting down metadata player, reason={e.Message}");
            }
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _player?.Dispose();   
                _disposed = true;
            }
        }

        private void ConfigureAndStartPlayer(Uri dataEndpoint, DateTime? startTime)
        {
            lock (this)
            {
                try
                {
                    var config = new PlayerConfiguration()
                    {
                        PipelineFactory = GetPipelineFactory(),
                        TypeFilter = _filter,
                        Uri = dataEndpoint
                    };

                    _player = new VxMetadataPlayer(config);

                    _player.Initialize();
                    _player.Start(startTime);

                    LOG.Info($"Started metadata player with RTSP endpoint '{RtspEndpoint}'");
                }
                catch (Exception e)
                {
                    LOG.Error($"Unable to initialize metadata player for  '{RtspEndpoint}', reason={e.Message}");
                }
            }
        }
    }
}
