// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Services.Interfaces;

/// <summary>
/// Thrown when a SQL resource is not found or cannot be accessed.
/// </summary>
public class SqlResourceNotFoundException : Exception
{
    public SqlResourceNotFoundException(string message) : base(message) { }
}

/// <summary>
/// Thrown when SQL authorization fails.
/// </summary>
public class SqlAuthorizationException : Exception
{
    public SqlAuthorizationException(string message) : base(message) { }
}
