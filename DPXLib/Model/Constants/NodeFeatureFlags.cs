using DPXLib.Model.Nodes;

namespace DPXTool.DPX.Model.Constants
{
    /// <summary>
    /// feature flags for a <see cref="Node"/>
    /// </summary>
    public enum NodeFeatureFlags
    {
        FEATURE_FLAGS_NIBBLER,
        FEATURE_FLAGS_NIBBLER_TAPE,
        FEATURE_FLAGS_NIBBLER_DATA,
        FEATURE_FLAGS_SNAPVAULT_PRIMARY,
        FEATURE_FLAGS_SNAPVAULT_SECONDARY,
        FEATURE_FLAGS_XRCLIENT,
        FEATURE_FLAGS_XRSERVER,
        FEATURE_FLAGS_VM_NODE,
        FEATURE_FLAGS_VCENTER_PROXY,
        FEATURE_FLAGS_NDMP_TAPE,
        FEATURE_FLAGS_NDMP_DATA,
        FEATURE_FLAGS_CLUSTER,
        FEATURE_FLAGS_CLUSTER_NODE
    }
}
