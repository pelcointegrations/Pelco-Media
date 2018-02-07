//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using Pelco.Media.Pipeline;
using Pelco.Media.RTSP;
using Pelco.Media.RTSP.Client;
using System;
using System.Collections.Generic;

namespace Pelco.Media.RTP
{
    public class RtpSession : IDisposable
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private readonly object SessionLock = new object();

        private bool _started;
        private bool _disposed;
        private IRtpSource _source;

        public RtpSession(MediaTrack track, Session session, IRtpSource source)
        {
            _started = false;
            _disposed = false;
            _source = source;
            Pipelines = new List<MediaPipeline>();

            Track = track;
            Session = session;
        }

        #region Properties

        public string ID { get { return Session.ID; } }

        public Session Session { get; private set; }

        public MediaTrack Track { get; private set; }

        public bool Paused { get; private set; }

        public List<MediaPipeline> Pipelines { get; private set; }

        #endregion

        public void Start()
        {
            lock (SessionLock)
            {
                if (Paused)
                {
                    LOG.Info($"Un-pausing RtpSession '{ID}' ");

                    // Let un-pause if we are currently paused.
                    Pipelines.ForEach(p => p.SetFlushing(false));
                    Paused = false;

                    return;
                }

                if (_started)
                {
                    return;
                }

                _source.Start();

                Pipelines.ForEach(p => p.Start());

                _started = true;
            }
        }

        public void Stop()
        {
            lock (SessionLock)
            {
                try
                {
                    if (!_started)
                    {
                        return;
                    }

                    _source?.Stop();

                    Pipelines.ForEach(p => p.Stop());
                    Pipelines.Clear();
                }
                catch (Exception e)
                {
                    LOG.Error($"Failure occured while shutting down session '{ID}', reason: {e.Message}");
                }
            }
        }

        public void Pause()
        {
            lock (SessionLock)
            {
                LOG.Info($"Pausing RtpSession '{ID}'");

                Paused = true;
                Pipelines.ForEach(p => p.SetFlushing(true));
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
                    LOG.Debug($"Disposing RtpSession '{ID}'");
                    Stop();
                }

                _disposed = true;
            }
        }
    }
}
