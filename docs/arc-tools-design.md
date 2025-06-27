# Technical Design Document: Arc Tools in Azure MCP

# Introduction

## Overview
This document outlines the technical design for building and integrating Arc tools into the Azure MCP framework. The tools will enable users to manage Azure Arc-enabled Kubernetes clusters and perform operations such as cluster creation, connection to Azure Arc, and installation of extensions.

## Objectives
- Provide a comprehensive framework for managing Azure Arc-enabled Kubernetes clusters.
- Enable seamless integration of new tools and features.
- Ensure scalability, security, and usability of the tools.

## Existing Tools
The following tools are already implemented in Azure MCP:

### Cosmos Tools
- **List Databases**: Retrieves all databases in a Cosmos DB account.
- **List Containers**: Retrieves all containers within a database.
- **Query Items**: Executes SQL queries against items in a container.

### Kusto Tools
- **List Clusters**: Retrieves all Kusto clusters in a subscription.
- **Get Cluster Details**: Fetches detailed information about a specific cluster.
- **List Databases**: Retrieves all databases in a Kusto cluster.
- **List Tables**: Retrieves all tables in a database.
- **Get Table Schema**: Fetches the schema of a specific table.
- **Execute Query**: Runs KQL queries against a database.

### Monitor Tools
- **List Tables**: Retrieves all tables in a Log Analytics workspace.
- **Query Logs**: Executes KQL queries against logs in a workspace.

### Storage Tools
- **List Containers**: Retrieves all containers in a storage account.
- **List Blobs**: Retrieves all blobs in a container.
- **List Tables**: Retrieves all tables in a storage account.

### Redis Tools
- **List Caches**: Retrieves all Redis caches in a subscription.
- **List Databases**: Retrieves all databases in a Redis cluster.

### Service Bus Tools
- **Get Queue Details**: Fetches details about a specific queue.
- **Get Topic Details**: Fetches details about a specific topic.
- **Get Subscription Details**: Fetches details about a specific subscription.

### Arc Tools
- **List Clusters**: Retrieves all Azure Arc-enabled Kubernetes clusters.
- **Get Cluster Details**: Fetches detailed information about a specific cluster.
- **Configure Cluster**: Applies configuration changes to a cluster.

## New Features
The following features will be added to enhance the Arc tools:

### AKS Cluster Creation
- **Description**: Creates a new Azure Kubernetes Service (AKS) cluster with a system-assigned managed identity.
- **Implementation**:
  - Command: `arc create-aks`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKS cluster.
    - `--resource-group`: Resource group for the cluster.
    - `--region`: Azure region for the cluster.

### Connect to Azure Arc
- **Description**: Connects an existing AKS cluster to Azure Arc.
- **Implementation**:
  - Command: `arc connect-arc`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKS cluster.
    - `--resource-group`: Resource group for the cluster.
    - `--location`: Azure region for the Arc resource.

### Install ACSA
- **Description**: Installs Azure Container Storage for Arc.
- **Implementation**:
  - Command: `arc install-acsa`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKS cluster.
    - `--resource-group`: Resource group for the cluster.

### Install AIO
- **Description**: Installs Azure IoT Operations Platform.
- **Implementation**:
  - Command: `arc install-aio`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKS cluster.
    - `--resource-group`: Resource group for the cluster.

### Install Secret Sync
- **Description**: Installs Secret Sync Service.
- **Implementation**:
  - Command: `arc install-secret-sync`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKS cluster.
    - `--resource-group`: Resource group for the cluster.

### Create AKSEE Cluster
- **Description**: Creates a new Azure Kubernetes Service Edge Essentials (AKSEE) cluster.
- **Implementation**:
  - Command: `arc create-aksee`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKSEE cluster.
    - `--resource-group`: Resource group for the cluster.
    - `--region`: Azure region for the cluster.

### Connect AKSEE to Azure Arc
- **Description**: Connects an existing AKSEE cluster to Azure Arc.
- **Implementation**:
  - Command: `arc connect-aksee`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKSEE cluster.
    - `--resource-group`: Resource group for the cluster.
    - `--location`: Azure region for the Arc resource.

### Deletes Cluster and VM Completely
- **Description**: Deletes a cluster and its associated virtual machines completely.
- **Implementation**:
  - Command: `arc remove-cluster-vm`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.

### Diagnose and Fix Cluster
- **Description**: Diagnoses issues in a cluster and applies fixes automatically.
- **Implementation**:
  - Command: `arc diagnose-fix-cluster`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.

### Remove Cluster from Resource Group
- **Description**: Removes a cluster from its resource group.
- **Implementation**:
  - Command: `arc remove-cluster-resource-group`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.

### Install Azure Monitor
- **Description**: Installs Azure Monitor extension for Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc install-monitor`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKS cluster.
    - `--resource-group`: Resource group for the cluster.

### Install Azure Policy
- **Description**: Installs Azure Policy extension for Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc install-policy`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKS cluster.
    - `--resource-group`: Resource group for the cluster.

### Install GitOps
- **Description**: Installs GitOps extension for Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc install-gitops`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the AKS cluster.
    - `--resource-group`: Resource group for the cluster.

## Northstar Goal

The following advanced and future Arc tools are proposed to further enhance the Azure Arc in MCP framework:

### Scale/Upgrade Cluster
- **Description**: Enables scaling and upgrading of Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc scale-upgrade-cluster`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.
    - `--scale`: Desired scale level.
    - `--upgrade`: Upgrade version.

