using System;

namespace DPXLib.Extension
{
    /// <summary>
    /// contains information about how much time a job spend in its different phases
    /// </summary>
    public class JobTimeInfo
    {
        /// <summary>
        /// how much time did the job take in total
        /// 
        /// Valid for:
        /// - NDMP
        /// - FILE
        /// - BLOCK
        /// </summary>
        public TimeSpan Total { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// how much time did the job spend initializing
        /// 
        /// Valid for:
        /// - NDMP
        /// - FILE
        /// - BLOCK
        /// </summary>
        public TimeSpan Initializing { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// how much time did the job spend waiting for:
        /// - a job slot (MSG_ID SNBJH_3845J)
        /// - a tape drive (MSG_ID SNBJH_3439J)
        /// 
        /// Valid for:
        /// - NDMP
        /// - FILE
        /// - BLOCK (?)
        /// </summary>
        public TimeSpan Waiting { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// how much time did the job spend for:
        /// - job definition preprocessing (Cluster & Node; MSG_ID SNBSVH_278J)
        /// 
        /// Valid for:
        /// - BLOCK
        /// </summary>
        public TimeSpan Preprocessing { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// how much time did the job spend for:
        /// - transferring data (NDMP / FILE; MSG_ID SNBJH_3257J)
        /// - transferring data (BLOCK; MSG_ID SNBSVH_234J)
        /// 
        /// Valid for:
        /// - NDMP
        /// - FILE
        /// - BLOCK
        /// </summary>
        public TimeSpan Transferring { get; set; } = TimeSpan.Zero;
    }
}
