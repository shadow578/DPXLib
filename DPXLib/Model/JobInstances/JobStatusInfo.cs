using DPXLib.Model.Constants;
using Newtonsoft.Json;
using System;

namespace DPXLib.Model.JobInstances
{
    /// <summary>
    /// contains further information about the run status of a <see cref="JobInstance"/>
    /// </summary>
    public class JobStatusInfo
    {
        /// <summary>
        /// Enum value of this job status
        /// </summary>
        [JsonIgnore]
        public JobStatus Status
        {
            get
            {
                return (JobStatus)Enum.Parse(typeof(JobStatus), StatusName, true);
            }
        }

        /// <summary>
        /// icon for this job status
        /// </summary>
        [JsonProperty("icon")]
        [Obsolete("Unknown Usage")]
        public string Icon { get; set; }

        /// <summary>
        /// display name of this job status
        /// </summary>
        [JsonProperty("display")]
        public string DisplayName { get; set; }

        /// <summary>
        /// internal name of this job status
        /// </summary>
        [JsonProperty("job_instance_status_name")]
        public string StatusName { get; set; }
    }
}
