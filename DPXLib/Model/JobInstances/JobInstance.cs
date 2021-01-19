using DPXLib.Model.Common;
using DPXLib.Model.Constants;
using DPXLib.Util;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DPXLib.Model.JobInstances
{
    /// <summary>
    /// information about a job instance
    /// </summary>
    public class JobInstance
    {
        /// <summary>
        /// The client that created this object
        /// </summary>
        [JsonIgnore]
        public DPXClient SourceClient { get; internal set; }

        /// <summary>
        /// the name of the job this is a instance of
        /// </summary>
        [JsonProperty("job_name")]
        public string Name { get; set; }

        /// <summary>
        /// the display name of the job this is a instance of
        /// </summary>
        [JsonProperty("job_display_name")]
        public string DisplayName { get; set; }

        /// <summary>
        /// the type of this job instance (BASE, DIFF, INCR)
        /// </summary>
        [JsonProperty("job_instance_run_type")]
        [JsonConverter(typeof(NullableStringEnumConverter))]
        public JobRunType? RunType { get; set; }

        /// <summary>
        /// instance type grouping of this job (BLOCK, NDMP, FILE, ...)
        /// </summary>
        [JsonProperty("job_instance_type_grouping")]
        [JsonConverter(typeof(NullableStringEnumConverter))]
        public JobType? JobType { get; set; }

        /// <summary>
        /// how long this job will be retained, in days
        /// </summary>
        [JsonProperty("retention_days")]
        public long Retention { get; set; }

        /// <summary>
        /// Total data to back up, in bytes
        /// </summary>
        [JsonProperty("total_data")]
        public long TotalData { get; set; }

        /// <summary>
        /// Amount of data already backed up, in bytes
        /// </summary>
        [JsonProperty("completed_data")]
        public long CompletedData { get; set; }

        /// <summary>
        /// throughput of backup operation, in bytes/s
        /// </summary>
        [JsonProperty("throughput")]
        [Obsolete("Unknown Unit")]
        public long Throughput { get; set; }

        /// <summary>
        /// return code of the job
        /// see https://kb.catalogicsoftware.com/s/article/000005423
        /// </summary>
        [JsonProperty("rc")]
        public int ReturnCode { get; set; }

        /// <summary>
        /// date and time the job started
        /// </summary>
        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// date and time the job ended
        /// </summary>
        [JsonProperty("end_time")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// how long the job ran, in milli seconds
        /// </summary>
        [JsonProperty("duration")]
        public long RunDuration { get; set; }

        /// <summary>
        /// has the job been cataloged?
        /// </summary>
        [JsonProperty("catalog_completed")]
        public bool IsCataloged { get; set; }

        /// <summary>
        /// the schedule ID of this job instance
        /// </summary>
        [JsonProperty("job_sched_id")]
        public long ScheduleID { get; set; }

        /// <summary>
        /// the instance id of this job instance
        /// </summary>
        [JsonProperty("job_instance_id")]
        public long ID { get; set; }

        /// <summary>
        /// instance type of this job
        /// </summary>
        [JsonProperty("job_instance_type")]
        [Obsolete("Unknown Usage")]
        public string InstanceType { get; set; }

        /// <summary>
        /// job_instance_command_name
        /// </summary>
        [JsonProperty("job_instance_command_name")]
        [Obsolete("Unknown Usage")]
        public string InstanceCommandName { get; set; }

        /// <summary>
        /// task_id
        /// </summary>
        [JsonProperty("task_id")]
        [Obsolete("Unknown Type Unknown Usage")]
        public object TaskID { get; set; }

        /// <summary>
        /// url to get additional information about the job this is a instance of
        /// </summary>
        [JsonProperty("job")]
        public string JobURL { get; set; }

        /// <summary>
        /// url to get additional information about the job status
        /// </summary>
        [JsonProperty("status")]
        public string StatusURL { get; set; }

        /// <summary>
        /// estimated_completion
        /// </summary>
        [JsonProperty("estimated_completion")]
        [Obsolete("Unknown Type")]
        public object EstimatedCompletion { get; set; }

        /// <summary>
        /// catalog_status
        /// </summary>
        [JsonProperty("catalog_status")]
        [Obsolete("Unknown Type Unknown Usage")]
        public object CatalogStatus { get; set; }

        /// <summary>
        /// cached job logs, from <see cref="GetLogEntriesAsync(long, long, bool, long, FilterItem[])"/>
        /// Only for getAllLogs = true
        /// </summary>
        private InstanceLogEntry[] CachedJobLogs { get; set; }

        /// <summary>
        /// Get logs for this job instance.
        /// if getAllLogs is true, the logs are cached (== only queried from the server the first time)
        /// </summary>
        /// <param name="startIndex">the index of the first log entry to get</param>
        /// <param name="count">how many log entries to load</param>
        /// <param name="getAllLogs">should all log entries be loaded (takes longer). If true, startIndex and count are ignored</param>
        /// <param name="filters">filters to apply to the logs. WARNING: this is more inofficial functionality</param>
        /// <param name="timeout">timeout to get job logs, in milliseconds. if the timeout is <= 0, no timeout is used</param>
        /// <returns>the list of log entries</returns>
        public async Task<InstanceLogEntry[]> GetLogEntriesAsync(long startIndex = 0, long count = 500, bool getAllLogs = false, long timeout = -1, params FilterItem[] filters)
        {
            if (getAllLogs)
            {
                // check if logs are already cached
                // if not, query from source client
                if (CachedJobLogs == null || CachedJobLogs.Length <= 0)
                    CachedJobLogs = await SourceClient?.GetAllJobInstanceLogsAsync(ID, 500, timeout, filters);

                return CachedJobLogs;
            }
            else
                return await SourceClient?.GetJobInstanceLogsAsync(ID, startIndex, count, filters);
        }

        /// <summary>
        /// query the status info of this job instance.
        /// If you only need the <see cref="JobStatus"/> of this instance, use <see cref="GetStatus"/> instead
        /// </summary>
        /// <returns>the job's status info</returns>
        public async Task<JobStatusInfo> GetStatusInfoAsync()
        {
            return await SourceClient?.GetStatusInfoAsync(StatusURL);
        }

        /// <summary>
        /// get the job status without making an api call
        /// </summary>
        /// <returns>the job status</returns>
        public JobStatus GetStatus()
        {
            //parse status string from url (last path segment)
            if (!Uri.TryCreate(StatusURL, UriKind.Absolute, out Uri result))
                return JobStatus.None;

            string statusSegment = result.Segments.Last();

            //parse status
            if (!Enum.TryParse(typeof(JobStatus), statusSegment, true, out object status))
                return JobStatus.None;

            return (JobStatus)status;
        }

        /// <summary>
        /// check if the object is equal to this job
        /// </summary>
        /// <param name="obj">the object to check</param>
        /// <returns>is equal?</returns>
        public override bool Equals(object obj)
        {
            if (obj is JobInstance job)
                return job.ID.Equals(ID) && job.Name.Equals(Name);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Name);
        }
    }
}
