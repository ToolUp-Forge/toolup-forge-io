---
title: Documentation
description: Every architectural area of toolup-forge — platform, AI, RAG, knowledge base, forms, scheduling, companions, design canon, migrations.
layout: page
slug: /docs/
---

The full documentation tree, synced from [`toolup-forge/docs/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/docs) on every site deploy. The forge repository is the source of truth — if a doc here looks wrong, the fix lands in [`toolup-forge`](https://github.com/ToolUp-Forge/toolup-forge) and propagates on the next deploy.

## Platform

The core SDK — composition, scope resolution, modules, infrastructure interfaces.

- [Architecture](/docs/platform/architecture) — three-tier package shape, composition roots, the infrastructure / domain split.
- [Composition roots](/docs/platform/composition-roots) — `ServerApp` / `AIServerApp` / `RAGServerApp` pipelines.
- [Modules](/docs/platform/modules) — the 4-file module convention, `ClientModule`, `ServerModule`, data types.
- [Surfaces](/docs/platform/surfaces) — auth posture, anonymous vs individual vs team vs multi-team.
- [Auth](/docs/platform/auth) — `IAuthProvider`, OIDC, registry pattern, multi-team session.
- [Storage](/docs/platform/storage) — `IBlobStorage`, `IEntityStore`, scope-aware persistence.
- [Jobs](/docs/platform/jobs) — `IJobScheduler`, `IJobStore`, distributed-ready handler contract.
- [Events](/docs/platform/events) — `IEventStore`, audit replication, retention policy.
- [Portability rules](/docs/platform/portability-rules) — the six rules every distributed-implementation interface obeys.
- [SSE deployment](/docs/platform/sse-deployment) — server-sent events through proxies / load balancers.
- [Prerender](/docs/platform/prerender) — static-site generation from the Fable client.
- [Premium](/docs/platform/premium) — tier gating, claim-based feature flags.
- [Surfaces — testing conventions](/docs/platform/testing-conventions) — Expecto over `dotnet test`, contract test packs.
- [Client logging](/docs/platform/client-logging) — `ILogger` on the Fable side.
- [Client remoting proxies](/docs/platform/client-remoting-proxies) — the per-module proxy pattern.
- [SVG helpers](/docs/platform/svg-helpers) — Feliz SVG primitives.
- [Ads + AdSense](/docs/platform/ads) — `IAdSurface`, AdSense approval requirements.
- [Data subject requests](/docs/platform/data-subject-requests) — GDPR-shaped erasure / export.

## AI

`ToolUp.AI` — agent loop, SSE streaming, tool registry, system-prompt composition. Built on the `IAIProvider` extension point.

- [Getting started](/docs/ai/getting-started) · [Concepts](/docs/ai/concepts) · [API reference](/docs/ai/api-reference) · [Extending](/docs/ai/extending)

## RAG

`ToolUp.RAG` — chunking, vector store, retrieval pipeline, ingestion + reembedding background services, prompt builder. Built on `IEmbeddingProvider` + `IVectorStore` + `IRetrievalPipeline`.

- [Getting started](/docs/rag/getting-started) · [Concepts](/docs/rag/concepts) · [API reference](/docs/rag/api-reference) · [Extending](/docs/rag/extending)

## Knowledge Base

`ToolUp.KnowledgeBase` — the canonical user-facing consumer of `ToolUp.RAG`. Document upload, multi-format extraction (PDF / PPTX / DOCX / XLSX / CSV), ingestion-status surfacing, narrative-commit, notes, AI-context.

- [Getting started](/docs/knowledge-base/getting-started) · [Concepts](/docs/knowledge-base/concepts) · [API reference](/docs/knowledge-base/api-reference) · [Extending](/docs/knowledge-base/extending)

## Forms

`ToolUp.Forms` — schema-driven forms, workflows, publishable surveys with HMAC-signed share tokens.

- [Getting started](/docs/forms/getting-started) · [Concepts](/docs/forms/concepts) · [API reference](/docs/forms/api-reference) · [Extending](/docs/forms/extending)

## Scheduling

`ToolUp.Scheduling` — booking with per-resource concurrency lock, recurrence expander, iCalendar serialisation.

- [Getting started](/docs/scheduling/getting-started) · [Concepts](/docs/scheduling/concepts) · [API reference](/docs/scheduling/api-reference) · [Extending](/docs/scheduling/extending)

## Companions

Provider companions sit behind SDK interfaces. Drop in the ones a deployment needs; skip the ones it doesn't.

- [Auth providers](/docs/companions/auth-providers) — OIDC (Microsoft Entra, Auth0, Keycloak, generic), Clerk UI.
- [Storage providers](/docs/companions/storage-providers) — AWS S3, Azure Blob, Google Cloud Storage.
- [AI providers](/docs/companions/ai-providers) — Anthropic Claude, OpenAI.
- [Embedding providers](/docs/companions/embedding-providers) — Local (in-process), OpenAI.
- [Notification channels](/docs/companions/notification-channels) — Redis, SMTP, SendGrid, Twilio, WebPush.

Or browse the full [companion catalogue](/companions) with NuGet + source links per entry.

## Design canon

- [Mixed-mode platform](/docs/design/mixed-mode-platform) — how Anonymous / Authenticated-Ephemeral / Individual / Team / Multi-Team modes share infrastructure without dragging weight into the surfaces that don't use them.

## Migrations

When a release ships a consumer-facing breaking change, a migration doc lands under `docs/migrations/` with before/after, verification steps, and rollback. The latest:

- [Namespace rename: `Fable.Remoting.*` → `ToolUp.Remoting.*` and `Elmish` → `ToolUp.Elmish`](/docs/migrations/73-namespace-rename-to-toolup-remoting-and-toolup-elmish) — mechanical search-and-replace across consumer code; behaviour unchanged.
- [`0.4.0` — toolup-elmish adoption](/docs/migrations/0.4.0-toolup-elmish-adoption) — moving from `Fable.Elmish.*` to the forge-native primitives.
- [`0.4.0` — OIDC app config](/docs/migrations/0.4.0-oidc-app-config) — auth-provider config shape change.
- [`0.4.0` — Entra External ID deprecation](/docs/migrations/0.4.0-entra-external-id-deprecation) — switching to Entra Workforce OIDC.
- [`0.4.x` — multi-platform AI provider substrate](/docs/migrations/0.4.x-phase-70-ai-provider-substrate) — provider companion contract refresh and `PlatformAIKeysApi` surface.
- [`0.3.x` — OIDC presets](/docs/migrations/0.3.x-oidc-presets) — moving from bespoke OIDC wiring to the preset registry.

The complete list is at [`docs/migrations/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/docs/migrations).

## Where to ask

- **Question?** [Discussions](https://github.com/ToolUp-Forge/toolup-forge/discussions) on GitHub.
- **Found a bug?** [Issues](https://github.com/ToolUp-Forge/toolup-forge/issues) on GitHub.
- **Security disclosure?** Per [SECURITY.md](https://github.com/ToolUp-Forge/toolup-forge/blob/main/SECURITY.md) — private channels only.
