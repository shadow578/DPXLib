using DPXLib.Model.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DPXLib.Model.License
{
    /// <summary>
    /// License category in a <see cref="LicenseResponse"/>
    /// </summary>
    public class LicenseCategory
    {
        /// <summary>
        /// the name of this category
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// info about this category
        /// </summary>
        [JsonProperty("info")]
        public string Info { get; set; }

        /// <summary>
        /// is the license for this category violated?
        /// </summary>
        [JsonProperty("licenseViolated")]
        public bool IsLicenseViolated { get; set; }

        /// <summary>
        /// licensed amount in this category
        /// </summary>
        [JsonProperty("licensed")]
        public DimensionedValue Licensed { get; set; }

        /// <summary>
        /// consumed amount in this category
        /// </summary>
        [JsonProperty("consumed")]
        public DimensionedValue Consumed { get; set; }

        /// <summary>
        /// Children of this category
        /// </summary>
        [JsonProperty("children")]
        public List<LicenseCategory> Children { get; set; }
    }
}
