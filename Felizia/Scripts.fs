namespace Felizia.Partials

open Feliz.ViewEngine

open Felizia

[<AutoOpen>]
module Scripts =
    /// Scripts and state to include in the bottom on the page.
    let scripts (model: Model) =
        let jsonState = model.Materialize ()

        Html.div [
            Html.script [ Html.rawText (sprintf "var __INIT_MODEL__ = %s" jsonState) ]

            Html.script [ prop.src "/tips.js"; prop.defer true ]
            Html.script [ prop.src (sprintf "/vendors.%s.js" model.Version); prop.defer true ]
            Html.script [ prop.src (sprintf "/app.%s.js" model.Version); prop.defer true ]
            Html.script [ prop.src (sprintf "/style.%s.js" model.Version); prop.async true ]
        ]
