namespace DPXLib.Extension
{
    /// <summary>
    /// data object for <see cref="DPXExtensions.GetBackupSizeAsync(Model.JobInstances.JobInstance, bool, long)"/>
    /// </summary>
    public class JobSizeInfo
    {
        /// <summary>
        /// total data backed up in this job, in bytes
        /// (MSG_ID SNBJH_3311J for NDMP and FILE jobs)
        /// (MSG_ID SNBSVH_253J for BLOCK jobs)
        /// </summary>
        public long TotalDataBackedUp { get; internal set; } = 0;

        /// <summary>
        /// total data wwritten to tape in this job, in bytes
        /// (MSG_ID SNBJH_3313J)
        /// </summary>
        public long TotalDataOnMedia { get; internal set; } = 0;
    }
}
