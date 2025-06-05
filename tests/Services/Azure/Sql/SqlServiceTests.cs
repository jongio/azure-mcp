// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using AzureMcp.Models.Sql;
using AzureMcp.Options;
using AzureMcp.Services.Azure.Sql;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AzureMcp.Tests.Services.Azure.Sql;

public class SqlServiceTests
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SqlService> _logger;
    private readonly SqlService _sqlService;

    public SqlServiceTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _cacheService = Substitute.For<ICacheService>();
        _logger = Substitute.For<ILogger<SqlService>>();
        _sqlService = new SqlService(_subscriptionService, _tenantService, _cacheService, _logger);
    }

    [Fact]
    public async Task GetIndexRecommendationsAsync_ReturnsRecommendations()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var server = "test-server";
        var database = "test-db";

        var subscriptionResource = Substitute.For<SubscriptionResource>();
        _subscriptionService.GetSubscription(subscriptionId).Returns(subscriptionResource);

        // Mock ResourceGroupResource
        var resourceGroupResource = Substitute.For<ResourceGroupResource>();
        var serverResponse = Substitute.For<Response<SqlServerResource>>();
        var serverResource = Substitute.For<SqlServerResource>();
        var databaseResponse = Substitute.For<Response<SqlDatabaseResource>>();
        var databaseResource = Substitute.For<SqlDatabaseResource>();

        resourceGroupResource.GetSqlServerAsync(server).Returns(serverResponse);
        serverResponse.Value.Returns(serverResource);
        serverResource.GetSqlDatabaseAsync(database).Returns(databaseResponse);
        databaseResponse.Value.Returns(databaseResource);

        // Mock advisor collection and recommendations
        var advisorCollection = Substitute.For<SqlDatabaseAdvisorCollection>();
        var advisor = Substitute.For<SqlDatabaseAdvisorResource>();
        var advisorData = new SqlDatabaseAdvisorData("CreateIndex")
        {
            RecommendedActions = new List<RecommendedActionData>
            {
                new RecommendedActionData("Action1") { Details = "Details1" },
                new RecommendedActionData("Action2") { Details = "Details2" }
            }
        };
        advisor.Data.Returns(advisorData);

        var pageable = AsyncPageable<SqlDatabaseAdvisorResource>.FromPages(
            new[] { Page<SqlDatabaseAdvisorResource>.FromValues(new[] { advisor }, null, Substitute.For<Response>()) });

        advisorCollection.GetAllAsync().Returns(pageable);
        databaseResource.GetSqlDatabaseAdvisors().Returns(advisorCollection);

        // Act
        var result = await _sqlService.GetIndexRecommendationsAsync(
            database, server, resourceGroup, null, null, subscriptionId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Action1", result[0].Name);
        Assert.Equal("Details1", result[0].Description);
        Assert.Equal("Action2", result[1].Name);
        Assert.Equal("Details2", result[1].Description);
    }

    [Fact]
    public async Task GetIndexRecommendationsAsync_ThrowsWhenServerNotFound()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var server = "test-server";
        var database = "test-db";

        var subscriptionResource = Substitute.For<SubscriptionResource>();
        _subscriptionService.GetSubscription(subscriptionId).Returns(subscriptionResource);

        var resourceGroupResource = Substitute.For<ResourceGroupResource>();
        resourceGroupResource.GetSqlServerAsync(server).Returns((Response<SqlServerResource>)null);

        // Act & Assert
        await Assert.ThrowsAsync<AzureMcp.Services.Azure.Sql.SqlResourceNotFoundException>(() =>
            _sqlService.GetIndexRecommendationsAsync(database, server, resourceGroup, null, null, subscriptionId));
    }

    [Fact]
    public async Task ListServers_ReturnsServers()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var subscriptionResource = Substitute.For<SubscriptionResource>();
        _subscriptionService.GetSubscription(subscriptionId).Returns(subscriptionResource);

        // Mock subscription resource and servers
        var server1 = Substitute.For<SqlServerResource>();
        var server2 = Substitute.For<SqlServerResource>();
        var serverData1 = new SqlServerData(AzureLocation.WestUS2)
        {
            // Only set properties that are not read-only
        };
        typeof(SqlServerData).GetProperty("Name")?.SetValue(serverData1, "server1");
        var serverData2 = new SqlServerData(AzureLocation.WestUS2);
        typeof(SqlServerData).GetProperty("Name")?.SetValue(serverData2, "server2");
        server1.Data.Returns(serverData1);
        server2.Data.Returns(serverData2);

        var pageable = Pageable<SqlServerResource>.FromPages(
            new[] { Page<SqlServerResource>.FromValues(new[] { server1, server2 }, null, Substitute.For<Response>()) });

        subscriptionResource.GetSqlServers().Returns(pageable);

        // Act
        var result = await _sqlService.ListServers(subscriptionId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("server1", result);
        Assert.Contains("server2", result);
    }

    [Fact]
    public async Task ListDatabases_ReturnsDatabases()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var server = "test-server";

        var subscriptionResource = Substitute.For<SubscriptionResource>();
        _subscriptionService.GetSubscription(subscriptionId).Returns(subscriptionResource);

        // Mock ResourceGroupResource and server
        var resourceGroupResource = Substitute.For<ResourceGroupResource>();
        var serverResponse = Substitute.For<Response<SqlServerResource>>();
        var serverResource = Substitute.For<SqlServerResource>();

        resourceGroupResource.GetSqlServerAsync(server).Returns(serverResponse);
        serverResponse.Value.Returns(serverResource);

        // Mock databases
        var db1 = Substitute.For<SqlDatabaseResource>();
        var db2 = Substitute.For<SqlDatabaseResource>();
        var dbData1 = new SqlDatabaseData(AzureLocation.WestUS2);
        typeof(SqlDatabaseData).GetProperty("Name")?.SetValue(dbData1, "db1");
        var dbData2 = new SqlDatabaseData(AzureLocation.WestUS2);
        typeof(SqlDatabaseData).GetProperty("Name")?.SetValue(dbData2, "db2");
        db1.Data.Returns(dbData1);
        db2.Data.Returns(dbData2);

        var pageable = AsyncPageable<SqlDatabaseResource>.FromPages(
            new[] { Page<SqlDatabaseResource>.FromValues(new[] { db1, db2 }, null, Substitute.For<Response>()) });

        var dbCollection = Substitute.For<SqlDatabaseCollection>();
        dbCollection.GetAllAsync().Returns(pageable);
        serverResource.GetSqlDatabases().Returns(dbCollection);

        // Act
        var result = await _sqlService.ListDatabases(server, resourceGroup, subscriptionId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("db1", result);
        Assert.Contains("db2", result);
    }

    [Fact]
    public async Task ListDatabases_ThrowsWhenServerNotFound()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var server = "test-server";

        var subscriptionResource = Substitute.For<SubscriptionResource>();
        _subscriptionService.GetSubscription(subscriptionId).Returns(subscriptionResource);

        var resourceGroupResource = Substitute.For<ResourceGroupResource>();
        resourceGroupResource.GetSqlServerAsync(server).Returns((Response<SqlServerResource>)null);

        // Act & Assert
        await Assert.ThrowsAsync<AzureMcp.Services.Azure.Sql.SqlResourceNotFoundException>(() =>
            _sqlService.ListDatabases(server, resourceGroup, subscriptionId));
    }

    [Fact]
    public async Task ListServers_HandlesException()
    {
        // Arrange
        var subscription = "sub123";
        _subscriptionService.GetSubscription(subscription).Throws(new Exception("Test error"));

        // Act & Assert
        await Assert.ThrowsAsync<SqlServiceException>(() => _sqlService.ListServers(subscription));
    }

    [Fact]
    public async Task ListDatabases_HandlesException()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg123";
        var server = "server123";
        _subscriptionService.GetSubscription(subscription).Throws(new Exception("Test error"));

        // Act & Assert
        await Assert.ThrowsAsync<SqlServiceException>(() =>
            _sqlService.ListDatabases(server, resourceGroup, subscription));
    }

    private static SqlServerResource CreateMockSqlServer(string name)
    {
        var server = Substitute.For<SqlServerResource>();
        var serverData = new SqlServerData { Name = name };
        server.Data.Returns(serverData);
        return server;
    }

    private static SqlDatabaseResource CreateMockSqlDatabase(string name)
    {
        var database = Substitute.For<SqlDatabaseResource>();
        var databaseData = new SqlDatabaseData { Name = name };
        database.Data.Returns(databaseData);
        return database;
    }
} 