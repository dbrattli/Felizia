namespace Felizia

open System.Collections.Generic

open Fable.SimpleJson
open Feliz

type View = Model -> Dispatch -> ReactElement
type IRouter = IDictionary<Url, View>

type Theme = {
    Name: string
    Index: View
    Single: View
    List: View
}

/// Client extensions
[<AutoOpen>]
module Extensions =
    type Model
    with
        static member Deserialize (stateJson: string option) : Model =
            match stateJson with
            | Some json ->
                let model =
                    json
                    |> SimpleJson.parseNative
                    |> Json.convertFromJsonAs<Model>

                // Enrich the model
                model.SetLanguage model.Language

            | None -> Model.Empty // no SSR -> show home page using empty initial model
