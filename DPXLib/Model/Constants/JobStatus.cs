namespace DPXLib.Model.Constants
{
    /// <summary>
    /// states a job can have in dpx
    /// </summary>
    public enum JobStatus
    {
        None,
        Aborted,
        Cancelled,
        Cancelling,
        Completed,
        Failed,
        Held,
        Resuming,
        Running,
        Suspended,
        Suspending
    }
}
