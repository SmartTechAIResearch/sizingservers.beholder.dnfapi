/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System.ComponentModel.DataAnnotations;

namespace sizingservers.beholder.dnfapi.Models {
    public class VMwareHostConnectionInfo {
        /// <summary>
        /// </value>
        [Key]
        public string hostname { get; set; }
        /// <summary>
        /// For when the current machine is a ESXi (>= 6.5) host. Assign quests to this range so a more usable UI can be build.
        /// </summary>
        public string vmHostnames { get; set; }
        /// <summary>
        /// For when the current machine is a ESXi (>= 6.5) host. Be carefull with credentials.
        /// We are interested in Vim25.HostHardwareInfo so only the property "HostSystem" with the path "hardware" should be accesible via https://ip of current machine/sdk using the Vim25 API (VMWare SDK 6.7).
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// For when the current machine is a ESXi (>= 6.5) host. Be carefull with credentials. 
        /// We are interested in Vim25.HostHardwareInfo so only the property "HostSystem" with the path "hardware" should be accesible via https://ip of current machine/sdk using the Vim25 API (VMWare SDK 6.7).
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// Gets or sets enabling retrieving sysinfo from the VMware SDK. This value is set to false when retrieval fails and must be re-ebabled manually by updating connection info in the database. This is neccessary because ESXi locks logins when to many false login attempts happen. 
        /// </summary>
        /// <value>
        /// The enabled.
        /// </value>
        public int enabled { get; set; }

        /// <summary>
        /// No credentials in the tostring.
        /// </summary>
        public override string ToString() {
            return "ipOrHostname: " + hostname + " vmHostnames: " + vmHostnames;
        }

    }
}