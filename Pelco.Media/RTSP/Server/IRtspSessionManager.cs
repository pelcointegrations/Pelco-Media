//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System.Collections.Generic;

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
        /// Tearsdown an existing session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        bool TearDownSession(string sessionId);

        /// <summary>
        /// Retrieves a list dictionary of session properties. This method
        /// can usually be used to retrieve session information.  Usually
        /// when making a GET_PARAMETER call.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>A dictionary of key/value pairs.</returns>
        Dictionary<string, object> GetSessionProperties(string sessionId);
    }
}
