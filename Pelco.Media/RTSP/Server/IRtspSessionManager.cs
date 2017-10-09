using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pelco.Media.RTSP.Server
{
    public interface IRtspSessionManager
    {
        /// <summary>
        /// Starts the session manager.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the session manager.
        /// </summary>
        void Stop();

        /// <summary>
        /// Register a new session to be managed.
        /// </summary>
        /// <param name="session">The session to register</param>
        /// <returns></returns>
        bool RegisterSession(IRtspSession session);

        /// <summary>
        /// Play (start) an <see cref="IRtspSession"/>
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        bool PlaySession(string sessionId);

        /// <summary>
        /// Refreshes an existing <see cref="IRtspSession"/>
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        bool RefreshSession(string sessionId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        bool TearDownSession(string sessionId);
    }
}
