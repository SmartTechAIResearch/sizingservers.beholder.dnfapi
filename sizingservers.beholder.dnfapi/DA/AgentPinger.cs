using System.IO;
using System.Net.Sockets;

namespace sizingservers.beholder.dnfapi.DA {
    public class AgentPinger {

        /// <summary>
        /// Pings the specified hostname. 5 seconds send and receive timeout.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public bool Ping(string hostname, int port) {
            bool pong = false;

            var tcpClient = new TcpClient();
            tcpClient.Connect(hostname, port);
       
            if (tcpClient.Connected) {
                tcpClient.SendTimeout = tcpClient.ReceiveTimeout = 360000;
                var sw = new StreamWriter( tcpClient.GetStream());
                var sr = new StreamReader(tcpClient.GetStream());

                sw.Write("ping\r\n");
                pong = (sr.ReadLine().Trim().ToLowerInvariant() == "pong");
            }

            if (tcpClient != null) {
                try {
                    if (tcpClient.Connected) tcpClient.Close();
                }
                catch { }
                tcpClient = null;
            }

            return pong;
        }
    }
}