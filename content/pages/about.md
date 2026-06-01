---
title: About
description: Governance, licensing, and the relationship between toolup-forge the open-source SDK and the commercial trademark holder.
layout: page
slug: /about/
---

`toolup-forge` is an open-source SDK released under [Apache License 2.0](https://github.com/ToolUp-Forge/toolup-forge/blob/main/LICENSE). The source lives at [`github.com/ToolUp-Forge/toolup-forge`](https://github.com/ToolUp-Forge/toolup-forge); this site lives at [`github.com/toolup-forge/toolup-forge-io`](https://github.com/toolup-forge/toolup-forge-io). Both are public, both are Apache-2.0.

## Project posture

- **Licence:** [Apache 2.0](https://github.com/ToolUp-Forge/toolup-forge/blob/main/LICENSE).
- **Contributor agreement:** [Developer Certificate of Origin](https://developercertificate.org/) (DCO). Every commit carries a `Signed-off-by:` line. CI enforces this.
- **Code of Conduct:** [Contributor Covenant v2.1](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CODE_OF_CONDUCT.md).
- **Security disclosure:** private channels only — see [`SECURITY.md`](https://github.com/ToolUp-Forge/toolup-forge/blob/main/SECURITY.md) for the contact path.
- **Versioning:** pre-release `0.x.y` with SemVer-on-`0.x` policy — minor bumps may include breaking changes; patch bumps stay non-breaking. `1.0.0` lands once the public surface is judged stable. [Migration docs](/docs/) ship alongside every breaking change.

## Who maintains it

The project is maintained by [Andrew J. Willshire](https://github.com/ajwillshire) with a co-owner reviewer. Issues and pull requests are welcome from anyone; the [Contributing guide](/contributing/) describes the flow.

If you want to maintain a companion package (a new auth provider, storage backend, audit sink, etc.) as part of the core distribution, open an issue or discussion first to align on shape — companion authoring is a documented pattern with portability constraints, and a draft PR conversation is cheaper than rework. See [companion-authoring](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CLAUDE.md#companion-authoring-guide) in the contributor doc.

## Trademark notice

"ToolUp Analytics", "ToolUp", and the related word and design marks are trademarks of **ToolUp Analytics Ltd** (a UK private limited company). The trademark is operated independently from this open-source project: nothing about contributing to `toolup-forge`, distributing it, or building applications with it grants any trademark right.

You can:

- Use the SDK to build commercial or non-commercial applications, under the Apache 2.0 licence's terms.
- Describe your application as "built with `toolup-forge`" or "uses the `toolup-forge` SDK" — these are nominative-fair-use references to identify the software you depend on.

You should not:

- Use the name "ToolUp" or "ToolUp Analytics" as the brand for your own product, fork, or service.
- Use any ToolUp logo or wordmark as if you were authorised to use it.

If you're unsure whether a use is permitted, please ask via a [GitHub discussion](https://github.com/ToolUp-Forge/toolup-forge/discussions). Trademarks exist to keep readers from being confused about the source of a product; that's the standard this project applies.

## Why open source

The SDK shape — modular companion packages, schema-driven modules, multi-tenant by construction, six portability rules — is the value. Open-sourcing it widens the contributor pool, gets the architecture tested against a broader set of applications than any one team can build, and keeps the design honest. Apache 2.0 with the trademark separation is the cleanest licence shape for that posture.

## This site

[`toolup-forge.io`](https://toolup-forge.io/) is the project's public website. It is **server-side-rendered F#** using the [`ToolUp.PublicRendering`](/companions/) companion — every page you're reading is rendered through Giraffe.ViewEngine by an SDK consumer. The source is open at [`github.com/toolup-forge/toolup-forge-io`](https://github.com/toolup-forge/toolup-forge-io); the documentation tree is synced from [`toolup-forge/docs/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/docs) on every deploy.

If you want a reference implementation of an SSR-only `toolup-forge` consumer, this is one. The repo is small enough to read end to end.

## Reporting an issue with this site

The site repo's [Issues page](https://github.com/toolup-forge/toolup-forge-io/issues) is the place for site bugs (broken layout, missing page, doc that doesn't render). Issues with the SDK itself land on [the forge repo's issue tracker](https://github.com/ToolUp-Forge/toolup-forge/issues).

Found a doc that's wrong? The fix lands in the forge repo's `docs/` tree — propose a change there and the next deploy propagates it here.
