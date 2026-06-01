module ToolUpForge.Site.Layouts.DocLayout

open Giraffe.ViewEngine
open ToolUp.PublicRendering

/// Documentation-page layout — used for every page under /docs/. Three
/// columns at desktop width: left sidebar (section nav), centre content
/// (the doc body), right rail (in-page table of contents — populated by
/// a Markdig pipeline step in Stage 4). At narrower widths the sidebar
/// collapses to a top-of-page disclosure.
///
/// The nav tree itself is intentionally hard-coded for now — Stage 4
/// generates it from the synced `content/docs/` directory. The shape
/// here is the structural placeholder.

let private docNav =
    aside [
        _class
            "hidden lg:block lg:w-64 lg:flex-none lg:border-r lg:border-border lg:bg-surface lg:py-8 lg:pr-6 lg:pl-6"
    ] [
        nav [ _class "text-sm" ] [
            // Stage 4 replaces this hand-coded tree with a generated one
            // derived from content/docs/. The structure mirrors
            // toolup-forge/docs/.
            div [ _class "space-y-6" ] [
                section [] [
                    h3 [ _class "text-xs font-semibold uppercase tracking-wider text-muted mb-2" ] [
                        str "Platform"
                    ]
                    ul [ _class "space-y-1" ] [
                        li [] [
                            a [
                                _href "/docs/platform/architecture/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Architecture" ]
                        ]
                        li [] [
                            a [
                                _href "/docs/platform/composition-roots/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Composition roots" ]
                        ]
                        li [] [
                            a [
                                _href "/docs/platform/modules/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Modules" ]
                        ]
                        li [] [
                            a [
                                _href "/docs/platform/portability-rules/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Portability rules" ]
                        ]
                    ]
                ]
                section [] [
                    h3 [ _class "text-xs font-semibold uppercase tracking-wider text-muted mb-2" ] [
                        str "Companions"
                    ]
                    ul [ _class "space-y-1" ] [
                        li [] [
                            a [
                                _href "/docs/ai/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "AI" ]
                        ]
                        li [] [
                            a [
                                _href "/docs/rag/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "RAG" ]
                        ]
                        li [] [
                            a [
                                _href "/docs/knowledge-base/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Knowledge Base" ]
                        ]
                        li [] [
                            a [
                                _href "/docs/forms/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Forms" ]
                        ]
                        li [] [
                            a [
                                _href "/docs/scheduling/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Scheduling" ]
                        ]
                    ]
                ]
                section [] [
                    h3 [ _class "text-xs font-semibold uppercase tracking-wider text-muted mb-2" ] [
                        str "Reference"
                    ]
                    ul [ _class "space-y-1" ] [
                        li [] [
                            a [
                                _href "/docs/design/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Design canon" ]
                        ]
                        li [] [
                            a [
                                _href "/docs/migrations/"
                                _class "block px-2 py-1 rounded hover:bg-bg-light hover:text-brand"
                            ] [ str "Migrations" ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let render (page: PublicPage) : XmlNode =
    let bodyHtml =
        match page.Body with
        | Html fragment -> rawText fragment
        | Markdown source -> rawText source

    let layoutBody =
        div [ _class "mx-auto max-w-7xl flex flex-1 w-full" ] [
            docNav
            div [ _class "flex-1 min-w-0 px-6 py-10 lg:px-10" ] [
                article [ _class "prose prose-toolup max-w-3xl" ] [
                    header [ _class "mb-8" ] [
                        h1 [ _class "text-3xl font-semibold tracking-tight text-fg-emphasis" ] [
                            str page.Title
                        ]
                        (match page.Description with
                         | "" -> rawText ""
                         | desc -> p [ _class "mt-2 text-base text-muted not-prose" ] [ str desc ])
                    ]
                    bodyHtml
                ]
            ]
        ]

    BaseLayout.render page "flex-1 flex" layoutBody
