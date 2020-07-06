namespace Felizia.Arctic.Partials

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
#endif

open Felizia

[<AutoOpen>]
module Index =
    let index (model: Model) dispatch =
        let page = model.CurrentPage

        Html.div [
            navbarHero model dispatch

            Bulma.section [
                cards model dispatch

                Bulma.container [
                    paginationList model dispatch
                ]
            ]

            footer model dispatch
        ]

