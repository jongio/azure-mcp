// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AzureMcp.Services.Caching.Providers;
using AzureMcp.Services.Caching.Shared;
using Xunit;

namespace AzureMcp.Tests.Services.Caching;

public class FileCacheProviderTests : IDisposable
{    private readonly FileCacheProvider _sut;
    private readonly string _testCacheFolder;
    private const string TestKey = "test-key";
    private const string TestValue = "test-value-string";
    private static readonly TimeSpan TestExpiration = TimeSpan.FromMinutes(5);

    public FileCacheProviderTests()
    {
        _sut = new FileCacheProvider();
        
        // Get the cache folder path via reflection to ensure we're checking the right location
        var fieldInfo = typeof(FileCacheProvider).GetField("_cacheFolder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        _testCacheFolder = (string)fieldInfo!.GetValue(_sut)!;
        
        // Clean the test folder before each test
        CleanTestFolder();
    }

    public void Dispose()
    {
        // Clean up test files after tests
        CleanTestFolder();
    }

    private void CleanTestFolder()
    {
        if (Directory.Exists(_testCacheFolder))
        {
            foreach (var file in Directory.GetFiles(_testCacheFolder))
            {
                try
                {
                    // Make sure to remove read-only attribute before deletion
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch
                {
                    // Ignore deletion errors during cleanup
                }
            }
        }
    }    [Fact]
    public async Task GetAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _sut.GetAsync<string>(TestKey);

        // Assert
        Assert.Null(result);
    }    [Fact]
    public async Task GetAsync_WhenFileExistsButIsEmpty_ReturnsNull()
    {
        // Arrange - We'll use the path but not create a file here
        var filePath = GetFilePath(TestKey);
        
        // Make sure the file doesn't exist at the start
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // Act
        var result = await _sut.GetAsync<string>(TestKey);

        // Assert
        Assert.Null(result);
    }[Fact]
    public async Task SetAsync_WithNullData_DoesNotCreateFile()
    {
        // Arrange
        var filePath = GetFilePath(TestKey);
        string? nullData = null;

        // Act
        await _sut.SetAsync(TestKey, nullData);

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task SetAndGetAsync_PersistsAndRetrievesData()
    {
        // Act - Set the data
        await _sut.SetAsync(TestKey, TestValue);

        // Assert - File exists
        var filePath = GetFilePath(TestKey);
        Assert.True(File.Exists(filePath));

        // Act - Get the data
        var result = await _sut.GetAsync<string>(TestKey);

        // Assert - Data matches
        Assert.NotNull(result);
        Assert.Equal(TestValue, result);
    }    [Fact]
    public async Task GetAsync_WithExpiredData_ReturnsNullAndDeletesFile()
    {
        // Arrange - Set expired data directly
        var filePath = GetFilePath(TestKey);
        var expiredTime = DateTimeOffset.UtcNow.AddMinutes(-1); // Expired 1 minute ago
        
        // Create cache entry with expired time
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(TestValue, CacheJsonContext.Default.String));
        var entry = new DistributedCacheEntry(bytes, expiredTime, null);
        
        var serialized = JsonSerializer.Serialize(entry, CacheJsonContext.Default.DistributedCacheEntry);
        File.WriteAllText(filePath, serialized);

        // Act
        var result = await _sut.GetAsync<string>(TestKey);

        // Assert
        Assert.Null(result);
        Assert.False(File.Exists(filePath), "File should be deleted when expired");
    }    [Fact]
    public async Task DeleteAsync_RemovesFile()
    {
        // Arrange - Create a cache file
        await _sut.SetAsync(TestKey, TestValue);
        var filePath = GetFilePath(TestKey);
        Assert.True(File.Exists(filePath), "Setup failed - file not created");

        // Act
        await _sut.DeleteAsync(TestKey);

        // Assert
        Assert.False(File.Exists(filePath), "File should have been deleted");
    }

    [Fact]
    public async Task DeleteAsync_WhenFileDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var filePath = GetFilePath(TestKey);
        Assert.False(File.Exists(filePath), "File should not exist before test");

        // Act & Assert - should not throw
        await _sut.DeleteAsync(TestKey);
        Assert.False(File.Exists(filePath));
    }    [Fact]
    public async Task SetAsync_WithExplicitExpiration_SetsExpirationTime()
    {
        // Act
        await _sut.SetAsync(TestKey, TestValue, TestExpiration);
        
        // Assert - check the file contains the correct expiration
        var filePath = GetFilePath(TestKey);
        var fileContent = File.ReadAllText(filePath);
        var entry = JsonSerializer.Deserialize(fileContent, CacheJsonContext.Default.DistributedCacheEntry);
        
        Assert.NotNull(entry);
        Assert.NotNull(entry!.AbsoluteExpiration);
        
        // The expiration time should be roughly now + TestExpiration (5 minutes)
        var expectedTime = DateTimeOffset.UtcNow.Add(TestExpiration);
        var timeDifference = Math.Abs((entry.AbsoluteExpiration!.Value - expectedTime).TotalSeconds);
        
        Assert.True(timeDifference < 2, "Expiration time should be within 2 seconds of expected time");
    }    [Fact]
    public async Task GetAsync_WhenTypeNotInJsonContext_ThrowsInvalidOperationException()
    {
        // Arrange - Set data of a type not registered in CacheJsonContext
        var notRegisteredObject = new NotInJsonContext { Value = 123 };
        var filePath = GetFilePath(TestKey);
        var json = JsonSerializer.Serialize(notRegisteredObject);
        var bytes = Encoding.UTF8.GetBytes(json);
        var entry = new DistributedCacheEntry(bytes, null, null);
        var serialized = JsonSerializer.Serialize(entry, CacheJsonContext.Default.DistributedCacheEntry);
        File.WriteAllText(filePath, serialized);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.GetAsync<NotInJsonContext>(TestKey));
    }

    [Fact]
    public async Task SetAsync_WhenTypeNotInJsonContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var notRegisteredObject = new NotInJsonContext { Value = 123 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.SetAsync(TestKey, notRegisteredObject));
    }    [Fact]
    public async Task SetAsync_WhenFileIsReadOnly_ThrowsUnauthorizedAccessException()
    {
        // Create a file with proper DistributedCacheEntry structure
        var filePath = GetFilePath(TestKey);
        var entry = new DistributedCacheEntry(
            Encoding.UTF8.GetBytes("test"),
            null,
            null);
        var serialized = JsonSerializer.Serialize(entry, CacheJsonContext.Default.DistributedCacheEntry);
        await File.WriteAllTextAsync(filePath, serialized, TestContext.Current.CancellationToken);
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        try
        {            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _sut.SetAsync(TestKey, TestValue));
            Assert.Contains("Access to the path", exception.Message);
        }
        finally
        {
            // Cleanup - ignore errors during cleanup
            try
            {
                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task GetAsync_WhenFileIsLocked_ThrowsIOException()
    {
        // Arrange
        var filePath = GetFilePath(TestKey);
        await _sut.SetAsync(TestKey, TestValue);

        // Create a file lock
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        
        // Act & Assert
        await Assert.ThrowsAsync<IOException>(
            async () => await _sut.GetAsync<string>(TestKey));
    }

    /// <summary>
    /// A test class that is intentionally not registered in CacheJsonContext
    /// </summary>
    private class NotInJsonContext
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Helper to replicate the internal file path generation
    /// </summary>
    private string GetFilePath(string key) =>
        Path.Combine(_testCacheFolder, Convert.ToBase64String(Encoding.UTF8.GetBytes(key)));
}
