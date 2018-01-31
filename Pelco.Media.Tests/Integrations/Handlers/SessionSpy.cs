using System.Collections.Concurrent;

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

        public SessionData? GetData(string sessionId)
        {
            SessionData? data = null;
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

        public void IncrementBy(string sessionId, int count, int numBytes)
        {
            if (_data.ContainsKey(sessionId))
            {
                var data = _data[sessionId];
                data.TotalPacketsSent += count;
                data.TotalBytesSent += numBytes;
            }
        }
    }

    public struct SessionData
    {
        public long TotalBytesSent { get; set; }

        public int TotalPacketsSent { get; set; }
    }

}
