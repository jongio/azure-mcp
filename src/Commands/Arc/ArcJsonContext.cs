using System.Text.Json.Serialization;
using AzureMcp.Options;
using AzureMcp.Options.Arc;
using AzureMcp.Options.Subscription;

namespace AzureMcp.Commands.Arc;

[JsonSerializable(typeof(ArcConnectOptions))]
[JsonSerializable(typeof(CommandResponse))]
public sealed partial class ArcJsonContext : JsonSerializerContext
{
}
