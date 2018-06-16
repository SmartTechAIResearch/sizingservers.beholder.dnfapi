using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace sizingservers.beholder.dnfapi.DA {
    public static class AgentRequestReport {

        /// <summary>
        /// Pings the specified hostname. 5 minutes send and receive timeout.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public static bool RequestReport(string hostname, int port) {
            bool success = false;

            try {
                var tcpClient = new TcpClient();

                //Remove WORKGROUP or NETWORK from the hostname 
                tcpClient.Connect(hostname.Split('.')[0], port);

                if (tcpClient.Connected) {
                    tcpClient.SendTimeout = tcpClient.ReceiveTimeout = 360000;
                    var sw = new StreamWriter(tcpClient.GetStream());
                    var sr = new StreamReader(tcpClient.GetStream());

                    sw.Write("requestreport\r\n");
                    sw.Flush();
                    success = (sr.ReadLine().Trim().ToLowerInvariant() == "requestreport");
                    try {
                        tcpClient.Close();
                    }
                    catch { }
                }
                tcpClient = null;
            }
            catch {
                //Do not care. Report the agent as being unresponsive.
            }

            return success;
        }
    }
}