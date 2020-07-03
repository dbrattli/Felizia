namespace Felizia

open System
open System.Collections.Generic
open Feliz
open Fable.SimpleJson

type View = Model -> Dispatch -> ReactElement
type IRouter = IDictionary<Url, View>

type Theme = {
    Name: string
    Index: View
    Single: View
    List: View
}

[<AutoOpen>]
module Extensions =
    type Model
    with
        member this.Serialize () = String.Empty
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
