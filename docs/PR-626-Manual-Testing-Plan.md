# PR #626 — Manual Testing Plan: 50 Copilot E2E Prompts

This document lists 50 natural-language prompts you can paste into Copilot to exercise the Deploy and Quota tools introduced by PR #626. Prompts are grouped by capability and phrased to encourage Copilot to invoke the corresponding azmcp commands using the new hierarchical structure.

Notes
- Scope: Only features introduced in PR #626 (Deploy and Quota tools). Do not use these prompts to validate unrelated/core azmcp functionality.
- These are prompts, not shell commands. Paste into Copilot chat and let it choose the appropriate tool invocation.
- Focus areas: deploy app logs get, deploy infrastructure rules get, deploy pipeline guidance get, deploy plan get, deploy architecture diagram generate, quota usage check, quota region availability list.

## Quota — Usage Check (Prompts 1–10)
1. Check my Azure quota usage for subscription 11111111-1111-1111-1111-111111111111 in eastus for Microsoft.Compute/virtualMachines.
2. How much VM quota is left in westus2 for my default subscription?
3. Show current usage and limits for Microsoft.ContainerRegistry/registries in eastus2.
4. What are my quotas for App Service plans in centralus? Include used vs. limit.
5. Check GPU VM quota availability in eastus for the Standard_NC series.
6. Give me compute, network, and storage quota usage across eastus and westus.
7. Validate if I can deploy 50 Standard_D4s_v5 VMs in eastus given my current quota.
8. Show quota usage details for Functions in westus (consumption and premium if available).
9. Check current limits for public IP addresses in canadacentral and how many are free.
10. For my staging subscription, list usage for Microsoft.DBforPostgreSQL/flexibleServers in westeurope.

## Quota — Region Availability (Prompts 11–20)
11. Which regions currently support Microsoft.Compute/virtualMachines?
12. List all regions where Container Apps are available.
13. Where is Azure Database for PostgreSQL Flexible Server available today?
14. What regions support Premium SSD v2 disks for VMs?
15. Show regions that allow Azure OpenAI deployments.
16. For AKS, give me a list of regions with availability and note any regional restrictions.
17. In which regions can I create App Service Linux plans?
18. Where are Cognitive Services available for vision features?
19. Which regions support Availability Zones for VM deployments?
20. List regions that currently offer zone-redundant Container Apps environments.

## Deploy — Plan Get (Prompts 21–30)
21. Help me plan deployment for my .NET web app in this repo; recommend Azure services and next steps.
22. Create a deployment plan for a Node.js Express API and a React frontend.
23. Generate a plan for a microservices solution with 5 containerized services and a Postgres database.
24. Propose an Azure deployment for a Python FastAPI backend with a static web frontend.
25. I need a cost-optimized plan for a startup MVP with low traffic that can scale later.
26. Draft a HIPAA-aware deployment plan for a healthcare app handling PHI.
27. Plan multi-environment (dev/staging/prod) deployment with environment separation and secrets.
28. Recommend an Azure approach for a background worker/queue processor and a public API.
29. Given no IaC present, suggest the simplest path to ship quickly with azd.
30. Provide a deployment plan tuned for high traffic (100k DAU) with CDN and autoscale.

## Deploy — Infrastructure Rules Get (Prompts 31–35)
31. Review my Bicep templates and list infrastructure best practices I should adopt.
32. Inspect Terraform in this repo and recommend Azure-specific improvements.
33. Provide baseline IaC rules for network security, tagging, and naming conventions.
34. What are best practices for storing secrets and connection strings in IaC?
35. Suggest IaC rules to prepare for multi-region failover and disaster recovery.

## Deploy — Pipeline Guidance Get (Prompts 36–40)
36. Generate CI/CD guidance for a GitHub repository that deploys a .NET API to Azure.
37. Recommend an Azure DevOps pipeline for building and deploying a container app.
38. I have a monorepo; what pipeline setup should I use for independent services?
39. Provide guidance for handling secrets, environment variables, and approvals in the pipeline.
40. Help me set up CI/CD that builds PRs, runs tests, and deploys on merges to main.

## Deploy — App Logs Get (Prompts 41–45)
41. Show application logs for my azd environment named dev from the API service in the last 30 minutes.
42. Fetch logs for the worker service in the staging environment to diagnose timeouts.
43. Get error-level logs only for the web frontend service for the past hour.
44. Retrieve combined logs for all services in the prod environment with timestamps.
45. Find exceptions related to database connectivity across services in the last 24 hours.

