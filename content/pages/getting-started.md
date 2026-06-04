---
title: Getting started
description: From zero to a running toolup-forge app — package install, first module, full-stack handshake. ~15 minutes.
layout: page
slug: /getting-started/
---

This walks from an empty solution to a running toolup-forge app: server, Fable client, one Elmish module, type-safe wire. The intended state at the end is "I understand what each file in this project does, and I know which doc to read for any one of them."

## Prerequisites

- **.NET 10 SDK.** The whole stack targets `net10.0` / F# 10. Older SDKs work for some sub-packages but the canonical version is in [forge's `global.json`](https://github.com/ToolUp-Forge/toolup-forge/blob/main/global.json).
- **Node 22+** (for the Fable client's Vite dev server and any npm-based plugins like AG Grid).
- **A GitHub PAT with `read:packages` scope.** The `ToolUp.*` packages are public on the [ToolUp-Forge GitHub Packages feed](https://github.com/orgs/ToolUp-Forge/packages), but GitHub Packages NuGet requires authentication for restore (a long-standing quirk specific to NuGet on GH Packages — npm and Maven permit anonymous public restore; NuGet does not). Set `GITHUB_PACKAGES_USER` + `GITHUB_PACKAGES_TOKEN` in your shell profile.

## 1. Get the easiest possible starting point

Two routes — pick one:

**A. Clone the canonical sample.** [`samples/HelloWorld/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/samples/HelloWorld) in the forge repo is a runnable end-to-end app (server + Fable client + one module + ToolUp.Remoting wire). Clone forge, build, run.

```powershell
git clone https://github.com/ToolUp-Forge/toolup-forge.git
cd toolup-forge\samples\HelloWorld
dotnet run
```

**B. Start from empty.** Steps 2-6 below scaffold the same shape file by file.

## 2. Solution skeleton

```
my-app/
├── global.json                   # pin .NET 10
├── nuget.config                  # GitHub Packages feed + credentials
├── Directory.Packages.props      # ToolUpSdkVersion coordinated version
├── my-app.sln
├── src/
│   ├── Shared/                   # cross-tier types
│   ├── Server/                   # Giraffe handlers + composition root
│   └── Client/                   # Fable + Elmish + Feliz
└── modules/
    └── Hello/                    # single domain module (4 files)
```

The shape mirrors `samples/HelloWorld/` — copy that as the reference.

## 3. Coordinated SDK version

`Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <ToolUpSdkVersion>0.5.7</ToolUpSdkVersion>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="ToolUp.Sdk" Version="$(ToolUpSdkVersion)" />
    <PackageVersion Include="ToolUp.Platform.Core" Version="$(ToolUpSdkVersion)" />
    <PackageVersion Include="ToolUp.Platform.Server" Version="$(ToolUpSdkVersion)" />
    <PackageVersion Include="ToolUp.Platform.Client" Version="$(ToolUpSdkVersion)" />
  </ItemGroup>
</Project>
```

`ToolUp.Sdk` is a meta-manifest — adding it propagates `<PackageVersion>` entries for every `ToolUp.*` package at the same version. Bumping the whole SDK is a one-line edit. The meta-package is described under [Architecture](/docs/platform/architecture).

## 4. Minimum server composition root

In `src/Server/Server.fs`:

```fsharp
module MyApp.Server

open ToolUp.Platform
open ToolUp.Platform.Server

[<EntryPoint>]
let main _ =
    let config = {
        ServerConfig.defaults with
            Port = 5000
            Mode = PlatformMode.Individual
    }

    ServerApp.empty
    |> ServerApp.withConfig config
    |> ServerApp.addModules [ Hello.Server.register () ]
    |> ServerApp.run
```

What this does:

- Wires every infrastructure interface to its default in-process implementation (file-backed `IEntityStore`, in-process `IJobScheduler`, in-process `INotificationChannel`, etc.).
- Adds the `Hello` module's `ServerModule` to the registry.
- Runs Giraffe over ASP.NET Core on port 5000.

The full composition surface is in [Composition roots](/docs/platform/composition-roots). Why each field matters in `ServerConfig` is in [Surfaces](/docs/platform/surfaces).

## 5. Minimum module (the 4-file pattern)

`modules/Hello/` ships four files. Full convention is in [Modules](/docs/platform/modules); minimum shape:

```fsharp
// SharedTypes.fs — record types crossing the wire
module Hello.SharedTypes
type HelloApi = { DoThing: string -> Async<string> }

// Server.fs — handler + registration
module Hello.Server
open ToolUp.Platform
let private api : SharedTypes.HelloApi = {
    DoThing = fun input -> async { return sprintf "did: %s" input }
}
let register () : ServerModule =
    ServerModule.create "Hello"
    |> ServerModule.withGuardedApi api

// ClientModel.fs — Elmish Model/Msg/init/update
module Hello.ClientModel
open ToolUp.Elmish
type Model = { Text: string }
type Msg = NoOp
let init () : Model * Cmd<Msg> = { Text = "" }, Cmd.none
let update _ m = m, Cmd.none

// ClientView.fs — Feliz view + register
module Hello.ClientView
open Feliz
open ToolUp.Platform
open Hello.ClientModel

let view (m: Model) _ =
    Html.div [] , Html.div [ Html.text m.Text ]

let register () : ErasedModule =
    ClientModule.create {
        Init = init
        Update = update
        Name = "Hello"
        Icon = "/svg/chart.svg"
    }
    |> ClientModule.withView view
    |> ClientModule.register
```

The two `register ()` calls — one on the server side, one on the client side — are what wire the module into the shell. Every module obeys the same shape; the shell handles routing, scope resolution, persistence, auth, AI tool registration.

## 6. Minimum client composition root

In `src/Client/Client.fs`:

```fsharp
module MyApp.Client

open ToolUp.Platform

[<EntryPoint>]
let main _ =
    Client.empty
    |> Client.addModules [ Hello.ClientView.register () ]
    |> Client.run
    0
```

Plus a tiny `index.html` (Vite entry point) and Fable build wiring. The [`samples/HelloWorld/HelloWorld.Client/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/samples/HelloWorld) directory shows the full set.

## 7. Run

```powershell
# Terminal 1 - server (auto-restarts on .fs changes)
cd src/Server
dotnet watch run

# Terminal 2 - Fable client (recompiles only changed files on save)
cd src/Client
dotnet fable -o output --watch

# Terminal 3 - Vite dev server (HMR)
cd src/Client
npm run dev
```

Open `http://localhost:5000/`. The sidebar lists the `Hello` module; clicking it renders the Feliz view; the `DoThing` API call returns "did: ..." over ToolUp.Remoting.

The three-terminal loop is the fast-iteration setup; a single `dotnet run -- Run` from the FAKE driver also works for one-shot. The difference matters once you're editing — see [forge's CLAUDE.md](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CLAUDE.md) "Fast iteration" for the rationale.

## What just happened

| Layer | What it does | Where to read |
|---|---|---|
| `ServerApp.run` | Stands up Giraffe + ASP.NET Core, wires default interfaces, mounts `/api/<module>/...` routes for every registered `ServerModule`, mounts auth + platform-admin + health-check surfaces. | [Composition roots](/docs/platform/composition-roots) |
| `Hello.Server.register ()` | Returns a `ServerModule` carrying the `HelloApi` record. `ServerApp.addModules` mounts its handlers under `/api/Hello/`. | [Modules](/docs/platform/modules) |
| `Client.run` | Stands up the Elmish runtime, mounts the SDK shell (sidebar, top bar, modals, toast centre, side panel), wires `AuthUIProvider`, attaches every registered `ClientModule`. | [Modules](/docs/platform/modules) |
| `ToolUp.Remoting` (the wire) | Auto-generates a typed proxy from the `HelloApi` record. The client calls `api.DoThing "foo"` and the server handler runs — no DTOs, no hand-rolled JSON. Forked from Fable.Remoting (Zaid Ajaj, MIT) and ships in-tree under `ToolUp.Platform.{Core,Client,Server}`. | [Architecture](/docs/platform/architecture) |
| `Mode = Individual` | Pins per-user scope isolation. Data the module persists belongs to the signed-in user. | [Surfaces](/docs/platform/surfaces) |

## Next — pick what to add

- **An AI assistant** — drop in `ToolUp.AI.{Server,Client}` + an `IAIProvider` companion. Walkthrough at [AI getting started](/docs/ai/getting-started).
- **Retrieval-augmented generation** — `+ ToolUp.RAG` + `ToolUp.KnowledgeBase`. [RAG getting started](/docs/rag/getting-started).
- **A schema-driven form** — `+ ToolUp.Forms.{Server,Client}`. [Forms getting started](/docs/forms/getting-started).
- **An auth provider** — pick from [Auth providers](/docs/companions/auth-providers) (Microsoft Entra, generic OIDC, Auth0, etc.) and call `ServerApp.withAuth`.
- **Cloud storage** — pick a [storage provider](/docs/companions/storage-providers) (AWS S3, Azure Blob, Google Cloud) and call `ServerApp.withStorage`.

## When it doesn't work

- **Restore fails with 401.** Set `GITHUB_PACKAGES_USER` + `GITHUB_PACKAGES_TOKEN` per the prerequisites above. If you already use `gh` CLI, `gh auth refresh -s read:packages && gh auth token` gives you a token that works.
- **Fable can't find `ClientModule.register`.** Make sure your Client project includes `<PackageReference Include="ToolUp.Platform.Client" />` and the Fable source-extraction directive in the SDK's `.Client.props` is being honoured. The [Architecture doc](/docs/platform/architecture) covers the source-in-nupkg pattern.
- **Client compiles but the sidebar is empty.** The module's `register ()` is not being called from `Client.fs`, or the module's view is returning `(Html.div [], Html.div [])` (the right-pane / left-pane tuple). See [Modules](/docs/platform/modules).

If you're stuck, [GitHub Discussions](https://github.com/ToolUp-Forge/toolup-forge/discussions) is the place to ask.
