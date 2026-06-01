# Doc-redraft suggestions

Working notes captured during content authoring for `toolup-forge.io`. Each entry names a forge-repo doc that, when consumed *via the site*, surfaces a friction or inaccuracy worth fixing in the source. **These are suggestions for a follow-up conversation with the maintainer of [`toolup-forge`](https://github.com/ToolUp-Forge/toolup-forge); they are not edits to forge docs — the site repo does not write into the SDK repo.**

The suggestions group by target doc. Each entry: what site authoring revealed → suggested redraft shape → why it matters for site consumption.

---

## 1. `CONTRIBUTING.md` — `dotnet test` instruction contradicts `CLAUDE.md`

**Found:** The "Quick start" section at [`CONTRIBUTING.md:30`](https://github.com/ToolUp-Forge/toolup-forge/blob/main/CONTRIBUTING.md) tells contributors to run `dotnet test`. The repo's own `CLAUDE.md` "Build pipeline" section is explicit that `dotnet test` against the Expecto runners exits 0 having run nothing — a silent false-green. The contradiction means a contributor following CONTRIBUTING.md verbatim will think their changes pass tests when nothing ran.

**Suggested redraft:** Replace `dotnet test` in the Quick start block with the per-suite `dotnet run --project` invocations from `CLAUDE.md`. A single sentence above the block can say "test suites are Expecto console runners — see [Build pipeline](CLAUDE.md#build-pipeline) for the full list".

**Why it matters for site consumption:** The site's `/contributing/` page links into this CONTRIBUTING.md as the canonical reference. Site readers who follow the link will land on the contradictory instruction. The site itself can describe the testing convention more carefully, but the canonical doc should be self-consistent.

## 2. `README.md` — `ToolUpSdkVersion` stuck at `0.1.0`

**Found:** [README.md:23](https://github.com/ToolUp-Forge/toolup-forge/blob/main/README.md) shows the meta-manifest example with `<ToolUpSdkVersion>0.1.0</ToolUpSdkVersion>`. The published version is several minor bumps ahead.

**Suggested redraft:** Either (a) hardcode the current version with a note that it's a worked example; (b) use a placeholder like `<ToolUpSdkVersion>0.x.y</ToolUpSdkVersion>` with a callout pointing at the GitHub Packages page for the latest tag; or (c) generate it from a single source-of-truth file during the README's CI build.

**Why it matters for site consumption:** Readers copy-pasting the snippet get the floor version. NuGet will resolve to the latest available, but the appearance of a pinned 0.1.0 anchors expectations on a much older surface than what's published.

## 3. `README.md` — Composition-root snippet drops the `PlatformMode.` prefix

**Found:** [README.md:48](https://github.com/ToolUp-Forge/toolup-forge/blob/main/README.md) shows `Port = 5000; Mode = Individual`. The bare `Individual` resolves but only after an `open ToolUp.Platform` brings the DU's case names into scope. The user-facing memory `project_surfaces_compile_time_precedence.md` calls out that consumer composition roots that don't qualify the mode/surface explicitly hit silent-fallthrough failure modes.

**Suggested redraft:** Qualify: `Mode = PlatformMode.Individual` (or, post-0.4.x, `Surfaces = Surfaces.individual`). Either is fine; both are explicit.

**Why it matters for site consumption:** The README is the SDK's primary getting-started surface — the snippet is the first F# a new reader sees. Modelling explicit qualification here propagates the convention downstream.

## 4. `docs/` — no top-level docs index in the synced tree

**Found:** [`toolup-forge/docs/`](https://github.com/ToolUp-Forge/toolup-forge/tree/main/docs) has per-area `README.md` files (`docs/ai/README.md`, `docs/rag/README.md`, etc.) but no top-level `docs/README.md` or `docs/INDEX.md` orienting the reader to the structure.

**Suggested redraft:** Add a top-level `docs/README.md` that's a tighter version of the site's `/docs/` index — a paragraph per area, a link to each sub-area's README, and a pointer to migrations. The site's `/docs/` page can keep the richer treatment; the README is for GitHub-tree consumption.

**Why it matters for site consumption:** Readers landing on the synced docs tree via GitHub (e.g., from a search result deep-linking to `toolup-forge/docs/`) see a folder listing with no orientation. Sub-area READMEs help once you're inside an area but don't help discover what areas exist.

## 5. `docs/migrations/` — file names lead with version, making URL slugs version-coupled

**Found:** Migration files are named `0.4.0-toolup-elmish-adoption.md`, `0.4.0-oidc-app-config.md`, etc. The SyncDocs target maps these to `/docs/migrations/0.4.0-toolup-elmish-adoption/` slugs.

**Site impact:** URLs are stable (the version is part of the slug, so a migration's URL never moves), which is good — but the *grouping* visible on the site is alphabetical-by-version-prefix rather than topical. A reader who knows "I'm on 0.3.x, what do I need to know to move?" can't currently scan the index by-from-version.

**Suggested redraft (advisory):** Either (a) keep the names and add a frontmatter `appliesFrom: 0.3.x` / `appliesTo: 0.4.0` field that the site groups on; (b) move to `migrations/0.4.0/<slug>.md` per-version subdirectories. (a) is cheaper, (b) is cleaner.

**Why it matters for site consumption:** The site's `/docs/` index page currently surfaces a flat alphabetical list under the Migrations heading. Either change above lets the site render a tabular by-version view.

## 6. `ToolUp.Sdk` meta-manifest — referenced in README + multiple migration docs, no canonical doc

**Found:** The `ToolUp.Sdk` meta-package is mentioned in the README's quick start, in `CLAUDE.md`'s "Versioning" section, in multiple per-package READMEs, and in migration docs. There's no single canonical doc explaining what it does, how the manifest is structured, or how to use it without the coordinated-bump behaviour (i.e., when a consumer wants to bump one companion ahead of the others).

**Suggested redraft:** Add `docs/platform/sdk-meta-manifest.md` covering: what `ToolUp.Sdk` is (a meta-manifest that propagates `<PackageVersion>` entries); how `<ToolUpSdkVersion>` resolves; how to opt out per-package; how the per-package independent-version path works.

**Why it matters for site consumption:** Getting started has to explain this concept *anyway* (it's in the home-page quickstart). A canonical doc to link to keeps the rest of the docs from re-explaining it.

## 7. Per-companion docs follow the 5-file convention (concepts / api-reference / extending / getting-started / README) — but depth varies

**Found:** Spot-checked: `docs/ai/getting-started.md` is rich; `docs/scheduling/` files are present but lightweight. No suggestion to redraft today; flagging that the Stage 4 SyncDocs sweep will surface specific gaps when each doc renders on the site.

**Suggested follow-up:** After the site goes live, walk each `/docs/<area>/<file>/` and produce a per-area thinness audit. Then `Update Cookbook`-style sweep updates land in forge.

## 8. Cross-link discipline between docs

**Found:** Many forge docs link to other forge docs via relative paths (`../platform/architecture.md`, etc.). Once the docs sync into the site, these relative paths need to be rewritten to site-absolute (`/docs/platform/architecture/`). The site's SyncDocs target is the right place to do this — but the audit shape isn't yet clear.

**Suggested redraft (process):** When SyncDocs lands its link-rewrite step (Stage 4 of the application plan), audit every `[...](../...)` reference. Anything that doesn't resolve to a synced doc becomes a TIDY-UP item.

---

## Status

- **None of these are blockers** for the site launch. The site can launch on the docs as-is.
- **All are low-effort.** Most are single-file edits in the forge repo.
- **They batch well.** A single "docs polish for site launch" PR in forge can land items 1-6 together.

When the time comes to act on these, route through forge's [Issues](https://github.com/ToolUp-Forge/toolup-forge/issues) (one issue per item, or one umbrella issue with a checklist) so the discussion sits in the repo that owns the fix.
