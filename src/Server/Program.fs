module ToolUpForge.Site.Program

open System
open ToolUp.Platform
open ToolUp.Platform.Server
open ToolUp.PublicRendering

/// toolup-forge-io composition root. Website-class — SSR-only, no Fable
/// client, no AI, no modules. ServerConfig is the minimum viable shape:
/// Anonymous mode, PublicRendering enabled with `content/` as the root,
/// platform defaults stripped (no monetary values, no data ingestion,
/// no usage dashboard — the site renders prose, that's all).

[<EntryPoint>]
let main _ =
    // `content/` is copied into the output folder via the fsproj's
    // <None Include="..\..\content\**\*" .../>; the running binary
    // resolves against AppContext.BaseDirectory so dev + bin-deploy
    // behave identically.
    let contentPath = IO.Path.Combine(AppContext.BaseDirectory, "content")

    // Production swaps PublicBaseUrl to `https://toolup-forge.io` via an
    // env-var read added at Stage 6 (deploy). v0 local-dev stays on
    // localhost:13940.
    let publicBaseUrl =
        Environment.GetEnvironmentVariable "TOOLUPFORGE_SITE_BASE_URL"
        |> Option.ofObj
        |> Option.defaultValue "http://localhost:13940"

    // Static-files root. The SDK's UseStaticFiles middleware binds to
    // `config.PublicPath`; the website-class deploy serves the compiled
    // Tailwind output + favicon from `wwwroot/`. Resolved against
    // AppContext.BaseDirectory so dev + bin-deploy behave identically
    // (the fsproj copies `wwwroot/**/*` to the output directory).
    let publicPath = IO.Path.Combine(AppContext.BaseDirectory, "wwwroot")

    let config =
        { ServerConfig.defaults with
            Port = 13940
            // 0.4.x replaced `Mode: PlatformMode` with `Surfaces: SurfaceProfile
            // list`. `Surfaces.anonymous` from ToolUp.Platform.Core is itself a
            // singleton `[ SurfaceProfile.anonymous ]`, so no wrapping is
            // needed. Hardcoding the shape here is deliberate per the
            // surfaces-compile-time precedence finding — relying on `fromEnv`
            // to read `TOOLUP_PLATFORM_SURFACES` is fragile when an overrides
            // record default pins another value.
            Surfaces = Surfaces.anonymous
            PublicBaseUrl = Some publicBaseUrl
            PublicPath = publicPath
            PublicRendering = EnabledPublicRendering(ContentRoot contentPath)
            IncludePlatformDefaults = false }

    let logger = ConsoleLogger.ConsoleLogger() :> ILogger

    PublicRenderingCompose.PublicRenderingServerApp.create ()
    |> PublicRenderingCompose.PublicRenderingServerApp.withConfig config
    |> PublicRenderingCompose.PublicRenderingServerApp.withLogger logger
    |> PublicRenderingCompose.PublicRenderingServerApp.withLayout (LayoutName "page") Layouts.PageLayout.render
    |> PublicRenderingCompose.PublicRenderingServerApp.withLayout (LayoutName "doc") Layouts.DocLayout.render
    |> PublicRenderingCompose.PublicRenderingServerApp.run
