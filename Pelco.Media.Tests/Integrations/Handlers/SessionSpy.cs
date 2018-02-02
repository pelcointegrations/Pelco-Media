//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Pipeline;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Pelco.Media.Tests.Integrations.Handlers
{
    public class SessionSpy
    {
        private ConcurrentDictionary<string, SessionData> _data;

        public SessionSpy()
        {
            _data = new ConcurrentDictionary<string, SessionData>();
        }

        public bool ContainsData(string sessionId)
        {
            return _data.ContainsKey(sessionId);
        }

        public SessionData GetData(string sessionId)
        {
            SessionData data = null;
            if (_data.ContainsKey(sessionId))
            {
                data = _data[sessionId];
            }

            return data;
        }

        public void Insert(string sessionId, SessionData data)
        {
            _data.TryAdd(sessionId, data);
        }
    }

    public class SessionData
    {
        private List<ByteBuffer> _buffers = new List<ByteBuffer>();

        public List<ByteBuffer> Buffers
        {
            get
            {
                return _buffers;
            }
        }
    }

}
