namespace Felizia.Arctic.Partials

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
open Feliz.Bulma.Operators
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
open Feliz.Bulma.ViewEngine.Operators
#endif

open Felizia


[<AutoOpen>]
module ListView =
    let listView (model: Model) dispatch =
        Html.div [
            Navbar.navbar color.isWhite model dispatch

            Bulma.section [
                cards model dispatch

                Bulma.container [
                    paginationList model dispatch
                ]
            ]

            footer model dispatch
        ]