## Deploy — Architecture Diagram Generate (Prompts 46–50)
46. Generate a simple architecture diagram for this application showing web, API, and database.
47. Create an architecture diagram for a 3-service container app with an external database.
48. Produce a deployment diagram highlighting ingress, app services/containers, and storage.
49. Draw a diagram including a queue-based worker, the API, and a public web frontend.
50. Generate a diagram for a microservices layout with internal service-to-service calls and a shared VNet.

## Test Execution Log (Populate During Manual Run)

Instructions
- For each prompt, after executing via Copilot, mark Pass (✔) or Fail (✖). Leave the other column blank.
- If a test is blocked (environment/setup), write BLOCKED in Notes with reason.
- If Fail, include the observed command/tool invocation (or absence) and error snippet.
- Use additional rows if you repeat a prompt under different conditions (append .1, .2 to ID).

| # | Prompt Summary | Pass | Fail | Notes |
|---|----------------|------|------|-------|
| 1 | Quota usage VM eastus specific sub + resource type | | | |
| 2 | VM quota westus2 default subscription | | | |
| 3 | Quota ContainerRegistry eastus2 | | | |
| 4 | App Service plan quotas centralus | | | |
| 5 | GPU VM (Standard_NC) quota eastus | | | |
| 6 | Compute/network/storage quotas eastus+westus | | | |
| 7 | Validate deploy 50 D4s_v5 eastus | | | |
| 8 | Functions quota westus consumption/premium | | | |
| 9 | Public IP quota canadacentral | | | |
| 10 | PostgreSQL flexible servers westeurope | | | |
| 11 | Regions support virtualMachines | | | |
| 12 | Regions Container Apps | | | |
| 13 | Regions PostgreSQL Flexible Server | | | |
| 14 | Regions Premium SSD v2 disks | | | |
| 15 | Regions Azure OpenAI | | | |
| 16 | Regions AKS availability + restrictions | | | |
| 17 | Regions App Service Linux plans | | | |
| 18 | Regions Cognitive Services vision | | | |
| 19 | Regions VM Availability Zones support | | | |
| 20 | Regions zone-redundant Container Apps env | | | |
| 21 | Deployment plan .NET web app | | | |
| 22 | Plan Node.js API + React frontend | | | |
| 23 | Plan microservices 5 containers + Postgres | | | |
| 24 | Plan FastAPI + static web | | | |
| 25 | Cost-optimized MVP scalable | | | |
| 26 | HIPAA-aware plan PHI | | | |
| 27 | Multi-env dev/stage/prod separation | | | |
| 28 | Background worker + queue + public API | | | |
| 29 | No IaC simplest azd path | | | |
| 30 | High traffic 100k DAU with CDN autoscale | | | |
| 31 | Review Bicep infra best practices | | | |
| 32 | Inspect Terraform Azure improvements | | | |
| 33 | Baseline IaC rules security/tagging/naming | | | |
| 34 | Secrets & connection strings in IaC | | | |
| 35 | IaC rules multi-region DR | | | |
| 36 | CI/CD guidance GitHub .NET API | | | |
| 37 | Azure DevOps pipeline container app | | | |
| 38 | Monorepo pipeline independent services | | | |
| 39 | Pipeline secrets/env vars/approvals | | | |
| 40 | CI/CD PR build test deploy on merge | | | |
| 41 | App logs dev env API last 30m | | | |
| 42 | Logs worker staging diagnose timeouts | | | |
| 43 | Error-only logs web frontend 1h | | | |
| 44 | Combined logs all services prod | | | |
| 45 | Exceptions database connectivity 24h | | | |
| 46 | Diagram web + API + database | | | |
| 47 | Diagram 3-service container app + DB | | | |
| 48 | Diagram ingress, app services/containers, storage | | | |
| 49 | Diagram queue worker + API + web | | | |
| 50 | Diagram microservices internal calls + VNet | | | |

Legend
- Pass: Expected azmcp command invoked; output structurally valid; no unexpected errors.
- Fail: Tool not invoked, wrong command, incorrect output structure, or unhandled error.
- Notes: Include remediation ideas if fail; link to logs or screenshots if applicable.

