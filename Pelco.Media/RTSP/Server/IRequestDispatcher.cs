//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
namespace Pelco.Media.RTSP.Server
{
    /// <summary>
    /// Dispatches an RTSP Request.
    /// </summary>
    public interface IRequestDispatcher
    {
        /// <summary>
        /// Allows extensions to initialize resources if requried.
        /// </summary>
        void Init();

        /// <summary>
        /// Allows extensions to closed created resources if required.
        /// </summary>
        void Close();

        /// <summary>
        /// Dispatch an <see cref="RtspRequest"/>.
        /// </summary>
        /// <param name="request">The request to dispatch</param>
        /// <returns></returns>
        RtspResponse Dispatch(RtspRequest request);
    }
}
