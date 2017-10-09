using System.Net.NetworkInformation;

namespace Pelco.Media.Tests.Utils
{
    public class NetworkUnil
    {
        public static int FindAvailableTcpPort(int startPort = 4500)
        {
            int port = startPort;
            while (!IsTcpPortAvailable(port++)) ;

            return port;
        }

        public static bool IsTcpPortAvailable(int port)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (var endpoint in tcpConnInfoArray)
            {
                if (endpoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;

        }
    }
}
