namespace Felizia.Partials

open ServerCode.Version

#if FABLE_COMPILER
open Feliz
#else
open Feliz.ViewEngine
#endif

open Felizia.Model

[<AutoOpen>]
module Scripts =
    /// Scripts and state to include in the bottom on the page.
    let scripts (model: Model) =
        let jsonState = model.Materialize ()

        Html.div [
            Html.script [ Html.rawText (sprintf "var __INIT_MODEL__ = %s" jsonState) ]

            Html.script [ prop.src "/tips.js"; prop.defer true ]
            Html.script [ prop.src (sprintf "/vendors.%s.js" Version); prop.defer true ]
            Html.script [ prop.src (sprintf "/app.%s.js" Version); prop.defer true ]
            Html.script [ prop.src (sprintf "/style.%s.js" Version); prop.async true ]
        ]
