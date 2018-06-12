﻿/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System.ComponentModel.DataAnnotations;

namespace sizingservers.beholder.dnfapi.DA {
    /// <summary>
    /// Holds a host's system information.
    /// </summary>
    public struct VMwareHostSystemInformation {
        /// <summary>
        /// Generated by the API controller
        /// </summary>
        public long timeStampInSecondsSinceEpochUtc { get; set; }
        /// <summary>
        /// </summary>
        [Key]
        public string ipOrHostname { get; set; }
        /// <summary>
        /// For when the current machine is a ESXi (>= 6.5) host. Assign quests to this range so a more usable UI can be build.
        /// </summary>
        public string guestHostnames { get; set; }
        /// <summary>
        /// Example: VMware ESXi 6.5.0 build-5969303
        /// </summary>
        public string os { get; set; }
        /// <summary>
        /// Vendor + model. Example: QCI QSSC-S4R
        /// </summary>
        public string system { get; set; }
        /// <summary>
        /// Vendor + version. Example: Intel Corp. QSSC-S4R.QCI.01.00.0039.062320161211
        /// </summary>
        public string bios { get; set; }
        /// <summary>
        /// Tab seperated; Intel, AMD, ... + model + clock. Example: Intel(R) Core(TM) i7-6820HQ CPU @ 2.70GHz
        /// </summary>
        public string processors { get; set; }
        /// <summary>
        /// The number cpu cores.
        /// </summary>
        public int numCpuCores { get; set; }
        /// <summary>
        /// The number cpu threads.
        /// </summary>
        public int numCpuThreads { get; set; }
        /// <summary>
        /// The total installed RAM in GB.
        /// </summary>
        public int memoryInGB { get; set; }
        /// <summary>
        /// Tab seperated; LUN name + disk. Example: BECKTON4U-LOCAL disk Local INTEL Disk (naa.600605b000f922a01ba1b5e31eb57e46)
        /// </summary>
        public string datastores { get; set; }
        /// <summary>
        /// Tab seperated; The vDisk paths. Example: [BECKTON4U-LOCAL] drupal-v3/drupal-v3-000002.vmdk
        /// </summary>
        public string vDiskPaths { get; set; }
        /// <summary>
        /// Tab seperated; Decives + driver + connected. Example: vmnic0 igb driver (connected)
        /// </summary>
        public string nics { get; set; }
    }
}