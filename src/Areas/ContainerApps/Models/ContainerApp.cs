// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace AzureMcp.Areas.ContainerApps.Models;

public class ContainerApp
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName("subscriptionId")]
    public string? SubscriptionId { get; set; }

    [JsonPropertyName("managedEnvironmentId")]
    public string? ManagedEnvironmentId { get; set; }

    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    [JsonPropertyName("configuration")]
    public ContainerAppConfiguration? Configuration { get; set; }

    [JsonPropertyName("template")]
    public ContainerAppTemplate? Template { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
}

public class ContainerAppConfiguration
{
    [JsonPropertyName("ingress")]
    public ContainerAppIngress? Ingress { get; set; }

    [JsonPropertyName("secrets")]
    public List<ContainerAppSecret>? Secrets { get; set; }

    [JsonPropertyName("registries")]
    public List<ContainerAppRegistry>? Registries { get; set; }
}

public class ContainerAppIngress
{
    [JsonPropertyName("external")]
    public bool? External { get; set; }

    [JsonPropertyName("targetPort")]
    public int? TargetPort { get; set; }

    [JsonPropertyName("fqdn")]
    public string? Fqdn { get; set; }
}

public class ContainerAppSecret
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class ContainerAppRegistry
{
    [JsonPropertyName("server")]
    public string? Server { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}

public class ContainerAppTemplate
{
    [JsonPropertyName("containers")]
    public List<ContainerAppContainer>? Containers { get; set; }

    [JsonPropertyName("scale")]
    public ContainerAppScale? Scale { get; set; }
}

public class ContainerAppContainer
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("resources")]
    public ContainerAppResources? Resources { get; set; }
}

public class ContainerAppResources
{
    [JsonPropertyName("cpu")]
    public double? Cpu { get; set; }

    [JsonPropertyName("memory")]
    public string? Memory { get; set; }
}

public class ContainerAppScale
{
    [JsonPropertyName("minReplicas")]
    public int? MinReplicas { get; set; }

    [JsonPropertyName("maxReplicas")]
    public int? MaxReplicas { get; set; }
}
