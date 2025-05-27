// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Commands;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
