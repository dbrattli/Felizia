namespace Felizia.Arctic.Partials

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
#endif

open Felizia.Model

[<AutoOpen>]
module Content =
    type ModelProps = { Model: Model }

    // Server side react
    let content (model: Model) dispatch =
        let page = model.CurrentPage

        let content = React.functionComponent(fun (props: ModelProps) ->
            let page = model.CurrentPage

            Html.div [
                match model.Loading, page.Content with
                | _, Some content ->
                    prop.dangerouslySetInnerHTML content
                | true, None ->
                    prop.text "Loading content"
                | _ ->
                    ()
            ]
        )

        Bulma.container [
            Bulma.title.p page.Title

            Bulma.content [
                content { Model = model }
            ]
        ]
