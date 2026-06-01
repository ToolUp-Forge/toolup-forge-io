module ToolUpForge.Site.Layouts.BaseLayout

open Giraffe.ViewEngine
open ToolUp.PublicRendering

/// Shared shell — `<html>` / `<head>` / `<body>` skeleton, header with
/// brand mark + global nav, footer, and the link to the compiled
/// Tailwind stylesheet. Page-specific layouts (Page, Doc) compose this
/// with their content. GitHub-Pages-style minimal chrome — brand colour
/// reserved for the wordmark + links + primary action; everything else
/// is neutral light surface so docs content reads first.

let private siteHeader =
    header
        [ _class "border-b border-border bg-white sticky top-0 z-30" ]
        [ div
              [ _class "mx-auto max-w-7xl px-6 h-14 flex items-center gap-8" ]
              [
                // Brand mark — forge iconset SVG (the ⟨ ⟩ brackets-around-mark
                // primary logo) plus the wordmark in text. Renders crisply at
                // any size; falls back to text if the SVG fails to load. The
                // canonical brand assets live in the forge repo's
                // Brand Assets/logos/iconset/ and are copied into wwwroot/
                // verbatim.
                a
                    [ _href "/"
                      _class "flex items-center gap-2 text-fg-emphasis font-semibold tracking-tight" ]
                    [ img
                          [ _src "/svg/icon-mark.svg"
                            _alt "toolup-forge"
                            _class "h-7 w-auto"
                            _width "28"
                            _height "28" ]
                      span [ _class "text-lg" ] [ str "toolup-forge" ] ]

                // Primary nav — text links, brand purple on hover.
                nav
                    [ _class "flex items-center gap-6 text-sm text-fg" ]
                    [ a [ _href "/docs"; _class "hover:text-brand transition-colors" ] [ str "Docs" ]
                      a
                          [ _href "/getting-started"; _class "hover:text-brand transition-colors" ]
                          [ str "Getting started" ]
                      a [ _href "/companions"; _class "hover:text-brand transition-colors" ] [ str "Companions" ]
                      a [ _href "/releases"; _class "hover:text-brand transition-colors" ] [ str "Releases" ] ]

                // Right-aligned external links.
                div
                    [ _class "ml-auto flex items-center gap-4 text-sm" ]
                    [ a
                          [ _href "https://github.com/ToolUp-Forge/toolup-forge"
                            _target "_blank"
                            _rel "noopener noreferrer"
                            _class "text-fg hover:text-brand transition-colors" ]
                          [ str "GitHub" ] ] ] ]

let private siteFooter =
    // "Powered by ToolUp Forge" — the canonical attribution every
    // ToolUp-built website renders, mirroring the SDK shell's sidebar
    // footer convention. The official forge wordmark (icon-mark
    // brackets-around-mark + "ToolUp /forge" wordmark, light-background
    // variant from Brand Assets/logos/iconset/repo/) is the visible
    // mark; "Powered by" is the small leading label. Consumers who
    // fork their layout can keep, vary, or remove the badge per their
    // own brand posture (it's a default, not a licence term).
    footer
        [ _class "border-t border-border bg-surface mt-16" ]
        [ div
              [ _class "mx-auto max-w-7xl px-6 py-8 flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between" ]
              [ a
                    [ _href "/"
                      _title "Powered by ToolUp Forge"
                      _class
                          "flex items-center gap-3 text-sm text-muted hover:text-fg-emphasis transition-colors no-underline" ]
                    [ span [ _class "uppercase tracking-wider text-xs" ] [ str "Powered by" ]
                      // The two transparent wordmark variants ship together
                      // so the badge tracks `prefers-color-scheme` without a
                      // CSS-controlled image swap. Picture-element fallback:
                      // a browser that doesn't understand <picture> reads
                      // the inner <img> directly — i.e. the dark-text
                      // (light-mode) variant, which is the right default
                      // since the site is light-only today.
                      picture
                          []
                          [ source
                                [ _media "(prefers-color-scheme: dark)"
                                  _srcset "/repo/icon-wordmark-transparent-1024.png" ]
                            img
                                [ _src "/repo/icon-wordmark-transparent-dark-text-1024.png"
                                  _alt "ToolUp Forge"
                                  _class "h-12 w-auto"
                                  _width "48"
                                  _height "48" ] ]
                      span [ _class "text-muted/70 hidden sm:inline" ] [ str "· Apache 2.0" ] ]
                nav
                    [ _class "flex flex-wrap gap-4 text-sm text-muted" ]
                    [ a [ _href "/sitemap.xml"; _class "hover:text-brand" ] [ str "Sitemap" ]
                      a [ _href "/about"; _class "hover:text-brand" ] [ str "About" ]
                      a
                          [ _href "https://github.com/ToolUp-Forge/toolup-forge"
                            _class "hover:text-brand" ]
                          [ str "GitHub" ]
                      a
                          [ _href "https://github.com/ToolUp-Forge/toolup-forge/blob/main/CODE_OF_CONDUCT.md"
                            _class "hover:text-brand" ]
                          [ str "Code of Conduct" ] ] ] ]

