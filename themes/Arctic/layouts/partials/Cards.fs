namespace Felizia.Arctic.Partials

open System

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
#endif

open Felizia

[<AutoOpen>]
module Cards =
    /// Index application rendered by client
    let cards (model: Model) dispatch : ReactElement =
        let page = model.CurrentPage

        Bulma.container [
            Bulma.columns [
                columns.isMultiline
                prop.children [
                    for page in page.Paginator.Value.Pages do
                        card page model dispatch
                ]
            ]
        ]
