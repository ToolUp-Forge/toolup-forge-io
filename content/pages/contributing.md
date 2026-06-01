---
title: Contributing
description: How to contribute to toolup-forge — DCO sign-off, maintenance tiers, where to ask, style and conventions.
layout: page
slug: /contributing/
---

`toolup-forge` welcomes contributions — bug reports, fixes, features, documentation, new companion packages, and new modules. This page orients you to the process; the canonical reference is [`CONTRIBUTING.md`](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CONTRIBUTING.md) in the repo.

## Where to start

| You want to… | Go to |
|---|---|
| Report a bug | [Issues](https://github.com/ToolUp-Forge/toolup-forge/issues) on the forge repo |
| Ask a question | [Discussions](https://github.com/ToolUp-Forge/toolup-forge/discussions) on the forge repo |
| Propose a new feature | [Discussions → Ideas](https://github.com/ToolUp-Forge/toolup-forge/discussions/categories/ideas) before opening a PR |
| Fix something small | Open a PR — see [Quick start](#quick-start) below |
| Author a new companion | Read [companion-authoring guidance](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CLAUDE.md#companion-authoring-guide) + open a discussion first |
| Report a security issue | **Don't open a public issue.** See [SECURITY.md](https://github.com/ToolUp-Forge/toolup-forge/blob/main/SECURITY.md) for the private disclosure channel |

## Quick start

1. **Fork** the repository at [`github.com/ToolUp-Forge/toolup-forge`](https://github.com/ToolUp-Forge/toolup-forge).
2. **Clone** your fork, create a branch off `main`.
3. **Make your change.** Follow the [style + conventions](#style-and-conventions) below.
4. **Sign each commit** with `Signed-off-by:` — `git commit -s` is the easiest way. See [DCO](#developer-certificate-of-origin-dco).
5. **Build locally:**
    ```powershell
    dotnet build ToolUp.Forge.sln
    dotnet fantomas --check .
    # Run each Expecto suite via `dotnet run --project ...` — they ship as
    # console runners, not test packs (so `dotnet test` silently no-ops).
    dotnet run --project src/ToolUp.Platform.Tests/ToolUp.Platform.Tests.fsproj
    ```
6. **Open a PR** against `main` with a description and rationale. Reference related issues.
7. A maintainer reviews per the [contribution-flow timelines](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CONTRIBUTING.md#contribution-flow-by-type).

## Developer Certificate of Origin (DCO)

`toolup-forge` uses [DCO](https://developercertificate.org/) — not a contributor licence agreement. DCO sign-off is the same mechanism the Linux kernel and Docker use. By signing your commit, you certify you wrote the patch (or otherwise have the right to submit it).

Add a `Signed-off-by:` trailer to every commit:

```bash
git commit -s -m "Your change description"
```

Name + email must match your `git config user.name` / `user.email`. Pseudonymous sign-offs aren't accepted. The full DCO terms are at [`developercertificate.org`](https://developercertificate.org/).

To avoid forgetting `-s`, set sign-off globally:

```powershell
git config --global format.signoff true
```

## Three-tier maintenance model

The forge repo distinguishes **first-party** (maintained by the core team, ship with the SDK release), **community** (accepted PR, listed in the catalogue, maintained by the contributor), and **third-party** (lives outside the forge org, no relationship beyond being mentioned).

When you propose a new companion or module, the [Promotion from community to first-party](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CONTRIBUTING.md#promotion-from-community-to-first-party) section describes the bar. Briefly:

- The companion satisfies the [six portability rules](/docs/platform/portability-rules/) where applicable.
- A README + tests ship alongside.
- The contributor commits to a year-1 maintenance window (security + critical bug fixes; major-version refactors can be re-negotiated).

This is to keep the published surface durable. A companion package that lands as first-party becomes part of the SDK's compatibility promise.

## Style and conventions

Briefly — the canonical reference is [`CLAUDE.md`](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CLAUDE.md) in the forge repo:

- **F# 10 on .NET 10.** SDK pin is `10.0.203` (`rollForward: latestFeature`).
- **Fantomas** is the formatter. Run `dotnet fantomas <file>` BEFORE `dotnet build`. PRs failing the format check are rejected — fix locally, push the format-only commit, re-request review.
- **Immutable by default.** Records, no mutable fields without justification. The [Guiding Principles](/docs/platform/architecture/) — referenced as `(GP NN)` in source comments — are the canon.
- **Elmish MVU discipline.** `update` functions are pure. All side effects flow through `Cmd`. No mutable global state in client code.
- **Modules don't reference each other.** Shared types live in shared-types files. Inter-module communication uses persisted data or events, never imports.
- **Six portability rules** for distributed-implementation interfaces. See [Portability rules](/docs/platform/portability-rules/).
- **The two type-erasure boundaries** (`ClientModule.register` + `DataTypeDisplay.RenderSummary`) are sanctioned; other `box` / `unbox` usage isn't.

## Documentation contributions

This site is a separate repo at [`github.com/toolup-forge/toolup-forge-io`](https://github.com/toolup-forge/toolup-forge-io). It synchronises [`toolup-forge/docs/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/docs) on every deploy — so:

- **Fixing a typo or broken example in `/docs/*`** → land it in [`toolup-forge`](https://github.com/ToolUp-Forge/toolup-forge) under `docs/`. The next deploy of this site picks it up.
- **Fixing a marketing-page typo, a layout bug, or a missing internal link** → land it in [`toolup-forge-io`](https://github.com/toolup-forge/toolup-forge-io). Same DCO / Apache-2.0 posture.

When in doubt, open the issue on `toolup-forge` — a maintainer will route it.

## Code of Conduct

The project follows the [Contributor Covenant v2.1](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CODE_OF_CONDUCT.md). Be respectful and constructive. Reports go to the contact in `CODE_OF_CONDUCT.md`.

## Where to ask

| Question shape | Place |
|---|---|
| "How do I…?" | [Discussions → Q&A](https://github.com/ToolUp-Forge/toolup-forge/discussions/categories/q-a) |
| "Should the SDK do…?" | [Discussions → Ideas](https://github.com/ToolUp-Forge/toolup-forge/discussions/categories/ideas) |
| "I think this is broken" | [Issues](https://github.com/ToolUp-Forge/toolup-forge/issues) |
| "I want to author a companion / module" | [Discussions → Show and tell](https://github.com/ToolUp-Forge/toolup-forge/discussions/categories/show-and-tell) for ideas, then a PR |
| Security vulnerability | [SECURITY.md](https://github.com/ToolUp-Forge/toolup-forge/blob/main/SECURITY.md) — private channel only |

## Thank you

OSS sustainability is a problem the F# ecosystem has not entirely solved. Every issue triaged, every doc improved, every companion authored makes the SDK more useful for the next person. The acknowledgements file is the modest version of "thank you" — your name lands there when your first PR merges.
