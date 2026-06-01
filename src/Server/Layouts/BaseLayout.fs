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
    header [
        _class "border-b border-border bg-white sticky top-0 z-30"
    ] [
        div [ _class "mx-auto max-w-7xl px-6 h-14 flex items-center gap-8" ] [
            // Brand mark — text wordmark for v0; replace with SVG logo once
            // the design lands.
            a [
                _href "/"
                _class "flex items-center gap-2 text-brand font-semibold tracking-tight"
            ] [
                span [ _class "text-lg" ] [ str "toolup-forge" ]
                span [ _class "text-muted text-sm font-normal" ] [ str "/ docs" ]
            ]

            // Primary nav — text links, brand purple on hover.
            nav [ _class "flex items-center gap-6 text-sm text-fg" ] [
                a [ _href "/docs/"; _class "hover:text-brand transition-colors" ] [ str "Docs" ]
                a [
                    _href "/getting-started/"
                    _class "hover:text-brand transition-colors"
                ] [ str "Getting started" ]
                a [
                    _href "/companions/"
                    _class "hover:text-brand transition-colors"
                ] [ str "Companions" ]
                a [ _href "/releases/"; _class "hover:text-brand transition-colors" ] [ str "Releases" ]
            ]

            // Right-aligned external links.
            div [ _class "ml-auto flex items-center gap-4 text-sm" ] [
                a [
                    _href "https://github.com/ToolUp-Forge/toolup-forge"
                    _target "_blank"
                    _rel "noopener noreferrer"
                    _class "text-fg hover:text-brand transition-colors"
                ] [ str "GitHub" ]
            ]
        ]
    ]

let private siteFooter =
    footer [ _class "border-t border-border bg-surface mt-16" ] [
        div [
            _class "mx-auto max-w-7xl px-6 py-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between"
        ] [
            div [ _class "text-sm text-muted" ] [
                str "Built with "
                a [
                    _href "https://github.com/ToolUp-Forge/toolup-forge"
                    _class "text-brand hover:text-brand-dark underline-offset-2 hover:underline"
                ] [ str "toolup-forge" ]
                str " · Apache 2.0"
            ]
            nav [ _class "flex gap-4 text-sm text-muted" ] [
                a [ _href "/sitemap.xml"; _class "hover:text-brand" ] [ str "Sitemap" ]
                a [ _href "/about/"; _class "hover:text-brand" ] [ str "About" ]
                a [
                    _href "https://github.com/ToolUp-Forge/toolup-forge/blob/main/CODE_OF_CONDUCT.md"
                    _class "hover:text-brand"
                ] [ str "Code of Conduct" ]
            ]
        ]
    ]

/// Compose `<html>` / `<head>` / `<body>` around any page body content.
/// `bodyClasses` lets Page / Doc layouts adjust the main container shape
/// (Page is narrow + centred, Doc is wider with a sidebar).
let render (page: PublicPage) (bodyClasses: string) (bodyContent: XmlNode) : XmlNode =
    html [ _lang "en" ] [
        head [] [
            meta [ _charset "utf-8" ]
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [ str (sprintf "%s — toolup-forge" page.Title) ]
            meta [ _name "description"; _content page.Description ]

            // OpenGraph / Twitter card. Shared default OG image; per-page
            // overrides flow through frontmatter `og:image`.
            meta [ _property "og:title"; _content page.Title ]
            meta [ _property "og:description"; _content page.Description ]
            meta [ _property "og:type"; _content "website" ]
            (match Map.tryFind "og:image" page.Frontmatter with
             | Some img -> meta [ _property "og:image"; _content img ]
             | None -> meta [ _property "og:image"; _content "/og-default.png" ])
            meta [ _name "twitter:card"; _content "summary_large_image" ]

            // Tailwind output. Compiled to wwwroot/css/site.css by the
            // Build.fs Tailwind target (or `npm run build:css`).
            link [ _rel "stylesheet"; _href "/css/site.css" ]

            // Favicon + manifest — placeholder; replaced by a real set in
            // the launch sweep.
            link [ _rel "icon"; _type "image/svg+xml"; _href "/favicon.svg" ]
        ]
        body [ _class "bg-bg text-fg font-sans antialiased min-h-screen flex flex-col" ] [
            siteHeader
            main [ _class bodyClasses ] [ bodyContent ]
            siteFooter
        ]
    ]
