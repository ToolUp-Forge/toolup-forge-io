# toolup-forge-io

The public website for [`toolup-forge`](https://github.com/ToolUp-Forge/toolup-forge) — the open-source F# full-stack SDK.

**Live:** _v0 launch pending_ — will resolve to `https://toolup-forge.io/` once Stage 6 deploy lands.

## What this is

The developer-targeted documentation and landing surface for `toolup-forge`. **Built with `toolup-forge` itself** — every page is server-side-rendered F# via `ToolUp.PublicRendering`, demonstrating the SDK on the same surface it documents.

- **Landing + getting-started** — for developers evaluating the SDK.
- **Documentation** — every page under [`toolup-forge/docs/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/docs) synced into the site at build time. The forge repo's `docs/` tree is the source of truth; this repo renders it.
- **Companion catalogue** — every published `ToolUp.*` companion package with a link to its docs page, source, and NuGet entry.
- **Releases** — aggregated changelog.

## Stack

- **F#** (.NET 10) end-to-end. No JavaScript source files (the search UI ships a pre-built `pagefind` bundle as a static asset).
- **[`ToolUp.PublicRendering`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/src/ToolUp.PublicRendering)** — SSR substrate (Phase 38).
- **Giraffe.ViewEngine** — HTML rendering DSL.
- **Tailwind CSS v4** — compiled at build time; no JS framework in the browser.
- **Markdig** — markdown → HTML (transitive via `ToolUp.PublicRendering`).

## Running locally

Requires .NET 10 SDK + Node 22+ (for the Tailwind CLI).

```powershell
pwsh ./run.ps1
```

Serves on `http://localhost:13940/` per the workspace port allocation.

## Repository layout

```
toolup-forge-io/
├── src/Server/                # Composition root + layouts + Tailwind input
│   ├── Program.fs             # ToolUp.PublicRendering composition
│   ├── Layouts/               # Giraffe.ViewEngine layouts (Base, Page, Doc)
│   ├── styles/index.css       # Tailwind v4 entry — canonical contract + overrides
│   └── wwwroot/               # Static assets (favicon, og image, compiled CSS)
├── content/
│   ├── pages/                 # Marketing pages (markdown + YAML frontmatter)
│   └── docs/                  # Synced at build from ../toolup-forge/docs/ (gitignored)
├── Build.fs / Build.fsproj    # FAKE pipeline (Run, Build, Format, Tailwind, SyncDocs)
├── run.ps1                    # Universal entry point per workspace mandate
└── toolup-forge-io.sln
```

## Contributing

Issues and PRs welcome. The same Apache-2.0 / DCO / Contributor Covenant posture as [`toolup-forge`](https://github.com/ToolUp-Forge/toolup-forge) applies — see that repo's `CONTRIBUTING.md` / `CODE_OF_CONDUCT.md` / `SECURITY.md`.

## Licence

Apache 2.0 — see [`LICENSE`](LICENSE).
