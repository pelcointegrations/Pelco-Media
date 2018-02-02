//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Pelco.Media.RTSP.Server
{
    public class RtspSessionManager : IDisposable, IRtspSessionManager
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private Timer _refreshTimer;
        private ConcurrentDictionary<string, IRtspSession> _sessions;

        public RtspSessionManager()
        {
            _refreshTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _refreshTimer.AutoReset = true;
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;

            _sessions = new ConcurrentDictionary<string, IRtspSession>();
        }

        /// <summary>
        /// <see cref="IRtspSessionManager.Start"/>
        /// </summary>
        public void Start()
        {
            _refreshTimer.Start();

            LOG.Info("RTSP session manager started");
        }

        /// <summary>
        /// <see cref="IRtspSessionManager.Stop"/>
        /// </summary>
        public void Stop()
        {
            Dispose();

            foreach (var session in _sessions)
            {
                try
                {
                    session.Value.Stop();

                    LOG.Debug($"Stopped RTSP session '{session.Value.Id}'");
                }
                catch (Exception e)
                {
                    LOG.Info(e, $"Caught exception while shutting down session, msg={e.Message}");
                }
            }
            _sessions.Clear();

            LOG.Info("Successfully shutdown RTSP session manager.");
        }

        /// <summary>
        /// <see cref="IRtspSessionManager.RegisterSession(IRtspSession)"/>
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool RegisterSession(IRtspSession session)
        {
            LOG.Debug($"Registering new RTSP session '{session.Id}' of type '{session.GetType().Name}'");

            return _sessions.TryAdd(session.Id, session);
        }

        /// <summary>
        /// <see cref="IRtspSessionManager.PlaySession(string)"/>
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public bool PlaySession(string sessionId)
        {
            if (_sessions.ContainsKey(sessionId))
            {
                LOG.Debug($"Playing (starting) RTSP session '{sessionId}'");

                _sessions[sessionId].Start();

                return true;
            }

            return false;
        }

        /// <summary>
        /// <see cref="IRtspSessionManager.RefreshSession(string)"/>
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public bool RefreshSession(string sessionId)
        {
            if (_sessions.ContainsKey(sessionId))
            {
                LOG.Debug($"Refreshing RTSP session '{sessionId}'");

                _sessions[sessionId].Refresh();

                return true;
            }

            return false;
        }

        /// <summary>
        /// <see cref="IRtspSessionManager.TearDownSession(string)"/>
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public bool TearDownSession(string sessionId)
        {
            if (_sessions.ContainsKey(sessionId))
            {
                IRtspSession session = null;
                if (_sessions.TryRemove(sessionId, out session))
                {
                    LOG.Debug($"Tearing down RTSP session '{sessionId}'");

                    session.Stop();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// <see cref="IRtspSessionManager.GetSessionProperties(string)"/>
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetSessionProperties(string sessionId)
        {
            return new Dictionary<string, object>();
        }

        protected IRtspSession GetSession(string sessionId)
        {
            return _sessions.ContainsKey(sessionId) ? _sessions[sessionId] : null;
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Remove the expired sessions.
            _sessions.Where(s => s.Value.IsExpired).ToList().ForEach(session =>
            {
                LOG.Info($"Tearing down expired session '{session.Value.Id}'");
                TearDownSession(session.Value.Id);
            });
        }

        #region IDisposable

        public void Dispose()
        {
            _refreshTimer.Stop();
        }

        #endregion
    }
}
