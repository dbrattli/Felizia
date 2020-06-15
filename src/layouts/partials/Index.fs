namespace Felizia.Partials

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
open Feliz.Bulma.Operators
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
open Feliz.Bulma.ViewEngine.Operators
#endif

open Felizia.Model

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

