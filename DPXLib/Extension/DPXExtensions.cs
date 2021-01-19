using DPXLib.Model.Constants;
using DPXLib.Model.JobInstances;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DPXLib.Extension
{
    /// <summary>
    /// Extensions for the <see cref="DPXClient"/> and related classes
    /// </summary>
    public static class DPXExtensions
    {
        /// <summary>
        /// Get the volsers that were used by this job
        /// Only valid if the job finished
        /// </summary>
        /// <param name="job">the job to get volsers of</param>
        /// <param name="onlyCompleted">only check for volsers in completed jobs</param>
        /// <param name="timeout">timeout to get job volsers, in milliseconds. if the timeout is <= 0, no timeout is used</param>
        /// <returns>a list of volsers used by this job, or null if the job was not completed</returns>
        public static async Task<string[]> GetVolsersUsed(this JobInstance job, bool onlyCompleted = true, long timeout = -1)
        {
            //check job ran to completion
            if (job.GetStatus() != JobStatus.Completed && onlyCompleted)
                return null;

            //get all logs for this job
            InstanceLogEntry[] logs = await job.GetLogEntriesAsync(getAllLogs: true, timeout: timeout);

            //search for logs with message_code "SNBJH_3332J" (volser reporting in format "xxxxL8")
            List<string> volsers = new List<string>();
            foreach (InstanceLogEntry log in logs)
                if (log.Match(module: "ssjobhnd", messageCode: "SNBJH_3332J")//is SNBJH_3332J
                    && Regex.IsMatch(log.Message, "[0-9]{4}L[0-9]"))//check message matches volser format
                    volsers.Add(log.Message);

            return volsers.ToArray();
        }

        /// <summary>
        /// Get backup data size information for a job
        /// Only valid if the job finished
        /// </summary>
        /// <param name="job">the job to get data size information of</param>
        /// <param name="onlyCompleted">only check for completed jobs</param>
        /// <param name="timeout">timeout to get job information, in milliseconds. if the timeout is <= 0, no timeout is used</param>
        /// <returns>information about the size of the backup job, or null if the job was not completed or nothing was found</returns>
        public static async Task<JobSizeInfo> GetBackupSizeAsync(this JobInstance job, bool onlyCompleted = true, long timeout = -1)
        {
            //check job ran to completion
            if (job.GetStatus() != JobStatus.Completed && onlyCompleted)
                return null;

            //get all logs for this job
            InstanceLogEntry[] logs = await job.GetLogEntriesAsync(getAllLogs: true, timeout: timeout);

            //prepare data object
            JobSizeInfo sizeInfo = new JobSizeInfo();

            //search logs for the following message codes:
            //ssjobhnd - SNBJH_3311J - total backup size         - "Total data backed up: 3670512 KB"
            //ssjobhnd - SNBJH_3313J - total data on tape        - "Total data on media: 3670592 KB"
            //sssvh    - SNBSVH_253J - total data volume (BLOCK) - "Total data volume : 222 GB" 
            bool oneFound = false;
            foreach (InstanceLogEntry log in logs)
                if (log.Match(module: "ssjobhnd", messageCode: "SNBJH_3311J"))
                {
                    //total backup size; parse and convert from KB to Bytes
                    sizeInfo.TotalDataBackedUp = ParseLong(log.Message.ToLower(), @"total data backed up: (\d*) kb").GetValueOrDefault(0) * 1000;
                    oneFound = true;
                }
                else if (log.Match(module: "ssjobhnd", messageCode: "SNBJH_3313J"))
                {
                    //total data on tape; parse and convert from KB to Bytes
                    sizeInfo.TotalDataOnMedia = ParseLong(log.Message.ToLower(), @"total data on media: (\d*) kb").GetValueOrDefault(0) * 1000;
                    oneFound = true;
                }
                else if (log.Match(module: "sssvh", messageCode: "SNBSVH_253J"))
                {
                    //total backup volume (BLOCK); parse and convert from unit suffix to Bytes
                    const string PATTERN = @"total data volume : (\d*) (kb|mb|gb|tb|pb)";

                    //parse size in whatever unit
                    long size = ParseLong(log.Message.ToLower(), PATTERN).GetValueOrDefault(0);

                    //get unit using a second regex, same pattern
                    Match m = Regex.Match(log.Message.ToLower(), PATTERN);

                    // check we have a successfull match on the unit capture group
                    if (!m.Success
                        || m.Groups.Count < 3
                        || !m.Groups[2].Success)
                        break;

                    //get unit and convert size to bytes
                    switch(m.Groups[2].Value)
                    {
                        case "pb":
                            size /= 1000000000000000;
                            break;
                        case "tb":
                            size /= 1000000000000;
                            break;
                        case "gb":
                            size /= 1000000000;
                            break;
                        case "mb":
                            size /= 1000000;
                            break;
                        case "kb":
                            size /= 1000;
                            break;
                    }

                    //set size in SizeInfo object
                    sizeInfo.TotalDataBackedUp = size;
                    oneFound = true;
                }

            //return null if no matching log was found
            if (!oneFound)
                return null;

            return sizeInfo;
        }

        /// <summary>
        /// get information about the time a job spend in its different phases
        /// </summary>
        /// <param name="job">the job to get timing information of</param>
        /// <param name="timeout">timeout to get job information, in milliseconds. if the timeout is <= 0, no timeout is used</param>
        /// <returns>information about the time the backup job spend in its different phases, or null if the job was not completed or nothing was found</returns>
        public static async Task<JobTimeInfo> GetJobTimingsAsync(this JobInstance job, long timeout = -1)
        {
            //check job ran to completion
            if (job.GetStatus() != JobStatus.Completed)
                return null;

            //get all logs for this job
            InstanceLogEntry[] logs = await job.GetLogEntriesAsync(getAllLogs: true, timeout: timeout);

            //prepare datetimes to save beginnings of each phase
            DateTime? start = null,
                waitingPhaseBegin = null,
                preprocessPhaseBegin = null,
                transferPhaseBegin = null,
                end = null;

            // search logs for message codes that mark the beginning of the phases
            // assign time variables with the first log matching the phase
            foreach (InstanceLogEntry log in logs)
                if (log.Match(module: "sssched", messageCode: "SNBSCH5607J"))
                {
                    //NDMP / FILE / BLOCK: job initialization / START
                    if (!start.HasValue)
                        start = log.Time;
                }
                else if (log.Match(module: "sssched", messageCode: "SNBSCH5674J"))
                {
                    //NDMP / FILE / BLOCK: job end / END
                    if (!end.HasValue)
                        end = log.Time;
                }
                else if (log.Match(module: "ssjobhnd", messageCode: "SNBJH_3845J"))
                {
                    //NDMP / FILE / BLOCK?: Hold because of max job limit / WAITING
                    if (!waitingPhaseBegin.HasValue)
                        waitingPhaseBegin = log.Time;
                }
                else if (log.Match(module: "ssjobhnd", messageCode: "SNBJH_3439J"))
                {
                    //NDMP / FILE / BLOCK?: Hold because no drive is available / WAITING
                    if (!waitingPhaseBegin.HasValue)
                        waitingPhaseBegin = log.Time;
                }
                else if (log.Match(module: "sssvh", messageCode: "SNBSVH_278J"))
                {
                    //BLOCK: job definition preprocessing (node OR cluster) / PREPROCESS
                    if (!preprocessPhaseBegin.HasValue)
                        preprocessPhaseBegin = log.Time;
                }
                else if (log.Match(module: "ssjobhnd", messageCode: "SNBJH_3257J"))
                {
                    //NDMP / FILE: task start / TRANSFER
                    if (!transferPhaseBegin.HasValue)
                        transferPhaseBegin = log.Time;
                }
                else if (log.Match(module: "sssvh", messageCode: "SNBSVH_234J"))
                {
                    //BLOCK: transfer status update
                    if (!transferPhaseBegin.HasValue)
                        transferPhaseBegin = log.Time;
                }

            // in case we did not find start / end times using logs,
            // use the start / end times in JobInstance as a fallback
            if (!start.HasValue)
                start = job.StartTime;
            if (!end.HasValue)
                end = job.EndTime;

            // calculate times spend in phases:
            JobTimeInfo timeSpend = new JobTimeInfo
            {
                Total = end.Value - start.Value
            };

            // init phase is start -> beginning of next phase
            if (waitingPhaseBegin.HasValue)
                timeSpend.Initializing = waitingPhaseBegin.Value - start.Value;
            else if (preprocessPhaseBegin.HasValue)
                timeSpend.Initializing = preprocessPhaseBegin.Value - start.Value;
            else if (transferPhaseBegin.HasValue)
                timeSpend.Initializing = transferPhaseBegin.Value - start.Value;

            // waiting phase is waitingPhaseBegin -> beginning of next phase
            // but only if we had a waiting phase
            if (waitingPhaseBegin.HasValue)
                if (preprocessPhaseBegin.HasValue)
                    timeSpend.Waiting = preprocessPhaseBegin.Value - waitingPhaseBegin.Value;
                else if (transferPhaseBegin.HasValue)
                    timeSpend.Waiting = transferPhaseBegin.Value - waitingPhaseBegin.Value;

            // preprocess phase is preprocessPhaseBegin -> transferPhaseBegin
            // but only if we had both phases
            if (preprocessPhaseBegin.HasValue && transferPhaseBegin.HasValue)
                timeSpend.Preprocessing = transferPhaseBegin.Value - preprocessPhaseBegin.Value;

            // transfer phase is transferPhaseBegin -> end
            // but only if we had a transfer phase
            if (transferPhaseBegin.HasValue)
                timeSpend.Transferring = end.Value - transferPhaseBegin.Value;

            return timeSpend;
        }

        #region Utility
        /// <summary>
        /// check if a status indicates a failed job
        /// </summary>
        /// <param name="status">the status to check</param>
        /// <returns>indicates the status a failed job?</returns>
        public static bool IsFailedStatus(this JobStatus status)
        {
            switch (status)
            {
                case JobStatus.Running:
                case JobStatus.Resuming:
                case JobStatus.Completed:
                case JobStatus.Held:
                    return false;

                case JobStatus.Aborted:
                case JobStatus.Cancelled:
                case JobStatus.Cancelling:
                case JobStatus.Failed:
                case JobStatus.Suspended:
                case JobStatus.Suspending:
                case JobStatus.None:
                default://this is exhaustive switch, but compiler dont like :P
                    return true;
            }
        }

        /// <summary>
        /// check if a log entrys values fulfills all of the given criteria.
        /// Checks between strings are made using <see cref="StringComparison.OrdinalIgnoreCase"/>
        /// </summary>
        /// <param name="log">the log entry to match against</param>
        /// <param name="module">the module field, eg. "sssched"</param>
        /// <param name="messageCode">the message code, eg. "SNBSCH5607J"</param>
        /// <param name="sourceIp">the ip of the node that this message came from. Not recommended to be used!</param>
        /// <param name="message">the message string. Not recommended for matching!</param>
        /// <param name="time">the time of the message. Not recommended for matching!</param>
        /// <returns>does the log entry match all criteria?</returns>
        public static bool Match(this InstanceLogEntry log,
            string module = null,
            string messageCode = null,
            string sourceIp = null,
            string message = null,
            DateTime? time = null)
        {
            // check module
            if (!string.IsNullOrWhiteSpace(module)
                && !module.Equals(log.Module, StringComparison.OrdinalIgnoreCase))
                return false;

            // check message code
            if (!string.IsNullOrWhiteSpace(messageCode)
                && !messageCode.Equals(log.MessageCode, StringComparison.OrdinalIgnoreCase))
                return false;

            // check source ip
            if (!string.IsNullOrWhiteSpace(sourceIp)
                && !sourceIp.Equals(log.SourceIP, StringComparison.OrdinalIgnoreCase))
                return false;

            // check message string
            if (!string.IsNullOrWhiteSpace(message)
                && !message.Equals(log.Message, StringComparison.OrdinalIgnoreCase))
                return false;

            // check time
            if (time.HasValue
                && !time.Equals(log.Time))
                return false;

            return true;
        }

        /// <summary>
        /// parse a long from a string using regex
        /// </summary>
        /// <param name="str">the string to parse from</param>
        /// <param name="pattern">the regex pattern to use. target long is in a capture group</param>
        /// <param name="targetCaptureGroup">the capture group to parse to long from</param>
        /// <returns>the parsed long, or null if parse failed</returns>
        static long? ParseLong(string str, string pattern, int targetCaptureGroup = 1)
        {
            //run regex with the given pattern on input string
            Match m = Regex.Match(str, pattern);

            // check we have a successfull match and target capture group is in bounds
            if (!m.Success
                || targetCaptureGroup < 0
                || targetCaptureGroup >= m.Groups.Count
                || !m.Groups[targetCaptureGroup].Success)
                return null;

            //get target capture group
            string target = m.Groups[targetCaptureGroup].Value;

            // parse target as long
            if (!long.TryParse(target, out long result))
                return null;

            return result;
        }
        #endregion
    }
}
