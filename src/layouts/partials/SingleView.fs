namespace Felizia.Partials

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
#endif

open Felizia.Model

[<AutoOpen>]
module SingleView =
    // Server side react
    let singleView (model: Model) dispatch =
        let site = model.CurrentSite

        Html.div [
            Navbar.navbar color.isWhite model dispatch

            Bulma.section [
                Content.content model dispatch

                // Tag-list

                Bulma.container [
                    paginationSingle model dispatch

                    if site.Params.Share then
                        share model dispatch
                    else
                        Html.none
                ]
            ]

            footer model dispatch
        ]
