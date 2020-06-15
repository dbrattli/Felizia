namespace Felizia.Model

open Feliz
open Fable.SimpleJson

[<AutoOpen>]
module Extensions =
    type Model with
        static member Dematerialize (stateJson: string option) : Model =
            match stateJson with
            | Some json ->
                let model =
                    json
                    |> SimpleJson.parseNative
                    |> Json.convertFromJsonAs<Model>

                // Enrich the model
                model.SetLanguage model.Language

            | None -> Model.Empty // no SSR -> show home page using empty initial model
