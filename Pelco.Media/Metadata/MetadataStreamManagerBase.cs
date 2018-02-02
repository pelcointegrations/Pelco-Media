//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using Pelco.Media.Metadata.Api;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Pelco.Media.Metadata
{
    public abstract class MetadataStreamManagerBase : IMetadataStreamManager, IDisposable
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        protected ConcurrentDictionary<string, IMetadataStream> _streams;

        protected MetadataStreamManagerBase()
        {
            _streams = new ConcurrentDictionary<string, IMetadataStream>();
        }

        public virtual async Task JumpAllToLive()
        {
            await Task.WhenAll(_streams.Values.Select(s =>
            {
                try
                {
                    return s.JumpToLive();
                }
                catch (Exception e)
                {
                    LOG.Error($"Failed to jump stream '{s.RtspEndpoint}' to live, reason: {e.Message}");
                }

                return Task.CompletedTask;
            }).ToArray());
        }

        public virtual async Task JumpToLive(string streamId)
        {
            if (_streams.ContainsKey(streamId))
            {
                try
                {
                    await _streams[streamId].JumpToLive();
                }
                catch (Exception e)
                {
                    LOG.Error($"Failed to jump stream '{streamId}' to live, reason: {e.Message}");
                }
            }
        }

        public virtual async Task OnPlayBackControlUpdate(DateTime anchorTime, DateTime initiationTime, double scale)
        {
            await Task.WhenAll(_streams.Values.Select(s =>
            {
                try
                {
                    return s.Seek(anchorTime);
                }
                catch (Exception e)
                {
                    LOG.Error($"Failed to seek stream '{s.RtspEndpoint}' to live, reason: {e.Message}");
                }

                return Task.CompletedTask;
            }).ToArray());
        }

        public virtual async Task<string> RegisterStream(IMetadataStream stream, DateTime? startTime = null)
        {
            if (!stream.IsRunning)
            {
                await stream.Start(startTime);
            }

            var id = Guid.NewGuid().ToString();
            _streams.TryAdd(id, stream);

            return await Task.FromResult(id);
        }

        public Task<string> RegisterStream(IMetadataStream stream)
        {
            throw new NotImplementedException();
        }

        public virtual async Task Stop(string streamId)
        {
            if (_streams.ContainsKey(streamId))
            {
                try
                {
                    await _streams[streamId].Stop();
                }
                catch (Exception e)
                {
                    LOG.Error($"Failed to stop stream '{streamId}', reason: {e.Message}");
                }
            }
        }

        public virtual async Task StopAll()
        {
            await Task.WhenAll(_streams.Values.Select(s => s.Stop()).ToArray());
        }

        public async void Dispose()
        {
            await StopAll();
            _streams.Clear();
        }
    }
}
