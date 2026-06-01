module ToolUpForge.Site.Layouts.PageLayout

open Giraffe.ViewEngine
open ToolUp.PublicRendering

/// Marketing-page layout — Home, Why Forge, Getting started, Contributing.
/// Narrow reading column (~720-880px), centred. No sidebar. The page's
/// markdown body is rendered into a typographic container styled via the
/// `.prose` utility classes in Tailwind.
let render (page: PublicPage) : XmlNode =
    let bodyHtml =
        match page.Body with
        | Html fragment -> rawText fragment
        | Markdown source -> rawText source

    let structuredData = StructuredDataHelpers.organization page

    let pageBody =
        div [ _class "mx-auto max-w-3xl px-6 py-12" ] [
            article [] [
                header [ _class "mb-10" ] [
                    h1 [ _class "text-4xl font-semibold tracking-tight text-fg-emphasis" ] [
                        str page.Title
                    ]
                    (match page.Description with
                     | "" -> rawText ""
                     | desc -> p [ _class "mt-3 text-lg text-muted" ] [ str desc ])
                ]
                div [ _class "prose prose-toolup max-w-none" ] [ bodyHtml ]
            ]
            script [ _type "application/ld+json" ] [ rawText structuredData ]
        ]

    BaseLayout.render page "flex-1" pageBody
