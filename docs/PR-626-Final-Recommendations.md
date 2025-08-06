# PR #626 Final Recommendations - Deploy and Quota Commands

## Executive Summary

This document consolidates all analysis, feedback, and recommendations for PR #626 which introduces deployment and quota management commands to Azure MCP Server. Based on architectural review, standards compliance analysis, and stakeholder feedback, this document provides the definitive refactoring plan.

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Final Architecture Recommendations](#final-architecture-recommendations)
3. [Command Structure Reorganization](#command-structure-reorganization)
4. [Integration with Existing Commands](#integration-with-existing-commands)
5. [Implementation Action Plan](#implementation-action-plan)
6. [Test Scenarios](#test-scenarios)
7. [Validation Criteria](#validation-criteria)

## Current State Analysis

### PR Overview
- **Files Added**: 105 files with 6,521 additions and 44 deletions
- **New Areas**: `deploy` and `quota` command areas
- **Current Commands**: 7 commands with hyphenated naming and flat registration

### Standards Violations Identified
1. **Command Registration**: Uses flat `AddCommand()` instead of hierarchical `CommandGroup` pattern
2. **Command Naming**: Hyphenated names (`plan-get`, `iac-rules-get`) violate `<objects/resources> <verb>` pattern
3. **Architecture**: Overlaps with existing `AzCommand` and `AzdCommand` in `areas/extension/`

### Existing Extension Commands
The codebase contains:
- `areas/extension/src/AzureMcp.Extension/Commands/AzCommand.cs` - Full Azure CLI execution
- `areas/extension/src/AzureMcp.Extension/Commands/AzdCommand.cs` - Full AZD execution

## Final Architecture Recommendations

### Command Groups

Organize tools into these command groups:

1. **`quota`** - Resource quota checking and usage analysis
2. **`deploy azd`** - AZD-specific deployment tools
3. **`deploy az`** - Azure CLI-specific deployment tools
4. **`deploy diagrams`** - Architecture diagram generation

### Command Structure Changes

**From Current**:
```bash
azmcp deploy plan-get
azmcp deploy iac-rules-get
azmcp deploy azd-app-log-get
azmcp deploy cicd-pipeline-guidance-get
azmcp deploy architecture-diagram-generate
azmcp quota usage-get
azmcp quota available-region-list
```

**To Target**:
```bash
azmcp quota usage check
azmcp quota region availability list
azmcp deploy app logs get
azmcp deploy infrastructure rules get
azmcp deploy pipeline guidance get
azmcp deploy plan get
azmcp deploy architecture diagram generate
```

### Integration Strategy

Integration with existing commands:
- **AZD Operations**: Use existing `azmcp extension azd` internally
- **Azure CLI Operations**: Use existing `azmcp extension az` internally
- **Value-Added Services**: PR commands provide structured guidance on top of base CLI

## Command Structure Reorganization

### Deploy Area Refactoring

**File**: `areas/deploy/src/AzureMcp.Deploy/DeploySetup.cs`

**Target Structure**:
```csharp
public static void RegisterCommands(CommandGroup deploy)
{
    // Application-specific commands
    var appGroup = new CommandGroup("app", "Application-specific deployment tools");
    appGroup.AddCommand("logs", new LogsGetCommand(...)); // app logs get

    // Infrastructure as Code commands
    var infrastructureGroup = new CommandGroup("infrastructure", "Infrastructure as Code operations");
    infrastructureGroup.AddCommand("rules", new RulesGetCommand(...)); // infrastructure rules get

    // CI/CD Pipeline commands
    var pipelineGroup = new CommandGroup("pipeline", "CI/CD pipeline operations");
    pipelineGroup.AddCommand("guidance", new GuidanceGetCommand(...)); // pipeline guidance get

    // Deployment planning commands
    var planGroup = new CommandGroup("plan", "Deployment planning operations");
    planGroup.AddCommand("get", new GetCommand(...)); // plan get

    // Architecture diagram commands
    var architectureGroup = new CommandGroup("architecture", "Architecture diagram operations");
    architectureGroup.AddCommand("diagram", new DiagramGenerateCommand(...)); // architecture diagram generate

    deploy.AddCommandGroup(appGroup);
    deploy.AddCommandGroup(infrastructureGroup);
    deploy.AddCommandGroup(pipelineGroup);
    deploy.AddCommandGroup(planGroup);
    deploy.AddCommandGroup(architectureGroup);
}
```

### Quota Area Refactoring

**File**: `areas/quota/src/AzureMcp.Quota/QuotaSetup.cs`

**Target Structure**:
```csharp
public static void RegisterCommands(CommandGroup quota)
{
    // Resource usage and quota operations
    var usageGroup = new CommandGroup("usage", "Resource usage and quota operations");
    usageGroup.AddCommand("check", new CheckCommand(...)); // usage check

    // Region availability operations
    var regionGroup = new CommandGroup("region", "Region availability operations");
    regionGroup.AddCommand("availability", new AvailabilityListCommand(...)); // region availability list

    quota.AddCommandGroup(usageGroup);
    quota.AddCommandGroup(regionGroup);
}
```

### Command Name Property Updates

**Changes Required**:
1. `PlanGetCommand.Name` → `"get"` (was `"plan-get"`)
2. `IaCRulesGetCommand.Name` → `"rules"` (was `"iac-rules-get"`)
3. `AzdAppLogGetCommand.Name` → `"logs"` (was `"azd-app-log-get"`)
4. `PipelineGenerateCommand.Name` → `"guidance"` (was `"cicd-pipeline-guidance-get"`)
5. `GenerateArchitectureDiagramCommand.Name` → `"diagram"` (was `"architecture-diagram-generate"`)
6. `UsageCheckCommand.Name` → `"check"` (was `"usage-get"`)
7. `RegionCheckCommand.Name` → `"availability"` (was `"available-region-list"`)

## File and Folder Reorganization

### Deploy Area File Structure Changes

#### Current Structure:
```
areas/deploy/src/AzureMcp.Deploy/
├── Commands/
│   ├── AzdAppLogGetCommand.cs
│   ├── GenerateArchitectureDiagramCommand.cs
│   ├── IaCRulesGetCommand.cs
│   ├── PipelineGenerateCommand.cs
│   └── PlanGetCommand.cs
├── Options/
│   ├── AzdAppLogOptions.cs
│   ├── GenerateArchitectureDiagramOptions.cs
│   ├── IaCRulesOptions.cs
│   ├── PipelineGenerateOptions.cs
│   └── PlanGetOptions.cs
└── Services/
    └── [various service files]
```

#### Target Structure (Hierarchical Organization):
```
areas/deploy/src/AzureMcp.Deploy/
├── Commands/
│   ├── App/
│   │   └── LogsGetCommand.cs            (renamed from AzdAppLogGetCommand.cs)
│   ├── Infrastructure/
│   │   └── RulesGetCommand.cs           (renamed from IaCRulesGetCommand.cs)
│   ├── Pipeline/
│   │   └── GuidanceGetCommand.cs        (renamed from PipelineGenerateCommand.cs)
│   ├── Plan/
│   │   └── GetCommand.cs                (renamed from PlanGetCommand.cs)
│   └── Architecture/
│       └── DiagramGenerateCommand.cs    (renamed from GenerateArchitectureDiagramCommand.cs)
├── Options/
│   ├── App/
│   │   └── LogsGetOptions.cs            (renamed from AzdAppLogOptions.cs)
│   ├── Infrastructure/
│   │   └── RulesGetOptions.cs           (renamed from IaCRulesOptions.cs)
│   ├── Pipeline/
│   │   └── GuidanceGetOptions.cs        (renamed from PipelineGenerateOptions.cs)
│   ├── Plan/
│   │   └── GetOptions.cs                (renamed from PlanGetOptions.cs)
│   └── Architecture/
│       └── DiagramGenerateOptions.cs    (renamed from GenerateArchitectureDiagramOptions.cs)
├── Templates/                           (new directory)
│   ├── InfrastructureRulesTemplate.md
│   ├── PipelineGuidanceTemplate.md
│   └── DeploymentPlanTemplate.md
└── Services/
    ├── ITemplateService.cs              (new interface)
    ├── TemplateService.cs               (new implementation)
    └── [existing service files]
```

### Quota Area File Structure Changes

#### Current Structure:
```
areas/quota/src/AzureMcp.Quota/
├── Commands/
│   ├── RegionCheckCommand.cs
│   └── UsageCheckCommand.cs
├── Options/
│   ├── RegionCheckOptions.cs
│   └── UsageCheckOptions.cs
└── Services/
    └── [various service files]
```

#### Target Structure (Hierarchical Organization):
```
areas/quota/src/AzureMcp.Quota/
├── Commands/
│   ├── Usage/
│   │   └── CheckCommand.cs              (renamed from UsageCheckCommand.cs)
│   └── Region/
│       └── AvailabilityListCommand.cs   (renamed from RegionCheckCommand.cs)
├── Options/
│   ├── Usage/
│   │   └── CheckOptions.cs              (renamed from UsageCheckOptions.cs)
│   └── Region/
│       └── AvailabilityListOptions.cs   (renamed from RegionCheckOptions.cs)
└── Services/
    └── [existing service files]
```

### Detailed File Rename Mapping

#### Deploy Area Command Files:
| Current File | New File | New Class Name | Purpose |
|--------------|----------|----------------|---------|
| `Commands/AzdAppLogGetCommand.cs` | `Commands/App/LogsGetCommand.cs` | `LogsGetCommand` | Get logs from AZD-deployed applications |
| `Commands/IaCRulesGetCommand.cs` | `Commands/Infrastructure/RulesGetCommand.cs` | `RulesGetCommand` | Get Infrastructure as Code rules and guidelines |
| `Commands/PipelineGenerateCommand.cs` | `Commands/Pipeline/GuidanceGetCommand.cs` | `GuidanceGetCommand` | Get CI/CD pipeline guidance and configuration |
| `Commands/PlanGetCommand.cs` | `Commands/Plan/GetCommand.cs` | `GetCommand` | Generate Azure deployment plans |
| `Commands/GenerateArchitectureDiagramCommand.cs` | `Commands/Architecture/DiagramGenerateCommand.cs` | `DiagramGenerateCommand` | Generate Azure architecture diagrams |

#### Deploy Area Option Files:
| Current File | New File | New Class Name | Purpose |
|--------------|----------|----------------|---------|
| `Options/AzdAppLogOptions.cs` | `Options/App/LogsGetOptions.cs` | `LogsGetOptions` | Options for app log retrieval |
| `Options/IaCRulesOptions.cs` | `Options/Infrastructure/RulesGetOptions.cs` | `RulesGetOptions` | Options for IaC rules retrieval |
| `Options/PipelineGenerateOptions.cs` | `Options/Pipeline/GuidanceGetOptions.cs` | `GuidanceGetOptions` | Options for pipeline guidance |
| `Options/PlanGetOptions.cs` | `Options/Plan/GetOptions.cs` | `GetOptions` | Options for deployment planning |
| `Options/GenerateArchitectureDiagramOptions.cs` | `Options/Architecture/DiagramGenerateOptions.cs` | `DiagramGenerateOptions` | Options for diagram generation |

#### Quota Area Command Files:
| Current File | New File | New Class Name | Purpose |
|--------------|----------|----------------|---------|
| `Commands/UsageCheckCommand.cs` | `Commands/Usage/CheckCommand.cs` | `CheckCommand` | Check Azure resource usage and quotas |
| `Commands/RegionCheckCommand.cs` | `Commands/Region/AvailabilityListCommand.cs` | `AvailabilityListCommand` | List available regions for resource types |

#### Quota Area Option Files:
| Current File | New File | New Class Name | Purpose |
|--------------|----------|----------------|---------|
| `Options/UsageCheckOptions.cs` | `Options/Usage/CheckOptions.cs` | `CheckOptions` | Options for usage checking |
| `Options/RegionCheckOptions.cs` | `Options/Region/AvailabilityListOptions.cs` | `AvailabilityListOptions` | Options for region availability listing |

### Test File Updates Required

#### Deploy Area Test Files:
```
areas/deploy/tests/AzureMcp.Deploy.UnitTests/Commands/
├── App/
│   └── LogsGetCommandTests.cs           (update from AzdAppLogGetCommandTests.cs)
├── Infrastructure/
│   └── RulesGetCommandTests.cs          (update from IaCRulesGetCommandTests.cs)
├── Pipeline/
│   └── GuidanceGetCommandTests.cs       (update from PipelineGenerateCommandTests.cs)
├── Plan/
│   └── GetCommandTests.cs               (update from PlanGetCommandTests.cs)
└── Architecture/
    └── DiagramGenerateCommandTests.cs   (update from GenerateArchitectureDiagramCommandTests.cs)
```

#### Quota Area Test Files:
```
areas/quota/tests/AzureMcp.Quota.UnitTests/Commands/
├── Usage/
│   └── CheckCommandTests.cs             (update from UsageCheckCommandTests.cs)
└── Region/
    └── AvailabilityListCommandTests.cs  (update from RegionCheckCommandTests.cs)
```

### Namespace Updates Required

#### Deploy Area Namespaces:
- `AzureMcp.Deploy.Commands.App` for application-specific commands (logs)
- `AzureMcp.Deploy.Commands.Infrastructure` for Infrastructure as Code commands
- `AzureMcp.Deploy.Commands.Pipeline` for CI/CD pipeline commands
- `AzureMcp.Deploy.Commands.Plan` for deployment planning commands
- `AzureMcp.Deploy.Commands.Architecture` for architecture diagram commands
- `AzureMcp.Deploy.Options.App` for application command options
- `AzureMcp.Deploy.Options.Infrastructure` for infrastructure command options
- `AzureMcp.Deploy.Options.Pipeline` for pipeline command options
- `AzureMcp.Deploy.Options.Plan` for planning command options
- `AzureMcp.Deploy.Options.Architecture` for architecture command options
- `AzureMcp.Deploy.Services` for template and other services

#### Quota Area Namespaces:
- `AzureMcp.Quota.Commands.Usage` for usage-related commands
- `AzureMcp.Quota.Commands.Region` for region-related commands
- `AzureMcp.Quota.Options.Usage` for usage command options
- `AzureMcp.Quota.Options.Region` for region command options

### Project File Updates

#### Deploy Area Project File:
```xml
<!-- Add embedded resources for templates -->
<ItemGroup>
  <EmbeddedResource Include="Templates\*.md" />
</ItemGroup>

<!-- Add extension project reference -->
<ItemGroup>
  <ProjectReference Include="..\..\..\extension\src\AzureMcp.Extension\AzureMcp.Extension.csproj" />
</ItemGroup>
```

#### Quota Area Project File:
```xml
<!-- Add extension project reference -->
<ItemGroup>
  <ProjectReference Include="..\..\..\extension\src\AzureMcp.Extension\AzureMcp.Extension.csproj" />
</ItemGroup>
```

### Registration File Updates

#### DeploySetup.cs:
- Update `using` statements for new namespaces
- Update command registration to use `CommandGroup` hierarchy
- Register new `ITemplateService` and extension services

#### QuotaSetup.cs:
- Update `using` statements for new namespaces
- Update command registration to use `CommandGroup` hierarchy
- Register extension services

## Integration with Existing Commands

### Internal Service Integration

**Add Extension Dependencies**:
```xml
<!-- In Deploy and Quota project files -->
<ProjectReference Include="..\..\..\extension\src\AzureMcp.Extension\AzureMcp.Extension.csproj" />
```

**Service Registration**:
```csharp
// In area setup ConfigureServices methods
services.AddTransient<IAzService, AzService>();
services.AddTransient<IAzdService, AzdService>();
```

### Command Implementation Updates

**Example: AzdAppLogGetCommand using existing AzdCommand**:
```csharp
public sealed class AzdAppLogGetCommand(
    ILogger<AzdAppLogGetCommand> logger,
    IAzdService azdService) : SubscriptionCommand<AzdAppLogOptions>()
{
    public override string Name => "logs";

    protected override async Task<McpResult> ExecuteAsync(AzdAppLogOptions options, CancellationToken cancellationToken)
    {
        // Use existing AZD service to get environment info
        var envResult = await azdService.ExecuteAsync("env list", options.WorkspaceFolder);

        // Use existing AZD service to get logs
        var logsResult = await azdService.ExecuteAsync($"monitor logs --environment {options.AzdEnvName}", options.WorkspaceFolder);

        // Add value by filtering and formatting logs for specific app types
        var filteredLogs = FilterLogsForAppTypes(logsResult.Output);

        return McpResult.Success(filteredLogs);
    }
}
```

## Prompt Template Consolidation

### Template System Enhancement

**Objective**: Replace dynamic prompt construction with embedded markdown templates.

**Implementation**:
1. Create `areas/deploy/src/AzureMcp.Deploy/Templates/` directory
2. Extract prompts to markdown files:
   - `AzdRulesTemplate.md`
   - `PipelineGuidanceTemplate.md`
   - `DeploymentPlanTemplate.md`
3. Create injectable `ITemplateService` interface
4. Add embedded resources to project file

**Template Service Interface**:
```csharp
public interface ITemplateService
{
    Task<string> GetTemplateAsync(string templateName, object parameters = null);
}
```

### Deployment Planning Separation

**Split PlanGetCommand Responsibilities**:
- **Keep**: Project analysis and service recommendations
- **Add**: Next steps with specific tool commands
- **Remove**: Direct file generation (.azure/plan.copilotmd)

**Next Steps Response**:
```csharp
public class PlanAnalysisResult
{
    public string[] RecommendedServices { get; set; }
    public string[] NextStepCommands { get; set; }  // Specific azmcp commands to run
    public string DeploymentStrategy { get; set; }
    public string[] RequiredTools { get; set; }
}
```

## Implementation Action Plan

### Phase 1: Priority 0 (Must Complete First)

#### 1.1 Command Registration Refactoring
- **Priority**: P0
- **Files**: `DeploySetup.cs`, `QuotaSetup.cs`
- **Action**: Replace flat registration with `CommandGroup` hierarchy
- **Validation**: Commands accessible via new structure

#### 1.2 Command Name Updates
- **Priority**: P0
- **Files**: All command class files
- **Action**: Update `Name` properties to single verbs
- **Validation**: Unit tests pass with new names

#### 1.3 Build Verification
- **Priority**: P0
- **Action**: `dotnet build AzureMcp.sln`
- **Expected**: Zero compilation errors

### Phase 2: Integration and Enhancement

#### 2.1 Extension Service Integration
- **Priority**: P1
- **Action**: Add project references and service injection
- **Validation**: Commands use existing Az/Azd services internally

#### 2.2 Test Updates
- **Priority**: P1
- **Action**: Update unit and live tests for new structure
- **Validation**: All tests pass

### Phase 3: Optional Enhancements

#### 3.1 Template System
- **Priority**: P2
- **Action**: Extract prompts to embedded resources
- **Validation**: Template loading works correctly

#### 3.2 Documentation
- **Priority**: P2
- **Action**: Update `azmcp-commands.md` and `new-command.md`
- **Validation**: Documentation reflects new structure

## Test Scenarios

### Comprehensive Manual Test Cases (30 Scenarios)

#### Command Registration and Help Tests

1. **Deploy Command Group Help**
   - **Command**: `azmcp deploy --help`
   - **Expected**: Shows deploy subcommands (app, infrastructure, pipeline, plan, architecture)
   - **Validation**: All 5 subcommand groups are listed

2. **Quota Command Group Help**
   - **Command**: `azmcp quota --help`
   - **Expected**: Shows quota subcommands (usage, region)
   - **Validation**: Both subcommand groups are listed

3. **Deploy App Commands Help**
   - **Command**: `azmcp deploy app --help`
   - **Expected**: Shows app subcommands (logs)
   - **Validation**: logs command is available

4. **Deploy Infrastructure Commands Help**
   - **Command**: `azmcp deploy infrastructure --help`
   - **Expected**: Shows infrastructure subcommands (rules)
   - **Validation**: rules command is available

5. **Deploy Pipeline Commands Help**
   - **Command**: `azmcp deploy pipeline --help`
   - **Expected**: Shows pipeline subcommands (guidance)
   - **Validation**: guidance command is available

#### Quota Command Tests

6. **Quota Usage Check - Valid Subscription**
   - **Command**: `azmcp quota usage check --subscription-id 12345678-1234-1234-1234-123456789abc`
   - **Expected**: Returns quota usage information for the subscription
   - **Validation**: JSON output with quota data

7. **Quota Usage Check - Invalid Subscription**
   - **Command**: `azmcp quota usage check --subscription-id invalid-sub-id`
   - **Expected**: Returns authentication or validation error
   - **Validation**: Clear error message about invalid subscription

8. **Region Availability List - Specific Resource**
   - **Command**: `azmcp quota region availability list --resource-type Microsoft.Compute/virtualMachines`
   - **Expected**: Returns list of regions where VMs are available
   - **Validation**: JSON array of region names

9. **Region Availability List - All Resources**
   - **Command**: `azmcp quota region availability list`
   - **Expected**: Returns general region availability information
   - **Validation**: Comprehensive region data

10. **Quota Usage Check with Resource Group**
    - **Command**: `azmcp quota usage check --subscription-id 12345678-1234-1234-1234-123456789abc --resource-group myapp-rg`
    - **Expected**: Returns quota usage filtered to resource group
    - **Validation**: Scoped quota information

#### Deploy App Commands Tests

11. **App Logs Get - Valid AZD Environment**
    - **Command**: `azmcp deploy app logs get --workspace-folder ./myapp --azd-env-name dev`
    - **Expected**: Returns application logs from AZD-deployed environment
    - **Validation**: Log entries with timestamps

12. **App Logs Get - Invalid Environment**
    - **Command**: `azmcp deploy app logs get --workspace-folder ./myapp --azd-env-name nonexistent`
    - **Expected**: Returns error about environment not found
    - **Validation**: Clear error message

13. **App Logs Get - No AZD Project**
    - **Command**: `azmcp deploy app logs get --workspace-folder ./empty-folder`
    - **Expected**: Returns error about missing AZD project
    - **Validation**: Error indicates no azure.yaml found

14. **App Logs Get with Service Filter**
    - **Command**: `azmcp deploy app logs get --workspace-folder ./myapp --azd-env-name dev --service-name api`
    - **Expected**: Returns logs filtered to specific service
    - **Validation**: Logs only from specified service

#### Deploy Infrastructure Commands Tests

15. **Infrastructure Rules Get - Bicep Project**
    - **Command**: `azmcp deploy infrastructure rules get --workspace-folder ./bicep-project`
    - **Expected**: Returns Bicep-specific IaC rules and recommendations
    - **Validation**: Rules specific to Bicep templates

16. **Infrastructure Rules Get - Terraform Project**
    - **Command**: `azmcp deploy infrastructure rules get --workspace-folder ./terraform-project`
    - **Expected**: Returns Terraform-specific IaC rules and recommendations
    - **Validation**: Rules specific to Terraform configuration

17. **Infrastructure Rules Get - Mixed Project**
    - **Command**: `azmcp deploy infrastructure rules get --workspace-folder ./mixed-project`
    - **Expected**: Returns rules for detected IaC types
    - **Validation**: Rules for multiple IaC frameworks

18. **Infrastructure Rules Get - No IaC Project**
    - **Command**: `azmcp deploy infrastructure rules get --workspace-folder ./no-iac`
    - **Expected**: Returns general IaC guidance and getting started information
    - **Validation**: General recommendations

#### Deploy Pipeline Commands Tests

19. **Pipeline Guidance Get - GitHub Project**
    - **Command**: `azmcp deploy pipeline guidance get --workspace-folder ./github-project`
    - **Expected**: Returns GitHub Actions CI/CD pipeline guidance
    - **Validation**: GitHub-specific workflow recommendations

20. **Pipeline Guidance Get - Azure DevOps Project**
    - **Command**: `azmcp deploy pipeline guidance get --workspace-folder ./azdo-project`
    - **Expected**: Returns Azure DevOps pipeline guidance
    - **Validation**: Azure Pipelines YAML recommendations

21. **Pipeline Guidance Get - No VCS Project**
    - **Command**: `azmcp deploy pipeline guidance get --workspace-folder ./no-git`
    - **Expected**: Returns general CI/CD guidance
    - **Validation**: Platform-agnostic recommendations

#### Deploy Plan Commands Tests

22. **Plan Get - .NET Project**
    - **Command**: `azmcp deploy plan get --workspace-folder ./dotnet-app`
    - **Expected**: Returns deployment plan specific to .NET applications
    - **Validation**: Recommendations for App Service or Container Apps

23. **Plan Get - Node.js Project**
    - **Command**: `azmcp deploy plan get --workspace-folder ./node-app`
    - **Expected**: Returns deployment plan specific to Node.js applications
    - **Validation**: Recommendations for suitable Azure services

24. **Plan Get - Python Project**
    - **Command**: `azmcp deploy plan get --workspace-folder ./python-app`
    - **Expected**: Returns deployment plan specific to Python applications
    - **Validation**: Service recommendations with Python support

25. **Plan Get - Complex Microservices Project**
    - **Command**: `azmcp deploy plan get --workspace-folder ./microservices`
    - **Expected**: Returns multi-service deployment plan
    - **Validation**: Orchestration and service mesh recommendations

#### Deploy Architecture Commands Tests

26. **Architecture Diagram Generate - Simple App**
    - **Command**: `azmcp deploy architecture diagram generate --workspace-folder ./simple-app`
    - **Expected**: Returns Mermaid diagram for simple application architecture
    - **Validation**: Valid Mermaid syntax with basic components

27. **Architecture Diagram Generate - Microservices**
    - **Command**: `azmcp deploy architecture diagram generate --workspace-folder ./microservices`
    - **Expected**: Returns complex Mermaid diagram with multiple services
    - **Validation**: Comprehensive diagram with service relationships

28. **Architecture Diagram Generate with Custom Options**
    - **Command**: `azmcp deploy architecture diagram generate --workspace-folder ./myapp --include-networking --include-security`
    - **Expected**: Returns detailed diagram including network and security components
    - **Validation**: Enhanced diagram with additional layers

#### Error Handling and Edge Cases

29. **Invalid Command Structure (Legacy Format)**
    - **Command**: `azmcp deploy plan-get --workspace-folder ./myapp`
    - **Expected**: Command not found error
    - **Validation**: Clear error indicating command format change

30. **Missing Required Parameters**
    - **Command**: `azmcp quota usage check`
    - **Expected**: Returns error about missing required subscription parameter
    - **Validation**: Clear parameter requirement message

### Integration and Extension Service Tests

#### Extension Integration Tests

31. **AZD Service Integration**
    - **Scenario**: Verify deploy commands use existing AzdCommand internally
    - **Command**: `azmcp deploy app logs get --workspace-folder ./azd-project`
    - **Expected**: Command successfully executes AZD operations via extension
    - **Validation**: No duplication of AZD functionality

32. **Azure CLI Service Integration**
    - **Scenario**: Verify quota commands use existing AzCommand internally
    - **Command**: `azmcp quota usage check --subscription-id <valid-sub>`
    - **Expected**: Command successfully executes Azure CLI operations via extension
    - **Validation**: Structured output from Azure CLI data

### Performance and Reliability Tests

33. **Large Project Analysis**
    - **Command**: `azmcp deploy plan get --workspace-folder ./large-enterprise-app`
    - **Expected**: Command completes within reasonable time (< 30 seconds)
    - **Validation**: Response time and memory usage within limits

34. **Concurrent Command Execution**
    - **Scenario**: Run multiple commands simultaneously
    - **Commands**: Multiple instances of quota and deploy commands
    - **Expected**: All commands complete successfully without conflicts
    - **Validation**: No resource contention or errors

### Authentication and Authorization Tests

35. **Unauthenticated Azure Access**
    - **Command**: `azmcp quota usage check --subscription-id <valid-sub>`
    - **Expected**: Clear authentication error when not logged into Azure
    - **Validation**: Helpful error message with login instructions

36. **Insufficient Permissions**
    - **Command**: `azmcp quota usage check --subscription-id <restricted-sub>`
    - **Expected**: Permission denied error with clear explanation
    - **Validation**: Specific permission requirements listed

### Template and Output Format Tests

37. **JSON Output Format**
    - **Command**: `azmcp quota usage check --subscription-id <valid-sub> --output json`
    - **Expected**: Well-formed JSON response
    - **Validation**: Valid JSON structure

38. **Markdown Output Format**
    - **Command**: `azmcp deploy plan get --workspace-folder ./myapp --output markdown`
    - **Expected**: Formatted markdown response
    - **Validation**: Proper markdown structure with headers and lists

### Cross-Platform Tests

39. **Windows PowerShell Execution**
    - **Scenario**: Execute commands in Windows PowerShell environment
    - **Command**: All deploy and quota commands
    - **Expected**: Commands execute successfully on Windows
    - **Validation**: No platform-specific errors

40. **Linux/macOS Execution**
    - **Scenario**: Execute commands in bash/zsh environment
    - **Command**: All deploy and quota commands
    - **Expected**: Commands execute successfully on Unix-like systems
    - **Validation**: Cross-platform compatibility

### Copilot Natural Language Test Prompts

The following prompts can be used with GitHub Copilot or VS Code Copilot to test the deployment and quota functionality through natural language interactions. These prompts validate that the MCP tools are properly integrated and accessible through conversational interfaces.

#### Quota Management Prompts

1. **Basic Quota Check**
   - **Prompt**: "Check my Azure quota usage for subscription 12345678-1234-1234-1234-123456789abc"
   - **Expected**: Copilot uses `azmcp quota usage check` command
   - **Validation**: Returns structured quota information

2. **Region Availability Query**
   - **Prompt**: "What regions are available for virtual machines in Azure?"
   - **Expected**: Copilot uses `azmcp quota region availability list` command
   - **Validation**: Returns list of regions with VM availability

3. **Resource-Specific Quota**
   - **Prompt**: "Check quota limits for compute resources in my Azure subscription"
   - **Expected**: Copilot uses quota commands with appropriate filters
   - **Validation**: Returns compute-specific quota data

4. **Regional Capacity Planning**
   - **Prompt**: "I need to deploy 100 VMs - which Azure regions have capacity?"
   - **Expected**: Copilot uses region availability and quota commands
   - **Validation**: Provides capacity recommendations

#### Deployment Planning Prompts

5. **Application Deployment Planning**
   - **Prompt**: "Help me plan deployment for my .NET web application to Azure"
   - **Expected**: Copilot uses `azmcp deploy plan get` command
   - **Validation**: Returns deployment recommendations for .NET apps

6. **Microservices Architecture Planning**
   - **Prompt**: "Generate a deployment plan for my microservices application"
   - **Expected**: Copilot uses deployment planning tools
   - **Validation**: Returns multi-service deployment strategy

7. **Infrastructure as Code Guidance**
   - **Prompt**: "What are the best practices for Bicep templates in my project?"
   - **Expected**: Copilot uses `azmcp deploy infrastructure rules get` command
   - **Validation**: Returns Bicep-specific recommendations

8. **CI/CD Pipeline Setup**
   - **Prompt**: "Help me set up CI/CD for my GitHub project deploying to Azure"
   - **Expected**: Copilot uses `azmcp deploy pipeline guidance get` command
   - **Validation**: Returns GitHub Actions workflow recommendations

#### Architecture and Visualization Prompts

9. **Architecture Diagram Generation**
   - **Prompt**: "Create an architecture diagram for my application deployment"
   - **Expected**: Copilot uses `azmcp deploy architecture diagram generate` command
   - **Validation**: Returns Mermaid diagram

10. **Complex System Visualization**
    - **Prompt**: "Generate a detailed architecture diagram including networking and security"
    - **Expected**: Copilot uses diagram generation with enhanced options
    - **Validation**: Returns comprehensive diagram

#### Application Monitoring Prompts

11. **Application Log Analysis**
    - **Prompt**: "Show me logs from my AZD-deployed application in the dev environment"
    - **Expected**: Copilot uses `azmcp deploy app logs get` command
    - **Validation**: Returns filtered application logs

12. **Service-Specific Monitoring**
    - **Prompt**: "Get logs for the API service in my containerized application"
    - **Expected**: Copilot uses app logs command with service filtering
    - **Validation**: Returns service-specific log data

#### Multi-Step Workflow Prompts

13. **End-to-End Deployment Workflow**
    - **Prompt**: "I have a new Python app - help me deploy it to Azure from scratch"
    - **Expected**: Copilot uses multiple commands (plan, infrastructure, pipeline)
    - **Validation**: Provides step-by-step deployment guidance

14. **Capacity Planning Workflow**
    - **Prompt**: "Plan Azure resources for a high-traffic e-commerce application"
    - **Expected**: Copilot uses quota, planning, and architecture tools
    - **Validation**: Comprehensive capacity and architecture recommendations

15. **Troubleshooting Workflow**
    - **Prompt**: "My Azure deployment is failing - help me diagnose the issue"
    - **Expected**: Copilot uses logs, quota, and diagnostic commands
    - **Validation**: Systematic troubleshooting approach

#### Technology-Specific Prompts

16. **Node.js Application Deployment**
    - **Prompt**: "Deploy my Node.js Express app to Azure with best practices"
    - **Expected**: Copilot provides Node.js-specific deployment plan
    - **Validation**: Appropriate Azure service recommendations

17. **Container Deployment Strategy**
    - **Prompt**: "What's the best way to deploy my Docker containers to Azure?"
    - **Expected**: Copilot recommends container-specific Azure services
    - **Validation**: Container-optimized deployment strategy

18. **Database Integration Planning**
    - **Prompt**: "Plan deployment including a PostgreSQL database for my web app"
    - **Expected**: Copilot includes database services in deployment plan
    - **Validation**: Integrated database and application deployment

#### Error Handling and Edge Case Prompts

19. **Invalid Project Context**
    - **Prompt**: "Generate deployment plan for this empty folder"
    - **Expected**: Copilot handles missing project context gracefully
    - **Validation**: Appropriate error handling and guidance

20. **Authentication Issues**
    - **Prompt**: "Check my Azure quotas (when not authenticated)"
    - **Expected**: Copilot provides clear authentication guidance
    - **Validation**: Helpful error messages and login instructions

#### Advanced Integration Prompts

21. **Cross-Service Integration**
    - **Prompt**: "Plan deployment with Azure Functions, Cosmos DB, and App Service"
    - **Expected**: Copilot coordinates multiple Azure services
    - **Validation**: Integrated multi-service architecture

22. **Compliance and Security Focus**
    - **Prompt**: "Deploy my healthcare app with HIPAA compliance requirements"
    - **Expected**: Copilot emphasizes security and compliance features
    - **Validation**: Security-focused deployment recommendations

23. **Cost Optimization Planning**
    - **Prompt**: "Plan cost-effective Azure deployment for my startup application"
    - **Expected**: Copilot recommends cost-optimized services and configurations
    - **Validation**: Budget-conscious deployment strategy

24. **Scaling Strategy Development**
    - **Prompt**: "Plan Azure deployment that can scale from 1000 to 1 million users"
    - **Expected**: Copilot provides scalable architecture recommendations
    - **Validation**: Auto-scaling and performance considerations

25. **Multi-Environment Strategy**
    - **Prompt**: "Set up dev, staging, and production environments for my app"
    - **Expected**: Copilot provides multi-environment deployment strategy
    - **Validation**: Environment-specific configurations and pipelines

#### Integration Testing Prompts

26. **Tool Integration Validation**
    - **Prompt**: "Use Azure CLI to check my subscription then plan deployment"
    - **Expected**: Copilot seamlessly integrates existing AZ commands with new tools
    - **Validation**: No duplication of CLI functionality

27. **AZD Integration Testing**
    - **Prompt**: "Get logs from my azd-deployed application and plan next deployment"
    - **Expected**: Copilot uses existing AZD integration effectively
    - **Validation**: Proper integration with existing AZD commands

28. **Command Discovery Testing**
    - **Prompt**: "What deployment tools are available in this Azure MCP server?"
    - **Expected**: Copilot lists available deployment and quota commands
    - **Validation**: Complete tool discovery and explanation

#### Performance and Reliability Prompts

29. **Large Project Handling**
    - **Prompt**: "Analyze deployment requirements for this enterprise monorepo"
    - **Expected**: Copilot handles complex project analysis efficiently
    - **Validation**: Reasonable response time and comprehensive analysis

30. **Concurrent Operation Testing**
    - **Prompt**: "Check quotas while generating architecture diagram and planning deployment"
    - **Expected**: Copilot handles multiple concurrent operations
    - **Validation**: All operations complete successfully without conflicts

### Expected Copilot Behavior Patterns

When testing with these prompts, validate that Copilot:

1. **Command Selection**: Chooses appropriate azmcp commands based on user intent
2. **Parameter Handling**: Correctly infers or prompts for required parameters
3. **Error Handling**: Provides helpful guidance when commands fail
4. **Integration**: Uses existing extension commands when appropriate
5. **Output Processing**: Formats and explains command results clearly
6. **Follow-up Actions**: Suggests logical next steps after command execution
7. **Context Awareness**: Considers project structure and environment in recommendations

## Validation Criteria

### Build and Test Requirements

- [ ] `dotnet build AzureMcp.sln` succeeds with zero errors
- [ ] All unit tests pass
- [ ] Live tests pass (when Azure credentials available)
- [ ] CLI help commands work for all new structures

### Command Structure Compliance

- [ ] All commands follow `<objects/resources> <verb>` pattern
- [ ] No hyphenated command names
- [ ] Hierarchical `CommandGroup` registration used
- [ ] Command names are single verbs (`get`, `list`, `generate`, etc.)

### Integration Requirements

- [ ] Deploy commands use existing Extension services internally
- [ ] No duplication of Az/Azd CLI functionality
- [ ] Value-added services provide structured guidance
- [ ] Clear differentiation between guided vs direct CLI access

### Documentation Standards

- [ ] All commands documented in `azmcp-commands.md`
- [ ] Examples show new command structure
- [ ] Migration notes for changed command names
- [ ] Integration patterns documented in `new-command.md`

## Post-Implementation Considerations

### Future Architecture Evolution

1. **AZD MCP Server Migration**: When Azure Developer CLI creates their own MCP server, evaluate migrating AZD-specific tools
2. **Template System Enhancement**: Expand template system for more dynamic content generation
3. **Cross-Area Integration**: Explore integration between deploy and quota areas
4. **Performance Optimization**: Cache quota information and template loading

### Monitoring and Metrics

1. **Command Usage**: Track which new commands are most/least used
2. **Error Patterns**: Monitor common failure scenarios for improvement
3. **Integration Success**: Measure successful extension service integration
4. **User Feedback**: Collect feedback on new command structure

## Conclusion

This refactoring plan addresses identified standards violations while preserving the deployment and quota management capabilities introduced in PR #626. The changes include:

1. **Proper Command Structure**: Hierarchical `CommandGroup` registration following established patterns
2. **Standard Naming**: `<objects/resources> <verb>` pattern without hyphens
3. **Integration**: Leverage existing Extension commands to avoid duplication
4. **Value-Added Services**: Focus on structured guidance and templates rather than raw CLI access

The implementation will proceed with the priority 0 items first to ensure build stability, followed by integration enhancements and optional improvements. This approach maintains the capabilities while aligning with repository standards and architectural patterns.
