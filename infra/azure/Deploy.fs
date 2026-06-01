module ToolUpForge.Site.Deploy

open System
open Farmer
open Farmer.Builders

// Farmer template for the public website at https://toolup-forge.io.
// Website-class deployment shape: SSR-only F#, no Fable client, no AI,
// no auth. Far simpler than the toolup-app commercial deploy — no
// Entra, no Anthropic key, no AG Grid Enterprise.
//
// Mirrors the canonical pattern documented in
// ../../deployment-docs/AZURE-APP-SERVICE-DEPLOYMENT.md — App Service
// Linux on DOTNETCORE|10.0, decoupled ARM apply + zip-deploy with a
// 60s settle gap between, warmup-probe configuration that survives
// the Farmer 1.9.27 quirks. The site has no commercial knobs to
// forward (no TOOLUP_OIDC_*, no ANTHROPIC_API_KEY); just the platform
// constants plus a small site-specific set.

let private env name = Environment.GetEnvironmentVariable name

let private envOr name fallback =
    match env name with
    | null
    | "" -> fallback
    | v -> v

let private appName = envOr "AZURE_APP_NAME" "toolup-forge-io"
let private rgName = envOr "AZURE_RESOURCE_GROUP" "toolup-forge-io"
let private region = envOr "AZURE_REGION" "westeurope"
let private skuName = envOr "AZURE_SKU" "F1"

let private selectedSku =
    match (skuName: string).ToUpperInvariant() with
    | "F1" -> WebApp.Sku.F1
    | "B1" -> WebApp.Sku.B1
    | "B2" -> WebApp.Sku.B2
    | "B3" -> WebApp.Sku.B3
    | "S1" -> WebApp.Sku.S1
    | "S2" -> WebApp.Sku.S2
    | "S3" -> WebApp.Sku.S3
    | other -> failwithf "Unrecognised AZURE_SKU %s — set one of F1/B1/B2/B3/S1/S2/S3" other

// ─── App settings ────────────────────────────────────────────────────
//
// The site has fewer runtime knobs than toolup-app — no Entra, no AI
// provider keys, no client-side bundle defines. Only the platform
// constants (warmup, port pinning, no-build-on-deploy) plus a small
// site-specific set live here. Every `TOOLUP_*` env var in the runner's
// process environment is also forwarded verbatim so an operator can
// add a new TOOLUP_FOO variable on the `cd` GitHub Environment
// without an edit here.

let private toolupAppSettings =
    Environment.GetEnvironmentVariables()
    |> Seq.cast<Collections.DictionaryEntry>
    |> Seq.choose (fun e ->
        let k = e.Key :?> string
        let v = e.Value :?> string

        if k.StartsWith("TOOLUP", StringComparison.Ordinal) then
            Some(k, v)
        else
            None)
    |> List.ofSeq

let private platformAppSettings =
    [
      // Kestrel + Azure App Service Linux's built-in DOTNETCORE stack
      // wire the warmup probe at port 8080 (internal 169.254.129.2:8080,
      // regardless of WEBSITES_PORT). The SDK reads SERVER_PORT first
      // (forge SDK.Server.fs:186) and falls back to ServerConfig.Port =
      // 13940 — so SERVER_PORT=8080 here overrides the local-dev port
      // (13940 per the workspace port table) without editing Program.fs.
      // Both settings live in IaC so Farmer's REPLACE-not-MERGE apply
      // can't wipe them.
      "SERVER_PORT", "8080"
      "WEBSITES_PORT", "8080"
      // The bundle is a self-contained `dotnet publish` output — no need
      // for Oryx to attempt a fresh build at zip-deploy time.
      "SCM_DO_BUILD_DURING_DEPLOYMENT", "false"
      // Warmup probe. See deployment-docs/AZURE-APP-SERVICE-DEPLOYMENT.md
      // §4 for the gotcha — without these three, the default 230s probe
      // window kills the container before the SDK's /health endpoint is
      // ready, and the site enters a permanent restart loop.
      "WEBSITE_WARMUP_PATH", "/health"
      "WEBSITE_WARMUP_STATUSES", "200,404"
      "WEBSITES_CONTAINER_START_TIME_LIMIT", "1800"
      // Behind App Service's TLS-terminating proxy — required for the
      // SDK to honour X-Forwarded-* correctly so generated URLs use
      // HTTPS and the real client IP reaches handlers.
      "TOOLUP_TRUST_FORWARDED_HEADERS", "1"
      // Production deployments hard-require the static path exists.
      // The bundle ships wwwroot/ alongside the DLL; if Farmer's ARM
      // apply or a buggy publish drops it, fail loud at boot rather
      // than serve a CSS-less site.
      "TOOLUP_STATIC_PATH_BEHAVIOUR", "require"
      // Public base URL — used by sitemap absolute URLs + JSON-LD.
      // Starts at the azurewebsites.net default; flip to https://toolup-forge.io
      // via GitHub Variable override (TOOLUPFORGE_SITE_BASE_URL) once
      // the custom domain is wired.
      "TOOLUPFORGE_SITE_BASE_URL", envOr "TOOLUPFORGE_SITE_BASE_URL" (sprintf "https://%s.azurewebsites.net" appName)
      // DO NOT set TOOLUP_REQUIRE_HTTPS — breaks the App Service warmup
      // probe. HTTPS is enforced at the Azure layer via Farmer's
      // `https_only` directive (sets httpsOnly=true on the WebApp ARM
      // resource).
      ]

let private allAppSettings = platformAppSettings @ toolupAppSettings

// ─── Resource graph ──────────────────────────────────────────────────

let private plan =
    servicePlan {
        name (sprintf "%s-plan" appName)
        sku selectedSku
        operating_system Linux
    }

// `always_on` is incompatible with F1 — F1 is a free SKU that
// explicitly does not support always-on. The Farmer `Sku` type is a
// class (not a DU), so the conditional uses the lowercased SKU name
// rather than a pattern match.
let private toolupForgeIoApp =
    if (skuName: string).ToUpperInvariant() = "F1" then
        webApp {
            name appName
            link_to_service_plan plan
            runtime_stack (Runtime.DotNet "10.0")
            settings allAppSettings
            https_only
        }
    else
        webApp {
            name appName
            link_to_service_plan plan
            runtime_stack (Runtime.DotNet "10.0")
            settings allAppSettings
            https_only
            always_on
        }

let private deployment =
    arm {
        location (Location region)
        add_resource plan
        add_resource toolupForgeIoApp
    }

// ─── Entry point ─────────────────────────────────────────────────────

[<EntryPoint>]
let main argv =
    match argv |> Array.tryHead with
    | Some "cd" ->
        printfn "Deploying %s to resource group %s in %s (sku %s)" appName rgName region skuName

        printfn
            "Forwarding %d TOOLUP_* app settings + %d platform constants"
            toolupAppSettings.Length
            platformAppSettings.Length

        deployment |> Deploy.execute rgName Deploy.NoParameters |> ignore

        0
    | _ ->
        eprintfn "Usage: dotnet run --project infra/azure/Deploy.fsproj -- cd"
        eprintfn ""
        eprintfn "Env-var parameters (with defaults shown):"
        eprintfn "  AZURE_APP_NAME           %s" appName
        eprintfn "  AZURE_RESOURCE_GROUP     %s" rgName
        eprintfn "  AZURE_REGION             %s" region
        eprintfn "  AZURE_SKU                %s" skuName

        eprintfn
            "  TOOLUPFORGE_SITE_BASE_URL  %s (defaults to https://<app-name>.azurewebsites.net)"
            (envOr "TOOLUPFORGE_SITE_BASE_URL" "")

        1
