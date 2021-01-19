using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DPXLib.Model.Nodes
{
    /// <summary>
    /// information about a group of <see cref="Node"/>
    /// </summary>
    public class NodeGroup
    {
        /// <summary>
        /// The client that created this object
        /// </summary>
        [JsonIgnore]
        public DPXClient SourceClient { get; internal set; }

        /// <summary>
        /// Name of this group
        /// </summary>
        [JsonProperty("node_group_name")]
        public string Name { get; set; }

        /// <summary>
        /// comments on this group
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// name of this group's media pool
        /// </summary>
        [JsonProperty("media_pool_name")]
        public string MediaPool { get; set; }

        /// <summary>
        /// name of the device clustter in this group
        /// </summary>
        [JsonProperty("device_cluster_name")]
        public string ClusterName { get; set; }

        /// <summary>
        /// user that created this group
        /// </summary>
        [JsonProperty("creator")]
        public string Creator { get; set; }

        /// <summary>
        /// date this group was created
        /// </summary>
        [JsonProperty("creation_time")]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// admin_name
        /// </summary>
        [JsonProperty("admin_name")]
        [Obsolete("Unknown Usage")]
        public string AdminName { get; set; }

        /// <summary>
        /// get all nodes that are part of this group
        /// </summary>
        /// <param name="nodeType">the node type the nodes must have (optional)</param>
        /// <returns>a list of all nodes in this group</returns>
        public async Task<Node[]> GetNodesAsync(string nodeType = null)
        {
            return await SourceClient.GetNodesAsync(nodeGroup: Name, nodeType: nodeType);
        }
    }
}
