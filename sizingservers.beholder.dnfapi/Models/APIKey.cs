/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System.ComponentModel.DataAnnotations;

namespace sizingservers.beholder.dnfapi.Models {
    /// <summary>
    /// </summary>
    public class APIKey {
        /// <summary>
        /// When the key was issued.
        /// </summary>
        public long timeStampInSecondsSinceEpochUtc { get; set; }
        /// <summary>
        /// The e-mail adres of the user. A check could happen if this e-mail address exists.
        /// </summary>
        [Key]
        public string emailAddress { get; set; }
        /// <summary>
        /// No restrictions. Just a string. Should maybe be a SHA512 hash.
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// </summary>
        /// <returns>
        /// The key.
        /// </returns>
        public override string ToString() {
            return key;
        }
    }
}