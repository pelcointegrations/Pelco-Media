using System;

namespace Pelco.Media.RTSP.Server
{
    /// <summary>
    /// Base <see cref="IRtspSession"/> implementation
    /// </summary>
    public abstract class RtspSessionBase : IRtspSession
    {
        public static readonly uint RTSP_SESSION_TIMEOUT = 600; // 10 minutes

        private DateTime _refreshedTime;

        public RtspSessionBase()
        {
            var rand = new Random();

            Id = Guid.NewGuid().ToString();
            SSRC = (uint)rand.Next(0, int.MaxValue);

            _refreshedTime = DateTime.Now;
        }

        /// <summary>
        /// <see cref="IRtspSession.Id"/>
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// <see cref="IRtspSession.SSRC"/>
        /// </summary>
        public uint SSRC { get; private set; }

        /// <summary>
        /// <see cref="IRtspSession.IsExpired"/>
        /// </summary>
        public bool IsExpired
        {
            get
            {
                return (DateTime.Now - _refreshedTime).TotalSeconds > RTSP_SESSION_TIMEOUT;
            }
        }

        /// <summary>
        /// <see cref="IRtspSession.Refresh"/>
        /// </summary>
        public void Refresh()
        {
            _refreshedTime = DateTime.Now;
        }

        /// <summary>
        /// <see cref="IRtspSession.Start"/>
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// <see cref="IRtspSession.Stop"/>
        /// </summary>
        public abstract void Stop();
    }
}
