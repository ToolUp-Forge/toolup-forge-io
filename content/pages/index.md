---
title: toolup-forge
description: An open-source F# full-stack SDK for production multi-tenant analytical applications.
layout: page
---

`toolup-forge` is an open-source F# full-stack SDK for building production multi-tenant analytical applications. Giraffe over ASP.NET Core for the server, Fable + Elmish + Feliz for the client, [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting) for type-safe wire — F# end to end with no DTO duplication.

> **Status: pre-release (`0.x.y`).** SemVer-on-`0.x` — minor bumps may include breaking changes. `1.0.0` ships once the surface is stable.

## What it gives you

- **Multi-tenant by construction.** Scope isolation, RBAC, per-tenant data scoping, and audit trail are first-class — see [Surfaces](/docs/platform/surfaces) and [Composition roots](/docs/platform/composition-roots).
- **AI-augmented as a peer.** Agent loop, SSE streaming, tool registry, prompt caching, conversation persistence — drop in a provider companion and the LLM is wired. See [AI concepts](/docs/ai/concepts).
- **Schema-driven modules.** `FormSchema` + `WorkflowDefinition` + a 4-file module convention collapses CRUD-heavy intake and approval flows. See [Forms concepts](/docs/forms/concepts).
- **Companion packages isolate vendors.** No paid-by-default dependencies. Every cloud / vendor integration sits behind an interface, swappable per deployment. See the [companion catalogue](/companions).
- **Sector-agnostic.** The SDK ships infrastructure; you bring domain.

## Get started in 60 seconds

Add the coordinated meta-manifest to your `Directory.Packages.props`:

```xml
<PropertyGroup>
  <ToolUpSdkVersion>0.4.0</ToolUpSdkVersion>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="ToolUp.Sdk" />
</ItemGroup>
```

Reference what you need in each consuming `.fsproj`:

```xml
<PackageReference Include="ToolUp.Platform.Server" />
<PackageReference Include="ToolUp.Platform.Client" />
```

Minimum server composition root:

```fsharp
open ToolUp.Platform

[<EntryPoint>]
let main _ =
    ServerApp.empty
    |> ServerApp.withConfig { ServerConfig.defaults with Port = 5000 }
    |> ServerApp.run
```

That serves on `http://localhost:5000`. Add modules with `ServerApp.addModules`, swap in providers with `ServerApp.withAuth` / `withStorage`, layer on `AIServerApp` / `RAGServerApp` for AI/RAG. The full walkthrough is in [Getting started](/getting-started), and a runnable end-to-end example is at [`samples/HelloWorld/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/samples/HelloWorld).

## Pick your shape

| You want | Compose with | Read |
|---|---|---|
| A pure API service | `ToolUp.Platform.Server` only | [Composition roots](/docs/platform/composition-roots) |
| A full-stack app (SAFE-style) | `Platform.Server` + `Platform.Client` | [Architecture](/docs/platform/architecture) |
| Add an AI assistant | `+ ToolUp.AI.{Server,Client}` + a provider | [AI getting started](/docs/ai/getting-started) |
| Retrieval-augmented generation | `+ ToolUp.RAG` + `ToolUp.KnowledgeBase` | [RAG getting started](/docs/rag/getting-started) |
| Schema-driven forms | `+ ToolUp.Forms.{Server,Client}` | [Forms getting started](/docs/forms/getting-started) |
| Booking with concurrency lock | `+ ToolUp.Scheduling.{Server}` | [Scheduling getting started](/docs/scheduling/getting-started) |
| Public-facing SSR pages | `+ ToolUp.PublicRendering` | This site is built with it — [source](https://github.com/toolup-forge/toolup-forge-io) |
| A serverless host | `+ ToolUp.Platform.Core` + the AWS Lambda adapter | [Composition roots](/docs/platform/composition-roots) |

## Why F#

The SDK is opinionated about F# all the way down. Shared types cross the client/server boundary via Fable.Remoting without DTO duplication. Modules are pure-MVU on the client (no in-memory state between calls), pure-handler on the server. The whole stack — server, client (via Fable), build (via FAKE) — is one language. The reasons are written up at [Why toolup-forge](/why-forge).

## Built with toolup-forge

This site is server-side-rendered F# using the [`ToolUp.PublicRendering`](/docs/companions) companion (every page you're reading right now comes through Giraffe.ViewEngine). The source is on GitHub at [`toolup-forge/toolup-forge-io`](https://github.com/toolup-forge/toolup-forge-io) — same Apache 2.0 licence as the SDK itself.

If you ship something with `toolup-forge`, we'd like to hear about it — open a discussion at [github.com/ToolUp-Forge/toolup-forge/discussions](https://github.com/ToolUp-Forge/toolup-forge/discussions).

## Source + licence

- **Source:** [`github.com/ToolUp-Forge/toolup-forge`](https://github.com/ToolUp-Forge/toolup-forge)
- **NuGet:** [GitHub Packages — ToolUp-Forge](https://github.com/orgs/ToolUp-Forge/packages?repo_name=toolup-forge)
- **Licence:** [Apache 2.0](https://github.com/ToolUp-Forge/toolup-forge/blob/main/LICENSE)
- **Contributing:** see [Contributing](/contributing) and the [DCO sign-off requirement](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CONTRIBUTING.md)
