using AzureMcp.Options;
using AzureMcp.Options.Subscription;

namespace AzureMcp.Options.Arc
{
    public sealed class AkseeDeploymentOptions : BaseSubscriptionOptions
    {
        public string OutputPath { get; set; } = string.Empty;
        public string ClusterName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
