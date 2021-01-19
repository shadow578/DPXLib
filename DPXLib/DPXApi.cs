using DPXLib.Model.JobInstances;
using DPXLib.Model.License;
using DPXLib.Model.Login;
using DPXLib.Model.Nodes;
using Refit;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DPXLib
{
    /// <summary>
    /// DPX Rest API ReFit interface class
    /// </summary>
    [SuppressMessage("Style", "IDE1006")]
    internal interface DPXApi
    {
        /// <summary>
        /// Login into the DPX Rest api and optain a access token
        /// </summary>
        /// <param name="request">the login request</param>
        /// <returns>the login response</returns>
        [Post("/auth/login")]
        Task<LoginResponse> Login([Body] LoginRequest request);

        /// <summary>
        /// Get license information of the dpx master server
        /// </summary>
        /// <param name="bearerToken">token from Login function. (Bearer {token})</param>
        /// <returns>license information response</returns>
        [Get("/app/api/license")]
        Task<LicenseResponse> GetLicense([Header("Authorization")] string bearerToken);

        /// <summary>
        /// get job instances matching the given filter criteria
        /// </summary>
        /// <param name="bearerToken">token from Login function. (Bearer {token})</param>
        /// <param name="jobFilter">filters for the job instances, serialized array of <see cref="FilterItem"/></param>
        /// <returns>a list of all found job instances</returns>
        [Get("/app/api/job_instances")]
        Task<JobInstance[]> GetJobInstances([Header("Authorization")] string bearerToken, [AliasAs("filter")] string jobFilter);

        /// <summary>
        /// get job instances matching the given filter criteria
        /// </summary>
        /// <param name="bearerToken">token from Login function. (Bearer {token})</param>
        /// <param name="jobInstanceID">job instange to get</param>
        /// <returns>a list of all found job instances</returns>
        [Get("/app/api/job_instances/{id}")]
        Task<JobInstance> GetJobInstance([Header("Authorization")] string bearerToken, [AliasAs("id")] long jobInstanceID);

        /// <summary>
        /// get logs from a job instance with the given instance id
        /// </summary>
        /// <param name="bearerToken">token from Login function. (Bearer {token})</param>
        /// <param name="jobInstanceID">job instange to get logs of</param>
        /// <param name="index">start log entry index</param>
        /// <param name="count">how many log entries to get</param>
        /// <param name="filter">filters to apply to the log entries, serialized array of <see cref="FilterItem"/>. WARNING: this isn't really official api</param>
        /// <returns>a list of all found log entries for this job instance</returns>
        [Get("/app/api/job_instances/{id}/log")]
        Task<InstanceLogEntry[]> GetLogEntries([Header("Authorization")] string bearerToken, [AliasAs("id")] long jobInstanceID, long index, long count, string filter);

        /// <summary>
        /// Get the status info for a status string (eg. COMPLETED)
        /// </summary>
        /// <param name="statusStr">the status string, parsed from status url</param>
        /// <returns>the status info</returns>
        [Get("/app/api/job_instance_statuses/{status}")]
        Task<JobStatusInfo> GetStatusInfo([AliasAs("status")] string statusStr);

        /// <summary>
        /// Get information about a single node group
        /// </summary>
        /// <param name="bearerToken">token from Login function. (Bearer {token})</param>
        /// <param name="nodeGroup">the name of the node group</param>
        /// <returns>the node group</returns>
        [Get("/app/api/node_groups/{nodeGroup}")]
        Task<NodeGroup> GetNodeGroup([Header("Authorization")] string bearerToken, string nodeGroup);

        /// <summary>
        /// Get a list of all node groups
        /// </summary>
        /// <param name="bearerToken">token from Login function. (Bearer {token})</param>
        /// <returns>a list of node groups</returns>
        [Get("/app/api/node_groups")]
        Task<NodeGroup[]> GetNodeGroups([Header("Authorization")] string bearerToken );

        /// <summary>
        /// Get a list of all nodes
        /// </summary>
        /// <param name="bearerToken">token from Login function. (Bearer {token})</param>
        /// <param name="groupNameFilter">filter nodes by node group name</param>
        /// <param name="typeFilter">filter nodes by type</param>
        /// <returns>a list of all matching nodes</returns>
        [Get("/app/api/nodes")]
        Task<Node[]> GetNodes([Header("Authorization")] string bearerToken, [AliasAs("node_group_name")] string groupNameFilter, [AliasAs("node_type")] string typeFilter);
    }
}
