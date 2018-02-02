//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.RTSP.Client;
using Pelco.Media.RTSP.Server;
using Pelco.Media.Tests.Utils;
using System;
using System.Threading;

namespace Pelco.Media.Tests.Integrations
{
    public class RtspCommunicationFixture : IDisposable
    {
        private const string GlobalMutexId = "Global\\{387cc644-6041-4eaa-8e9b-6fed2c6bcbab}";

        private int _cseq;
        private string _path;
        private RtspServer _server;

        public RtspCommunicationFixture()
        {
            Initialized = false;
        }

        public void Initialize(string path, IRequestHandler handler)
        {
            using (var mutex = new Mutex(false, GlobalMutexId))
            {
                bool mutexAcquired = false;

                try
                {
                    // Because the tests can run on multiple threads we must synchronize
                    // to ensure that we don't start different test servers on the same port.
                    if ((mutexAcquired = mutex.WaitOne(5000, false)))
                    {
                        if (!Initialized)
                        {
                            ServerPort = NetworkUnil.FindAvailableTcpPort();

                            var dispatcher = new DefaultRequestDispatcher();
                            dispatcher.RegisterHandler(path, handler);

                            _path = path;
                            _server = new RtspServer(ServerPort, dispatcher);
                            _server.Start();

                            // Wait until the serer port is not available.
                            while (NetworkUnil.IsTcpPortAvailable(ServerPort))
                            {
                                Thread.Sleep(1000);
                            }

                            Client = new RtspClient(ServerUriEndpoint);

                            Initialized = true;
                        }
                    }
                }
                catch (AbandonedMutexException)
                {
                    // do nothing
                }
                catch (Exception)
                {
                    // Do nothing since this is just a test, and if we fail here the tests
                    // are going to fail also.
                }
                finally
                {
                    if (mutexAcquired)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        public bool Initialized { get; private set; }

        public RtspClient Client { get; private set; }

        public int ServerPort { get; private set; }

        public Uri ServerUriEndpoint
        {
            get
            {
                return new Uri($"rtsp://127.0.0.1:{ServerPort}{_path}");
            }
        }

        public int NextCseq()
        {
            return Interlocked.Increment(ref _cseq);
        }

        public void Dispose()
        {
            Client?.Close();
            _server?.Stop();

            while (!NetworkUnil.IsTcpPortAvailable(ServerPort))
            {
                Thread.Sleep(1000);
            }
        }
    }
}
