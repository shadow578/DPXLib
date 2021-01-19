using DPXTool.DPX.Model.Constants;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DPXLib.Model.Nodes
{
    /// <summary>
    /// information about a dpx node
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The client that created this object
        /// </summary>
        [JsonIgnore]
        public DPXClient SourceClient { get; internal set; }

        /// <summary>
        /// name of the node
        /// </summary>
        [JsonProperty("node_name")]
        public string Name { get; set; }

        /// <summary>
        /// type of the node
        /// </summary>
        [JsonProperty("node_type")]
        public string Type { get; set; }

        /// <summary>
        /// name of the node group this node is a part of
        /// </summary>
        [JsonProperty("node_group_name")]
        public string GroupName { get; set; }

        /// <summary>
        /// comment about this node
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// name of the server behind this node
        /// </summary>
        [JsonProperty("server_name")]
        public string ServerName { get; set; }

        /// <summary>
        /// OS group of this node
        /// </summary>
        [JsonProperty("opsys")]
        public NodeOSGroup OSGroup { get; set; }

        /// <summary>
        /// OS display name of this node
        /// </summary>
        [JsonProperty("osname")]
        public string OSDisplayName { get; set; }

        /// <summary>
        /// OS version of this node
        /// </summary>
        [JsonProperty("osversion")]
        public string OSVersion { get; set; }

        /// <summary>
        /// OS release version of this node
        /// </summary>
        [JsonProperty("osrelease")]
        public string OSRelease { get; set; }

        /// <summary>
        /// subostype
        /// </summary>
        [JsonProperty("subostype")]
        [Obsolete("Unknown Usage")]
        public int OSSubType { get; set; }

        /// <summary>
        /// vendor of this node.
        /// </summary>
        [JsonProperty("vendor_name")]
        [Obsolete("Unknown Usage")]
        public string Vendor { get; set; }

        /// <summary>
        /// user that created this node
        /// </summary>
        [JsonProperty("creator")]
        public string Creator { get; set; }

        /// <summary>
        /// date this node was created
        /// </summary>
        [JsonProperty("creation_time")]
        public DateTime CreationTime { get; set; }

        /// <summary>
        ///  features supported by this node
        /// </summary>
        [JsonProperty("node_feature_flags")]
        public NodeFeatureFlags[] FeatureFlags { get; set; }

        /// <summary>
        /// Get the node group instance this node is a part of
        /// </summary>
        /// <returns>the node group, or null if not found</returns>
        public async Task<NodeGroup> GetGroupAsync()
        {
            return await SourceClient.GetNodeGroupAsync(GroupName);
        }
    }
}
