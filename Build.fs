module ToolUpForge.Site.Build

open System
open System.IO
open System.Diagnostics
open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

let serverProject = "src/Server/ToolUpForge.Site.fsproj"
let solutionFile = "toolup-forge-io.sln"

/// `../toolup-forge/docs/` — workspace-relative source of doc content.
/// Resolved against the repo root (CWD when `dotnet run -- <target>` is
/// invoked from the repo root). The sync target hard-fails if the sibling
/// is missing — CI checks both repos out before running this.
let forgeDocsSource = Path.Combine("..", "toolup-forge", "docs")
let docsTarget = Path.Combine("content", "docs")

let tailwindInput = "src/Server/styles/index.css"
let tailwindOutput = "src/Server/wwwroot/css/site.css"

/// OSS publication-boundary deny-list per the workspace `CLAUDE.md`
/// "OSS publication boundary (mandate)". Any synced doc file matching
/// one of these terms fails SyncDocs and the build. Defence-in-depth:
/// `toolup-forge/docs/` SHOULD already be clean, but the site rechecks
/// before publishing the synced copy. Case-insensitive substring match.
let deniedVocabulary =
    [
        "ToolUp-Diametrical"
        "Toolup-Analytics"
        "diametrical-roadmap"
        "Fern.UI"
        "Fern.Adapter"
        "ToolUp.Fern"
        "UIControl"
        "Concord"
        "Xcelsys"
        "TaxTimeMachine"
        "Constellation"
        "KnowledgeMart"
        "Marketplace-app"
        "CHEF-GUIDE"
        "cookbook-apps"
        "Refine Roadmap"
        "Suggest features"
        "Accept suggestions"
        "Make it so"
        "Prompt next"
        "Full Analysis"
        "Vision Plan"
        "Development Roadmap"
    ]

// ---------------------------------------------------------------------------
// Context init — Fake.Core.Context is required from net10 since the
// EntryPoint isn't a script (FCS-style top-level expressions).
// ---------------------------------------------------------------------------

let initTargets (args: string array) =
    Context.FakeExecutionContext.Create false "build.fsx" (Array.toList args)
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/// Run a process synchronously, fail the build on non-zero exit.
let private runProc (exe: string) (args: string) (workingDir: string) =
    let psi = ProcessStartInfo(exe, args)
    psi.WorkingDirectory <- workingDir
    psi.UseShellExecute <- false
    let p = Process.Start psi
    p.WaitForExit()
    if p.ExitCode <> 0 then
        failwithf "Process '%s %s' (cwd '%s') exited with %d" exe args workingDir p.ExitCode

/// Resolve npx via the workspace's npx.cmd (avoids the Node 22.x
/// npm.ps1 / npx.ps1 substring-slice bug — see workspace `CLAUDE.md`
/// "Sibling launcher conventions (mandate)").
let private npxCmd =
    let pf86 = Environment.GetFolderPath Environment.SpecialFolder.ProgramFilesX86
    let pf = Environment.GetFolderPath Environment.SpecialFolder.ProgramFiles
    let candidates = [
        Path.Combine(pf, "nodejs", "npx.cmd")
        Path.Combine(pf86, "nodejs", "npx.cmd")
        "npx.cmd"
    ]
    candidates |> List.tryFind File.Exists |> Option.defaultValue "npx.cmd"

let private repoRoot = Path.GetFullPath "."

// ---------------------------------------------------------------------------
// Targets
// ---------------------------------------------------------------------------

