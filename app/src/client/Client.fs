module Felizia.App

open Elmish

open Feliz
open Feliz.Router

type Model = {
    Felizia: Felizia.Model
    // Add additional application specific state here
}

type Msg =
    | FeliziaMsg of Felizia.Msg
    // Add additional application specific messages here

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
let init () : Model*Cmd<_> =
    let feliziaModel, cmd = Felizia.Client.init ()
    let model = {
        Felizia = feliziaModel
    }

    model, cmd |> Cmd.map FeliziaMsg

let update (msg: Msg) (currentModel: Model) : Model * Cmd<_> =
    match msg with
    | FeliziaMsg msg ->
        let model, cmd = Felizia.Client.update msg currentModel.Felizia
        { currentModel with Felizia = model }, cmd |> Cmd.map FeliziaMsg

let render (model: Model) (dispatch : Msg -> unit) =
    let currentView : ReactElement =
        let currentPage = model.Felizia.CurrentPage
        let template : View =
            match templates.TryGetValue model.Felizia.CurrentUrl with
            | true, tmpl -> tmpl
            | false, _ ->
                // Use default templates
                if currentPage.IsPage
                then Felizia.Theme.theme.Single
                else Felizia.Theme.theme.List

        template model.Felizia (FeliziaMsg >> dispatch)

    Router.router [
        Router.pathMode
        Router.onUrlChanged (UrlChanged >> FeliziaMsg >> dispatch)
        Router.application currentView
    ]

open Fable.Core.JsInterop

open Elmish.React
open Elmish.Debug
open Elmish.HMR

let withReact =
    if !!Browser.Dom.window?__INIT_MODEL__
    then Program.withReactHydrate
    else Program.withReactBatched

// App
Program.mkProgram init update render
#if DEBUG
//|> Program.withDebugger
#endif
|> Program.withConsoleTrace
|> withReact "feliz-app"
|> Program.run