### Monitor Health
- **Description**: Monitors the health of Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc monitor-health`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.

### Enable Logging
- **Description**: Enables logging for Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc enable-logging`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.

### Security/Audit
- **Description**: Provides security and audit capabilities for Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc security-audit`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.

### Backup/Restore
- **Description**: Enables backup and restore functionality for Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc backup-restore`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.
    - `--backup-location`: Location for storing backups.

### Extension Management
- **Description**: Manages extensions for Azure Arc-enabled Kubernetes clusters.
- **Implementation**:
  - Command: `arc manage-extensions`
  - Arguments:
    - `--subscription`: Azure subscription ID.
    - `--cluster-name`: Name of the cluster.
    - `--resource-group`: Resource group for the cluster.
    - `--install`: Install a new extension.
    - `--remove`: Remove an existing extension.

## Design Considerations
- **Scalability**: Ensure tools can handle large-scale operations across multiple clusters and subscriptions.
- **Security**: Use Azure authentication mechanisms to secure operations.
- **Usability**: Provide clear error messages and documentation for each tool.

## Implementation Steps

### 1. **Add the Service Method**
- In ArcService.cs, add your new method (e.g., `InstallAioPlatformExtensionAsync`).
- If you want to call it via DI, add the method signature to `IArcService` and implement it as an instance method.

---

### 2. **Create the Command Class**
- In Arc, create a new command class (e.g., `InstallAIOCommand.cs`).
- Inherit from `BaseClusterCommand<BaseClusterOptions>` (or a custom options class if needed).
- Implement `ExecuteAsync` to call your new service method and wrap the result in a result record.

---

### 3. **(Optional) Create an Options Class**
- If your command needs extra options, create a new options class in Arc inheriting from `BaseClusterOptions`.
- Otherwise, use `BaseClusterOptions` directly.

---

### 4. **Update ArcJsonContext**
- In ArcJsonContext.cs, add a `[JsonSerializable(typeof(YourCommand.YourResultRecord))]` attribute for your result record to enable serialization.

---

### 5. **Register the Command**
- In CommandFactory.cs, inside `RegisterArcCommands()`, add:
  ```csharp
  cluster.AddCommand("your-command-name", new Arc.YourCommand(GetLogger<Arc.YourCommand>()));
  ```

---

### 6. **Rebuild and Restart**
- Run `dotnet build` to compile your changes.
- Restart the MCP server.

---

### 7. **Test the Command**
- Use the CLI, HTTP API, or VS Code agent mode to invoke and verify your new command.

---

### Summary Table

| Step | File/Location                  | Action                                                      |
|------|-------------------------------|-------------------------------------------------------------|
| 1    | ArcService.cs, IArcService.cs | Add/implement the service method                            |
| 2    | Commands/Arc/                 | Create the command class                                    |
| 3    | Options/Arc/                  | (Optional) Add a custom options class                       |
| 4    | Commands/Arc/ArcJsonContext.cs| Add `[JsonSerializable(...)]` for the result record         |
| 5    | CommandFactory.cs             | Register the command in `RegisterArcCommands()`             |
| 6    | —                             | Build and restart the server                                |
| 7    | —                             | Test via CLI, HTTP, or VS Code                              |

## Test Coverage

To ensure robust functionality and reliability of new Arc tools, the following test coverage requirements must be met:

### Unit Tests
- **Location:** `/tests/Commands/Arc/`
- **Purpose:** Validate individual command logic, argument parsing, and error handling.
- **Files to Update/Create:**
  - Add required unit test files

### Live/Integration Tests
- **Location:** `/tests/Client/`
- **Purpose:** Test interactions with Azure resources and ensure commands execute successfully in a live environment.
- **Files to Update/Create:**
  - `ClientToolTests.cs`
  - `CommandTests.cs`

### End-to-End (E2E) Tests
- **Location:** `/e2eTests/e2eTestPrompts.md`
- **Purpose:** Validate the complete workflow of new tools, including user prompts and tool invocation.
- **Files to Update/Create:**
  - Add at least one test prompt per new tool.

### Implementation Steps
1. **Unit Tests:**
   - Mock external services and dependencies.
   - Cover success scenarios, error handling, and argument validation.
2. **Live/Integration Tests:**
   - Use live Azure resources to validate command execution.
   - Ensure proper cleanup after tests.
3. **E2E Tests:**
   - Define test prompts for each new tool.
   - Validate tool invocation and output.

### Summary Table
| Test Type       | Location                  | Files to Update/Create                  |
|-----------------|---------------------------|-----------------------------------------|
| Unit Tests      | `/tests/Commands/Arc/`    | `ClusterConfigureCommandTest.cs`, `ClusterListCommandTest.cs` |
| Live Tests      | `/tests/Client/`          | `ClientToolTests.cs`, `CommandTests.cs` |
| E2E Tests       | `/e2eTests/`              | `e2eTestPrompts.md`                     |

## Conclusion
The integration of Arc tools into Azure MCP will provide users with powerful capabilities to manage Azure Arc-enabled Kubernetes clusters efficiently. The addition of AKS cluster creation and Arc connection features will further enhance the framework's functionality.
