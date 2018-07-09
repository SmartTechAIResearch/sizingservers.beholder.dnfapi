/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using SizingServers.Log;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace sizingservers.beholder.dnfapi.Helpers {
    /// <summary>
    /// Polls the vmare sdk and the agents using PollIntervalInSeconds in appsettings.json.
    /// </summary>
    public static class Poller {
        /// <summary>
        /// The default poll interval in seconds
        /// </summary>
        public const int DEFAULT_POLL_INTERVAL_IN_SECONDS = 360;

        private static Timer _pollTimer;

        /// <summary>
        /// Gets the poll interval in seconds.
        /// </summary>
        /// <value>
        /// The poll interval in seconds.
        /// </value>
        public static int PollIntervalInSeconds {
            get {
                int pollIntervalInSeconds = DEFAULT_POLL_INTERVAL_IN_SECONDS;
                try {
                    pollIntervalInSeconds = AppSettings.GetValue<int>("PollIntervalInSeconds");
                }
                catch {
                    Loggers.Log(Level.Warning, "PollIntervalInSeconds not found in appsettings.json. Reverted to DEFAULT_CACHE_EXPIRES_IN_SECONDS (== 10).");
                }
                return pollIntervalInSeconds;
            }
        }
        /// <summary>
        /// Starts polling the vmare sdk and the agents using PollIntervalInSeconds in appsettings.json.
        /// </summary>
        /// <param name="synchronizationContext">The synchronization context for syncing to the main thread for SQLite file access.</param>
        public static void Start() {
            Stop();
            _pollTimer = new Timer(Poll, null, 0, PollIntervalInSeconds * 1000);
        }
        /// <summary>
        /// Stops polling
        /// </summary>
        public static void Stop() {
            if (_pollTimer != null) {
                try { _pollTimer.Dispose(); } catch { }
                _pollTimer = null;
            }
        }

        private static void Poll(object state) {
            PollSystemInformation();
            PollVHSystemInformation();
        }

        /// <summary>
        /// Polls the system information.
        /// </summary>
        /// <returns></returns>
        public static Models.SystemInformation[] PollSystemInformation() {
            Models.SystemInformation[] list = CacheHelper.Get<Models.SystemInformation[]>("systemInformationList");
            if (list == null) {
                list = DA.SystemInformationsDA.GetAll();

                bool responsiveChanged = false;
                Parallel.For(0, list.Length, (i) => {
                    var systemInformation = list[i];
                    int responsive = DA.AgentRequestReport.RequestReport(systemInformation.hostname, systemInformation.requestReportTcpPort) ? 1 : 0;

                    if (systemInformation.responsive != responsive) {
                        systemInformation.responsive = responsive;
                        responsiveChanged = true;
                    }
                });

                if (responsiveChanged)
                    foreach (var row in list) DA.SystemInformationsDA.AddOrUpdate(row);

                CacheHelper.Add("systemInformationList", list);
            }

            return list;
        }
        /// <summary>
        /// Polls the vh system information.
        /// </summary>
        /// <returns></returns>
        public static Models.VMwareHostSystemInformation[] PollVHSystemInformation() {
            Models.VMwareHostSystemInformation[] list = CacheHelper.Get<Models.VMwareHostSystemInformation[]>("vhSystemInformationList");
            if (list == null) {
                var sysinfos = new ConcurrentBag<Models.VMwareHostSystemInformation>();
                Parallel.ForEach(DA.VMwareHostConnectionInfosDA.GetAll(), (hostinfo) => {
                    Models.VMwareHostSystemInformation sysinfo = null;
                    try {
                        sysinfo = DA.VMwareHostSystemInformationRetriever.Retrieve(hostinfo);
                    }
                    catch {
                        sysinfo = DA.VMwareHostSystemInformationsDA.Get(hostinfo.hostname);
#warning Handle this better?
                        if (sysinfo == null) sysinfo = new Models.VMwareHostSystemInformation() { hostname = hostinfo.hostname, vmHostnames = hostinfo.vmHostnames };
                        sysinfo.responsive = 0;
                    }

                    sysinfos.Add(sysinfo);

                });

                foreach (var sysinfo in sysinfos)
                    DA.VMwareHostSystemInformationsDA.AddOrUpdate(sysinfo);


                list = sysinfos.ToArray();

                CacheHelper.Add("vhSystemInformationList", list);
            }

            return list;
        }
    }
}