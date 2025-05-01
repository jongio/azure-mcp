﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AzureMcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace AzureMcp.Tests.Client;

public class MockClientTests
{
    private readonly McpServerOptions _options;

    public MockClientTests()
    {
        _options = CreateOptions();
    }

    private static McpServerOptions CreateOptions(ServerCapabilities? capabilities = null)
    {
        return new McpServerOptions
        {
            ProtocolVersion = "2024",
            InitializationTimeout = TimeSpan.FromSeconds(30),
            Capabilities = capabilities,
            ServerInfo = new Implementation { Name = "Azure MCP", Version = "1.0.0-beta" }
        };
    }

    [Fact]
    public async Task Invoke_Ping_Request_To_Server()
    {
        await Invoke_Request_To_Server(
            method: "ping",
            serverCapabilities: null,
            configureOptions: null,
            assertResult: response =>
            {
                JsonObject jObj = Assert.IsType<JsonObject>(response);
                Assert.Empty(jObj);
            });
    }

    [Fact]
    public async Task Invoke_Init_Command()
    {
        await Invoke_Request_To_Server(
            method: "initialize",
            serverCapabilities: null,
            configureOptions: null,
            assertResult: response =>
            {
                var result = JsonSerializer.Deserialize<InitializeResult>(response);
                Assert.NotNull(result);
                Assert.Equal("Azure MCP", result.ServerInfo.Name);
                Assert.Equal("1.0.0-beta", result.ServerInfo.Version);
                Assert.Equal("2024", result.ProtocolVersion);
            });
    }

    [Fact]
    public async Task Invoke_Az_List_Subscription_Command()
    {
        await Invoke_Request_To_Server(
            method: "tools/call",
            new ServerCapabilities
            {
                Tools = new()
                {
                    CallToolHandler = (request, ct) =>
                    {
                        if (request.Params?.Name == "azmcp-subscription-list")
                        {
                            return ValueTask.FromResult(new CallToolResponse
                            {
                                Content =
                                [
                                    new Content
                                    {
                                        Type = "application/json",
                                        Text = JsonSerializer.Serialize(new
                                        {
                                            subscriptions = new[]
                                            {
                                                new { id = "sub-1", name = "Test Sub A" },
                                                new { id = "sub-2", name = "Test Sub B" }
                                            }
                                        })
                                    }
                                ]
                            });
                        }

                        throw new Exception($"Unhandled tool name: {request.Params?.Name}");
                    },
                    ListToolsHandler = (request, ct) => throw new NotImplementedException(),
                }
            },
            requestParams: JsonSerializer.SerializeToNode(new
            {
                name = "azmcp-subscription-list",
                arguments = new { }
            }),
            configureOptions: null,
            assertResult: response =>
            {
                var callToolResponse = JsonSerializer.Deserialize<CallToolResponse>(response);
                Assert.NotNull(callToolResponse);
                Assert.NotEmpty(callToolResponse.Content);

                var jsonContent = callToolResponse.Content.FirstOrDefault(c => c.Type == "application/json");
                Assert.NotNull(jsonContent);

                var json = JsonSerializer.Deserialize<JsonNode>(jsonContent!.Text!);
                var subs = json?["subscriptions"]?.AsArray();
                Assert.NotNull(subs);
                Assert.NotEmpty(subs!);
                Assert.Equal("Test Sub A", subs![0]!["name"]?.ToString());
            });
    }


    [Fact]
    public async Task Invoke_List_Tools_Command()
    {
        await Invoke_Request_To_Server(
            method: "tools/list",
            new ServerCapabilities
            {
                Tools = new()
                {
                    ListToolsHandler = (request, ct) =>
                    {
                        return ValueTask.FromResult(new ListToolsResult
                        {
                            Tools = [new() { Name = "ListTools" }]
                        });
                    },
                    CallToolHandler = (request, ct) => throw new NotImplementedException(),
                }
            },
            configureOptions: null,
            assertResult: response =>
            {
                var result = JsonSerializer.Deserialize<ListToolsResult>(response);
                Assert.NotNull(result);
                Assert.NotEmpty(result.Tools);
                Assert.Equal("ListTools", result.Tools[0].Name);
            });
    }

    [Fact]
    public async Task Invoke_Dummy_Tool()
    {
        await Invoke_Request_To_Server(
            method: "tools/call",
            new ServerCapabilities
            {
                Tools = new()
                {
                    CallToolHandler = (request, ct) =>
                    {
                        return ValueTask.FromResult(new CallToolResponse
                        {
                            Content = [new Content { Text = "dummyTool" }]
                        });
                    },
                    ListToolsHandler = (request, ct) => throw new NotImplementedException(),
                }
            },
            configureOptions: null,
            assertResult: response =>
            {
                var result = JsonSerializer.Deserialize<CallToolResponse>(response);
                Assert.NotNull(result);
                Assert.NotEmpty(result.Content);
                Assert.Equal("dummyTool", result.Content[0].Text);
            });
    }


    private async Task Invoke_Request_To_Server(string method, ServerCapabilities? serverCapabilities, Action<McpServerOptions>? configureOptions, Action<JsonNode?> assertResult)
    {
        await Invoke_Request_To_Server(
            serverCapabilities: serverCapabilities,
            method: method,
            requestParams: null,
            configureOptions: configureOptions,
            assertResult: assertResult
        );
    }

    private async Task Invoke_Request_To_Server(string method, ServerCapabilities? serverCapabilities, JsonNode? requestParams, Action<McpServerOptions>? configureOptions, Action<JsonNode?> assertResult)
    {
        await using var transport = new CustomTestTransport();
        var options = CreateOptions(serverCapabilities);
        configureOptions?.Invoke(options);

        await using var server = McpServerFactory.Create(transport, options);
        var runTask = server.RunAsync();

        var receivedMessage = new TaskCompletionSource<JsonRpcResponse>();

        transport.MessageListener = (message) =>
        {
            if (message is JsonRpcResponse response && response.Id.ToString() == "07")
                receivedMessage.SetResult(response);
        };

        // Simulate a client sending a request to the server
        await transport.SendMessageAsync(
        new JsonRpcRequest
        {
            Method = method,
            Params = requestParams,
            Id = new RequestId("07"),
        }
        );

        var response = await receivedMessage.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.NotNull(response);

        var node = JsonSerializer.SerializeToNode(response.Result);
        assertResult(node);

        await transport.DisposeAsync();
        await runTask;
    }
}