---
title: Companion catalogue
description: Every published toolup-forge companion package — providers, sinks, hosts, infrastructure. Drop in what you need; skip what you don't.
layout: page
slug: /companions/
---

Companions sit behind SDK interfaces. The Platform core carries no vendor SDK; every cloud / vendor integration is an opt-in `<PackageReference>` away. This page lists what's published today.

Per the SDK's [companion-authoring guide](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CLAUDE.md), every companion: takes its substrate dependencies (typically `ISecretStore`) through its `create` function — never reads env vars or config files directly; ships an `IHealthCheck` probe; registers an `IConfigValidator` for preflight when connection state is testable.

Want to author a new one? See the per-area "Extending" docs: [AI](/docs/ai/extending) · [RAG](/docs/rag/extending) · [Forms](/docs/forms/extending) · [Scheduling](/docs/scheduling/extending) · [Knowledge Base](/docs/knowledge-base/extending). Or open an issue at [github.com/ToolUp-Forge/toolup-forge](https://github.com/ToolUp-Forge/toolup-forge/issues).

## AI providers — `IAIProvider`

LLM providers behind one streaming + tool-calling interface. Pick by latency, cost, prompt-caching support, or licensing. Capabilities — `SupportsPromptCaching`, `SupportsVisionInput`, etc. — are reported per provider via `AIProviderResponse.Capabilities`.

| Package | What it wires | Notes |
|---|---|---|
| [`ToolUp.AIProviders.Claude`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AIProviders/Claude) | Anthropic Claude (Opus / Sonnet / Haiku) | Prompt caching, vision, tool calling. Streaming via `message_delta` for usage reporting. |
| [`ToolUp.AIProviders.OpenAI`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AIProviders/OpenAI) | OpenAI models (GPT-4o / 4-mini / o-series) | Streaming via `stream_options.include_usage=true`. |
| [`ToolUp.AIProviders.Gemini`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AIProviders/Gemini) | Google Gemini (1.5 / 2.x families) | Streaming, tool calling, structured output. |

How to compose: `ServerApp.withAIProvider` (or via the `DefaultAIProviderFactory` for multi-provider deployments). Walkthrough at [AI getting started](/docs/ai/getting-started); contract at [AI extending](/docs/ai/extending).

## Embedding providers — `IEmbeddingProvider`

Vector embeddings for RAG ingestion + query.

| Package | What it wires | Notes |
|---|---|---|
| [`ToolUp.EmbeddingProviders.Local`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/EmbeddingProviders/Local) | In-process embeddings | Offline, no API key, no per-call cost. Dev-only — stateful across calls. |
| [`ToolUp.EmbeddingProviders.OpenAI`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/EmbeddingProviders/OpenAI) | OpenAI's `text-embedding-3-*` models | Stateless; distributed-ready. |

See [RAG extending](/docs/rag/extending) for the contract; [Embedding providers doc](/docs/companions/embedding-providers) for the per-provider reference.

## Rerankers

Optional post-retrieval relevance reorder for RAG.

| Package | What it wires |
|---|---|
| [`ToolUp.Rerankers.Local`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Rerankers/Local) | In-process reranking |

## Auth providers — `IAuthProvider`

Sign-in + token issue + session lifecycle. Pick one per deployment (or none for `Anonymous` mode).

| Package | What it wires | Notes |
|---|---|---|
| [`ToolUp.AuthProviders.Oidc`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AuthProviders/Oidc) | Generic OIDC (Auth0, Keycloak, Okta, Microsoft Entra Workforce ID, …) | Authorization Code + PKCE. Preset registry for common IdPs. |
| [`ToolUp.AuthProviders.OidcClient`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AuthProviders/OidcClient) | Browser-side OIDC client | Pairs with `Oidc` server. |
| [`ToolUp.AuthProviders.EntraExternalId`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AuthProviders/EntraExternalId) | Microsoft Entra External ID | Customer-facing variant of Entra. |
| [`ToolUp.AuthProviders.EntraExternalIdClient`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AuthProviders/EntraExternalIdClient) | Browser-side Entra External ID | Pairs with `EntraExternalId` server. |
| [`ToolUp.AuthProviders.ClerkUI`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AuthProviders/ClerkUI) | Clerk hosted sign-in UI | Paid by Clerk; opt-in only. |

See [Auth providers doc](/docs/companions/auth-providers) for the per-IdP setup; [Auth](/docs/platform/auth) for the SDK-side contract.

## Storage — `IBlobStorage`

Binary blob persistence behind one interface. Scope-aware: every read / write goes through a `StorageScope` so a tenant can't accidentally read another's data.

| Package | Backing service |
|---|---|
| [`ToolUp.Storage.AwsS3Storage`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Storage/AwsS3Storage) | AWS S3 |
| [`ToolUp.Storage.AzureBlobStorage`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Storage/AzureBlobStorage) | Azure Blob Storage |
| [`ToolUp.Storage.GoogleCloudStorage`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Storage/GoogleCloudStorage) | Google Cloud Storage |

The default in-process implementation file-backs to disk under `data/` — fine for dev, single-process production. For multi-instance, swap in a cloud companion. See [Storage providers doc](/docs/companions/storage-providers).

## Vector stores — `IVectorStore`

For RAG vector indexes.

| Package | What it wires |
|---|---|
| [`ToolUp.VectorStores.Hnsw`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/VectorStores/Hnsw) | HNSW (Hierarchical Navigable Small World) index |

Default in-process implementation is suitable up to single-machine workloads; the HNSW companion scales to millions of vectors. See [RAG concepts](/docs/rag/concepts).

## Audit sinks — `IAuditSink`

Replicates audit events to external archival / SIEM systems. Batch-idempotent — dispatcher retries the whole batch on `Result.Error`; sinks must use vendor-specific dedup keys.

| Package | What it wires |
|---|---|
| [`ToolUp.AuditSinks.S3Archive`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AuditSinks/S3Archive) | S3 (content-addressable archival) |
| [`ToolUp.AuditSinks.SplunkHec`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AuditSinks/SplunkHec) | Splunk HTTP Event Collector |
| [`ToolUp.AuditSinks.DatadogLogs`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AuditSinks/DatadogLogs) | Datadog Logs |

## Notification channels — `INotificationChannel` / `INotificationSink`

Two distinct interfaces: `INotificationChannel` is the distributed pub/sub backbone (scope-isolated topics); `INotificationSink` is per-`Kind` transactional delivery (email, SMS, push).

### Real-time channel (replaces the default in-memory channel)

| Package | What it wires |
|---|---|
| [`ToolUp.NotificationChannels.Redis`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/NotificationChannels/Redis) | Redis pub/sub for cross-instance fan-out |

### Email sinks

| Package | What it wires |
|---|---|
| [`ToolUp.NotificationChannels.Email.Smtp`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/NotificationChannels/Email/Smtp) | Generic SMTP (MailKit; no vendor lock-in) |
| [`ToolUp.NotificationChannels.Email.SendGrid`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/NotificationChannels/Email/SendGrid) | SendGrid API |
| [`ToolUp.NotificationChannels.Email.Postmark`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/NotificationChannels/Email/Postmark) | Postmark API |

### SMS sinks

| Package | What it wires |
|---|---|
| [`ToolUp.NotificationChannels.Sms.Twilio`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/NotificationChannels/Sms/Twilio) | Twilio SMS |

### Push sinks

| Package | What it wires |
|---|---|
| [`ToolUp.NotificationChannels.Push.WebPush`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/NotificationChannels/Push/WebPush) | Browser Web Push (VAPID) |

See [Notification channels doc](/docs/companions/notification-channels) for the substrate contract.

## Secrets — `ISecretStore`

Every API key / provider token / connection string flows through `ISecretStore`. The store is consulted per-call (token rotation propagates without a redeploy).

| Package | Backing service |
|---|---|
| [`ToolUp.Secrets.AzureKeyVault`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Secrets/AzureKeyVault) | Azure Key Vault |
| [`ToolUp.Secrets.AwsSecretsManager`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Secrets/AwsSecretsManager) | AWS Secrets Manager |
| [`ToolUp.Secrets.HashiCorpVault`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Secrets/HashiCorpVault) | HashiCorp Vault |

The default `FileSecretStore` encrypts JSON to disk under `data/secrets/` — fine for dev. Production deployments swap one of the cloud companions in.

## Metrics — `IMetricsSink`

| Package | What it wires |
|---|---|
| [`ToolUp.Metrics.OpenTelemetry`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Metrics/OpenTelemetry) | OTLP export (any OTel-compatible backend) |

## Data sources

External-system query connectors for module data ingestion.

| Package | What it wires |
|---|---|
| [`ToolUp.DataSources.BigQuery`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/DataSources/BigQuery) | Google BigQuery |

## Serverless hosts

Adapters that let an `IServerHost`-shaped app run inside a serverless platform's invocation model. Pairs with the `ToolUp.Platform.Core`-only deployment shape — the host adapter brings the request/response loop.

| Package | What it wires |
|---|---|
| [`ToolUp.Hosts.AwsLambda`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Hosts/AwsLambda) | AWS Lambda + API Gateway / ALB |
| [`ToolUp.Hosts.AzureFunctions`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Hosts/AzureFunctions) | Azure Functions |
| [`ToolUp.Hosts.GoogleCloudFunctions`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/Hosts/GoogleCloudFunctions) | Google Cloud Functions |

Background: [Composition roots](/docs/platform/composition-roots).

## Infrastructure helpers

Not "providers" in the interface-implementation sense — companions providing infrastructure that's separable from the core.

| Package | What it wires |
|---|---|
| [`ToolUp.PublicRendering`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/ToolUp.PublicRendering) | SSR public-page substrate — Giraffe.ViewEngine + markdown loader + sitemap + structured-data helpers. This site uses it. |
| [`ToolUp.AssetStore`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/ToolUp.AssetStore) | Hashed-filename asset publishing for the Client tier — content-addressable URLs, far-future cache headers, derivative generation. |
| [`ToolUp.AgGridEnterprise`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/AgGridEnterprise) | AG Grid Enterprise initialisation shim — paid licence; opt-in only. |
| [`ToolUp.Reporting.{Core,Server}`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/ToolUp.Reporting) | Reporting primitives. |

## Distribution

Every companion ships as a NuGet package on the [ToolUp-Forge GitHub Packages feed](https://github.com/orgs/ToolUp-Forge/packages?repo_name=toolup-forge). Add the feed per [Getting started](/getting-started) and reference packages by `<PackageReference>` like any other.

Version coordinates with the platform via the `ToolUp.Sdk` meta-manifest. Bumping `<ToolUpSdkVersion>` in `Directory.Packages.props` moves every companion in lockstep.
