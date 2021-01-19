using Newtonsoft.Json;
using System;

namespace DPXLib.Model.License
{
    /// <summary>
    /// Response to a license request from the dpx server, containing license and usage details
    /// Method GET
    /// </summary>
    public class LicenseResponse
    {
        /// <summary>
        /// The client that created this object
        /// </summary>
        [JsonIgnore]
        public DPXClient SourceClient { get; internal set; }

        /// <summary>
        /// the currently installed license key
        /// </summary>
        [JsonProperty("key")]
        public string LicenseKey { get; set; }

        /// <summary>
        /// is this a evaluation license?
        /// </summary>
        [JsonProperty("eval_license")]
        public bool IsEvalLicanse { get; set; }

        /// <summary>
        /// days until the license expires
        /// </summary>
        [JsonProperty("expiration_days")]
        public int ExpiresInDays { get; set; }

        /// <summary>
        /// license warning code
        /// </summary>
        [JsonProperty("warning_code")]
        [Obsolete("Unknown Function")]
        public int WarningCode { get; set; }

        /// <summary>
        /// license warning message
        /// </summary>
        [JsonProperty("message")]
        [Obsolete("Unknown Function")]
        public string Message { get; set; }

        /// <summary>
        /// name of the master server node
        /// </summary>
        [JsonProperty("server_node_name")]
        public string ServerNodeName { get; set; }

        /// <summary>
        /// ip address of the master server node
        /// </summary>
        [JsonProperty("server_node_addr")]
        public string ServerNodeAddress { get; set; }

        /// <summary>
        /// host name (FQDN) of the master server node
        /// </summary>
        [JsonProperty("server_host_name")]
        public string ServerHostName { get; set; }

        /// <summary>
        /// dpx build version
        /// </summary>
        [JsonProperty("build_version")]
        public string DPXVersion { get; set; }

        /// <summary>
        /// dpx build date
        /// </summary>
        [JsonProperty("build_date")]
        public string DPXBuildDate { get; set; }

        /// <summary>
        /// dpx time of build
        /// </summary>
        [JsonProperty("build_time")]
        public string DPXBuildTime { get; set; }

        /// <summary>
        /// category information in this license
        /// </summary>
        [JsonProperty("categories")]
        public LicenseCategory[] LicenseCategories { get; set; }
    }
}
