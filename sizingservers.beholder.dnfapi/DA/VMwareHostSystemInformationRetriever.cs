/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using sizingservers.beholder.dnfapi.Models;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Xml;
using Vim25Api;
using VMware.Binding.WsTrust;
using System.Collections.Generic;
using System;
using SizingServers.Log;

namespace sizingservers.beholder.dnfapi.DA {
    /// <summary>
    /// For VMware vsphere SDK 6.7
    /// 
    /// https://vdc-repo.vmware.com/vmwb-repository/dcr-public/b525fb12-61bb-4ede-b9e3-c4a1f8171510/99ba073a-60e9-4933-8690-149860ce8754/doc/index-mo_types.html
    /// </summary>
    public static class VMwareHostSystemInformationRetriever {
        private static DateTime _epochUtc = new DateTime(1970, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

        static VMwareHostSystemInformationRetriever() {
            //Ignore invalid SSL certs.
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
        }
        /// <summary>
        /// Retrieves the specified host connection information. Throws exception when fails and disables the refresh (VMwareHostConnectionInfo = 0). Returns null when hostConnectionInfo.enabled == 0.
        /// </summary>
        /// <param name="hostConnectionInfo">The host connection information.</param>
        /// <returns></returns>
        public static VMwareHostSystemInformation Retrieve(VMwareHostConnectionInfo hostConnectionInfo) {
            try {
                if (hostConnectionInfo.enabled == 0) return null;

                var sysinfo = new VMwareHostSystemInformation();

                sysinfo.timeStampInSecondsSinceEpochUtc = (long)(DateTime.UtcNow - _epochUtc).TotalSeconds;
                sysinfo.responsive = 1;
                sysinfo.vmHostnames = hostConnectionInfo.vmHostnames;

                VimPortType service = null;
                ServiceContent serviceContent = null;

                //Connect
                string url = "https://" + hostConnectionInfo.hostname + "/sdk";
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                service = GetVimService(url, hostConnectionInfo.username, hostConnectionInfo.password);

                var svcRef = new ManagedObjectReference();
                svcRef.type = "ServiceInstance";
                svcRef.Value = "ServiceInstance";
                serviceContent = service.RetrieveServiceContent(svcRef);


                //Finally connect, we do not need the user session later on.
                UserSession session = service.Login(serviceContent.sessionManager, hostConnectionInfo.username, hostConnectionInfo.password, null);

                //Get the host ref by IP or by host name.
                IPAddress address;
                ManagedObjectReference reference = IPAddress.TryParse(hostConnectionInfo.hostname, out address) ?
                    service.FindByIp(serviceContent.searchIndex, null, hostConnectionInfo.hostname, false) :
                    service.FindByDnsName(serviceContent.searchIndex, null, Dns.GetHostEntry(hostConnectionInfo.hostname).HostName, false);


                sysinfo.hostname = GetPropertyContent(service, serviceContent, "HostSystem", "summary.config.name", reference)[0].propSet[0].val.ToString();
                if (string.IsNullOrEmpty(sysinfo.hostname)) sysinfo.hostname = hostConnectionInfo.hostname;

                if (hostConnectionInfo.hostname != sysinfo.hostname) {
                    VMwareHostConnectionInfosDA.ChangePKValue(hostConnectionInfo, sysinfo.hostname);
                    try {
                        VMwareHostSystemInformationsDA.ChangePKValue(hostConnectionInfo.hostname, sysinfo.hostname);
                    } catch { }
                }

                var vnics = GetPropertyContent(service, serviceContent, "HostSystem", "config.network.vnic", reference)[0].propSet[0].val as HostVirtualNic[];
                var ips = new List<string>();
                foreach (var vnic in vnics) {
                    if (!string.IsNullOrEmpty(vnic.spec.ip.ipAddress)) ips.Add(vnic.spec.ip.ipAddress);
                    if (vnic.spec.ip.ipV6Config != null)
                        foreach (var configIPV6 in vnic.spec.ip.ipV6Config.ipV6Address)
                            if (!string.IsNullOrEmpty(configIPV6.ipAddress)) ips.Add(configIPV6.ipAddress);
                }
                sysinfo.ips = string.Join("\t", ips);

                var systemInfo = GetPropertyContent(service, serviceContent, "HostSystem", "hardware.systemInfo", reference)[0].propSet[0].val as HostSystemInfo;
                sysinfo.system = systemInfo.vendor + " " + systemInfo.model;

                sysinfo.os = GetPropertyContent(service, serviceContent, "HostSystem", "summary.config.product.fullName", reference)[0].propSet[0].val.ToString();

                var biosInfo = GetPropertyContent(service, serviceContent, "HostSystem", "hardware.biosInfo", reference)[0].propSet[0].val as HostBIOSInfo;
                sysinfo.bios = biosInfo.vendor + " " + biosInfo.biosVersion;

                var cpuPkgs = GetPropertyContent(service, serviceContent, "HostSystem", "hardware.cpuPkg", reference)[0].propSet[0].val as HostCpuPackage[];
                var cpuDict = new SortedDictionary<string, int>();
                foreach (var cpuPkg in cpuPkgs) {
                    string[] candidateArr = cpuPkg.description.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    string candidate = string.Join(" ", candidateArr);
                    if (cpuDict.ContainsKey(candidate)) ++cpuDict[candidate]; else cpuDict.Add(candidate, 1);
                }
                sysinfo.processors = ComponentDictToString(cpuDict);

                var cpuInfo = GetPropertyContent(service, serviceContent, "HostSystem", "hardware.cpuInfo", reference)[0].propSet[0].val as HostCpuInfo;
                sysinfo.numCpuCores = cpuInfo.numCpuCores;
                sysinfo.numCpuThreads = cpuInfo.numCpuThreads;

                long memorySize = (long)GetPropertyContent(service, serviceContent, "HostSystem", "hardware.memorySize", reference)[0].propSet[0].val;
                sysinfo.memoryInGB = Convert.ToInt32(Math.Round(Convert.ToDouble(memorySize) / (1024 * 1024 * 1024), MidpointRounding.AwayFromZero));

                //----

                //First ask the childentity from the rootfolder (datacenter)
                ObjectContent[] oCont = GetPropertyContent(service, serviceContent, "Folder", "childEntity", serviceContent.rootFolder);

                ManagedObjectReference datacenter = (oCont[0].propSet[0].val as ManagedObjectReference[])[0];

                //Then ask the datastore from the datacenter
                var datastoreRefs = GetPropertyContent(service, serviceContent, "Datacenter", "datastore", datacenter)[0].propSet[0].val as ManagedObjectReference[];

                var hostMultipathInfo = GetPropertyContent(service, serviceContent, "HostSystem", "config.storageDevice.multipathInfo", reference)[0].propSet[0].val as HostMultipathInfo;
                var scsiLuns = GetPropertyContent(service, serviceContent, "HostSystem", "config.storageDevice.scsiLun", reference)[0].propSet[0].val as ScsiLun[];

                string[] datastoreArr = new string[datastoreRefs.Length];
                for (int i = 0; i != datastoreRefs.Length; i++) {
                    var candidate = datastoreRefs[i];
                    var dsInfo = GetPropertyContent(service, serviceContent, "Datastore", "info", candidate)[0].propSet[0].val;
                    string dataStoreName = null, diskName = null;

                    if (dsInfo is VmfsDatastoreInfo) {
                        dataStoreName = (dsInfo as VmfsDatastoreInfo).name;
                        diskName = (dsInfo as VmfsDatastoreInfo).vmfs.extent[0].diskName;
                    }
                    else if (dsInfo is NasDatastoreInfo) {
                        dataStoreName = (dsInfo as NasDatastoreInfo).name;
                    }
                    if (diskName == null) {
                        diskName = "unknown";
                    }
                    else {
                        foreach (ScsiLun lun in scsiLuns)
                            if (lun.canonicalName == diskName) {
                                diskName = lun.displayName;
                                break;
                            }
                    }

                    if (dataStoreName == null) dataStoreName = "Unknown";

                    datastoreArr[i] = dataStoreName + " disk " + diskName;
                }
                sysinfo.datastores = string.Join("\t", datastoreArr);

                //Then ask the vm folder from the datacenter
                var vmFolder = GetPropertyContent(service, serviceContent, "Datacenter", "vmFolder", datacenter)[0].propSet[0].val as ManagedObjectReference;
                //finally get the list of the managed object from the vms.
                var vmRefs = GetPropertyContent(service, serviceContent, "Folder", "childEntity", vmFolder)[0].propSet[0].val as ManagedObjectReference[];

                var vDiskPathsHs = new HashSet<string>();
                foreach (var vmRef in vmRefs) {
                    foreach (var dev in (GetPropertyContent(service, serviceContent, "VirtualMachine", "config.hardware", vmRef)[0].propSet[0].val as VirtualHardware).device) {
                        if (dev is VirtualDisk) {
                            if (dev.backing is VirtualDiskFlatVer2BackingInfo)
                                vDiskPathsHs.Add((dev.backing as VirtualDiskFlatVer2BackingInfo).fileName);
                            else if (dev.backing is VirtualDiskFlatVer1BackingInfo)
                                vDiskPathsHs.Add((dev.backing as VirtualDiskFlatVer1BackingInfo).fileName);
                        }
                    }
                }

                sysinfo.vDiskPaths = string.Join("\t", vDiskPathsHs);

                //---

                var physicalNics = GetPropertyContent(service, serviceContent, "HostSystem", "config.network.pnic", reference)[0].propSet[0].val as PhysicalNic[];
                string[] pNicsArr = new string[physicalNics.Length];
                for (int i = 0; i != physicalNics.Length; i++) {
                    var candidate = physicalNics[i];
                    pNicsArr[i] = candidate.device + " " + candidate.driver + " driver (" + (candidate.linkSpeed == null ? "not connected)" : "connected)");
                }
                sysinfo.nics = string.Join("\t", pNicsArr);

                var ipmiPropset = GetPropertyContent(service, serviceContent, "HostSystem", "config.ipmi", reference)[0].propSet;
                sysinfo.bmcIp = ipmiPropset == null ? "Unable to detect" : ipmiPropset[0].val.ToString();

                return sysinfo;
            }
            catch (Exception ex) {
                //Let IIS handle the errors, but using own logging.
                Loggers.Log(Level.Error, "Failed retrieving vhost system info", ex, new object[] { hostConnectionInfo });

                hostConnectionInfo.enabled = 0;
                try {
                    VMwareHostConnectionInfosDA.AddOrUpdate(hostConnectionInfo);
                }
                catch {
                    Loggers.Log(Level.Error, "Failed disabling vhost system info refresh in VMwareHostConnectionInfos", ex, new object[] { hostConnectionInfo });
                }

                throw;
            }
        }
        private static VimPortType GetVimService(string url, string username = null, string password = null, X509Certificate2 signingCertificate = null, XmlElement rawToken = null) {
            var binding = SamlTokenHelper.GetWcfBinding();
            var address = new EndpointAddress(url);

            var factory = new ChannelFactory<VimPortType>(binding, address);

            // Attach the behaviour that handles the WS-Trust 1.4 protocol for VMware Vim Service
            factory.Endpoint.Behaviors.Add(new WsTrustBehavior(rawToken));

            SamlTokenHelper.SetCredentials(username, password, signingCertificate, factory.Credentials);

            var service = factory.CreateChannel();
            return service;
        }
        private static ObjectContent[] GetPropertyContent(VimPortType service, ServiceContent serviceContent, string propertyType, string path, ManagedObjectReference reference) {
            var propertySpecs = new PropertySpec[] { new PropertySpec() { type = propertyType, pathSet = new string[] { path } } };
            var objectSpecs = new ObjectSpec[] { new ObjectSpec() { obj = reference } };
            var propertyFilterSpecs = new PropertyFilterSpec[] { new PropertyFilterSpec() { propSet = propertySpecs, objectSet = objectSpecs } };

            return service.RetrieveProperties(new RetrievePropertiesRequest(serviceContent.propertyCollector, propertyFilterSpecs)).returnval;
        }

        /// <summary>
        /// Combines a component dictionary (key = name, value = number of) to a flat string.
        /// </summary>
        /// <param name="componentDict">The component dictionary.</param>
        /// <returns
        /// </returns>
        private static string ComponentDictToString(SortedDictionary<string, int> componentDict) {
            string[] arr = new string[componentDict.Count];
            int i = 0;
            foreach (var kvp in componentDict) {
                string key = kvp.Key;
                if (kvp.Value > 1) key += " x" + kvp.Value;

                arr[i++] = key;
            }
            return string.Join("\t", arr);
        }
    }
}