let registerTargets () =
    Target.create "Clean" (fun _ ->
        !! "src/**/bin"
        ++ "src/**/obj"
        ++ "obj"
        ++ "src/Server/wwwroot/css/site.css"
        ++ "src/Server/wwwroot/css/site.css.map"
        |> Shell.cleanDirs
    )

    Target.create "Format" (fun _ ->
        Trace.trace "==> fantomas src/ Build.fs"
        runProc "dotnet" "fantomas src/ Build.fs" repoRoot
    )

    Target.create "FormatCheck" (fun _ ->
        Trace.trace "==> fantomas --check src/ Build.fs"
        runProc "dotnet" "fantomas --check src/ Build.fs" repoRoot
    )

    Target.create "Tailwind" (fun _ ->
        Trace.tracefn "==> tailwindcss -i %s -o %s --minify" tailwindInput tailwindOutput

        // Use the local @tailwindcss/cli installed via package.json. npx
        // (not npm) — the css build script is npm-callable but the npx
        // form skips the npm.ps1 shim entirely and is more reliable from
        // a FAKE driver.
        let outDir = Path.GetDirectoryName tailwindOutput
        if not (Directory.Exists outDir) then
            Directory.CreateDirectory outDir |> ignore

        let args = sprintf "tailwindcss -i %s -o %s --minify" tailwindInput tailwindOutput
        runProc npxCmd args repoRoot
    )

    Target.create "SyncDocs" (fun _ ->
        let absSource = Path.GetFullPath forgeDocsSource
        if not (Directory.Exists absSource) then
            failwithf
                "SyncDocs: source '%s' not found. The toolup-forge sibling must be checked out next to this repo (CI does this automatically; locally clone it as ../toolup-forge/)."
                absSource

        Trace.tracefn "==> SyncDocs: copying %s -> %s" absSource docsTarget

        // Wipe + recreate target so deletions in source propagate.
        if Directory.Exists docsTarget then
            Shell.cleanDir docsTarget
        else
            Directory.CreateDirectory docsTarget |> ignore

        // Mirror tree (markdown only — README.md, *.md). Other artefacts
        // in toolup-forge/docs/ (assets/, images/, code-snippet sources)
        // are picked up in Stage 4 once the docs ingestion design lands.
        let mdFiles =
            Directory.EnumerateFiles(absSource, "*.md", SearchOption.AllDirectories)
            |> Seq.toList

        Trace.tracefn "    %d markdown files to scan" (List.length mdFiles)

        // Deny-list check — defence in depth against OSS publication
        // boundary leaks (per workspace CLAUDE.md).
        let mutable violations = []
        for src in mdFiles do
            let content = File.ReadAllText src

            for term in deniedVocabulary do
                if content.Contains(term, StringComparison.OrdinalIgnoreCase) then
                    violations <- (src, term) :: violations

        match violations with
        | [] -> ()
        | hits ->
            Trace.traceError "SyncDocs: OSS publication-boundary deny-list violations:"
            for (file, term) in hits do
                Trace.traceErrorfn "    %s — contains '%s'" file term

            failwithf
                "SyncDocs: %d deny-list violation(s). Remove the references from toolup-forge/docs/ before publishing."
                (List.length hits)

        // Copy. Preserve relative tree.
        for src in mdFiles do
            let relative = Path.GetRelativePath(absSource, src)
            let dst = Path.Combine(docsTarget, relative)
            let dstDir = Path.GetDirectoryName dst
            if not (Directory.Exists dstDir) then
                Directory.CreateDirectory dstDir |> ignore
            File.Copy(src, dst, overwrite = true)

        Trace.tracefn "    %d files synced. content/docs/ is up to date." (List.length mdFiles)
    )

    Target.create "Build" (fun _ ->
        Trace.trace "==> dotnet build"
        DotNet.build (fun opts -> { opts with NoLogo = true }) solutionFile
    )

    Target.create "Run" (fun _ ->
        Trace.trace "==> dotnet run (server)"
        runProc "dotnet" (sprintf "run --project %s --no-build" serverProject) repoRoot
    )

    Target.create "Publish" (fun _ ->
        Trace.trace "==> dotnet publish"

        DotNet.publish
            (fun opts ->
                { opts with
                    NoLogo = true
                    OutputPath = Some(Path.GetFullPath "publish")
                    Configuration = DotNet.BuildConfiguration.Release
                })
            serverProject
    )

    Target.create "Default" ignore
    Target.create "Verify" ignore

    // Default workflow: Format ⇒ Tailwind ⇒ Build ⇒ Run.
    "Format" ==> "Tailwind" ==> "Build" ==> "Run" ==> "Default" |> ignore

    // CI / pre-commit shape: FormatCheck ⇒ SyncDocs ⇒ Tailwind ⇒ Build.
    "FormatCheck" ==> "SyncDocs" ==> "Tailwind" ==> "Build" ==> "Verify" |> ignore

[<EntryPoint>]
let main args =
    initTargets args
    registerTargets ()

    let target =
        match args |> Array.tryHead with
        | Some t -> t
        | None -> "Default"

    Target.runOrDefaultWithArguments target
    0
