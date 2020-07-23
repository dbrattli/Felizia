namespace Felizia.Partials

#if FABLE_COMPILER
open Feliz
#else
open Feliz.ViewEngine
#endif

open Felizia

[<AutoOpen>]
module Scripts =
    /// Scripts and state to include in the bottom on the page.
    let scripts (model: Model) =
        let jsonState = model.Serialize ()

        Html.div [
            Html.script [ Html.rawText (sprintf "var __INIT_MODEL__ = %s" jsonState) ]

            if model.CurrentSite.Params.Literate then
                Html.script [ prop.src "/tips.js"; prop.defer true ]

            Html.script [ prop.src (sprintf "/js/vendors.%s.js" model.Version); prop.defer true ]
            Html.script [ prop.src (sprintf "/js/app.%s.js" model.Version); prop.defer true ]
            Html.script [ prop.src (sprintf "/js/style.%s.js" model.Version); prop.async true ]
        ]