/// Compose `<html>` / `<head>` / `<body>` around any page body content.
/// `bodyClasses` lets Page / Doc layouts adjust the main container shape
/// (Page is narrow + centred, Doc is wider with a sidebar).
let render (page: PublicPage) (bodyClasses: string) (bodyContent: XmlNode) : XmlNode =
    html
        [ _lang "en" ]
        [ head
              []
              [ meta [ _charset "utf-8" ]
                meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
                title [] [ str (sprintf "%s — toolup-forge" page.Title) ]
                meta [ _name "description"; _content page.Description ]

                // `<meta name="generator">` — the standard HTML convention
                // for declaring what built this page. Indexed by
                // Wappalyzer / BuiltWith / etc., so any site running
                // `ToolUp.PublicRendering` is discoverable as a forge
                // consumer without anything visible on the page itself.
                // Pairs with the visible "Powered by ToolUp Forge" footer:
                // visible for humans, meta tag for crawlers + tooling.
                // Will move into the SDK substrate (default emitted by
                // `ToolUp.PublicRendering` directly) in a future forge
                // bump; declared explicitly here for now.
                meta [ _name "generator"; _content "ToolUp Forge — toolup-forge.io" ]

                // OpenGraph / Twitter card. Default OG image is the forge
                // social-preview banner (1280×640, horizontal lockup
                // brackets-around-mark + wordmark + tagline); per-page
                // overrides flow through frontmatter `og:image`.
                meta [ _property "og:title"; _content page.Title ]
                meta [ _property "og:description"; _content page.Description ]
                meta [ _property "og:type"; _content "website" ]
                (match Map.tryFind "og:image" page.Frontmatter with
                 | Some img -> meta [ _property "og:image"; _content img ]
                 | None -> meta [ _property "og:image"; _content "/social/social-preview-1280x640.png" ])
                meta [ _name "twitter:card"; _content "summary_large_image" ]
                meta [ _name "twitter:image"; _content "/social/social-preview-1280x640.png" ]

                // Tailwind output. Compiled to wwwroot/css/site.css by the
                // Build.fs Tailwind target (or `npm run build:css`).
                link [ _rel "stylesheet"; _href "/css/site.css" ]

                // Forge brand iconset — matches Brand Assets/logos/iconset/
                // head-snippet.html verbatim. Browser tab favicons, Apple
                // touch icon, PWA manifest, theme colour for mobile chrome
                // (brand purple #7A3BD9), and the OG / Twitter social-
                // preview banner.
                link
                    [ _rel "icon"
                      _type "image/png"
                      _sizes "32x32"
                      _href "/favicon/favicon-32.png" ]
                link
                    [ _rel "icon"
                      _type "image/png"
                      _sizes "16x16"
                      _href "/favicon/favicon-16.png" ]
                link
                    [ _rel "apple-touch-icon"
                      _sizes "180x180"
                      _href "/apple/apple-touch-icon.png" ]
                link [ _rel "manifest"; _href "/site.webmanifest" ]
                meta [ _name "theme-color"; _content "#7A3BD9" ]
                meta [ _property "og:image:width"; _content "1280" ]
                meta [ _property "og:image:height"; _content "640" ] ]
          body
              [ _class "bg-bg text-fg font-sans antialiased min-h-screen flex flex-col" ]
              [ siteHeader; main [ _class bodyClasses ] [ bodyContent ]; siteFooter ] ]
