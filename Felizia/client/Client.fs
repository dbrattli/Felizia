namespace Felizia

open System

open Elmish
open Fable.Core.JsInterop
open Fable.SimpleJson
open Feliz
open Feliz.Router
open Fetch

open Felizia
open Felizia.Common

module Client =
    let init () : Model*Cmd<_> =
        // was the page rendered server-side?
        let stateJson: string option = !!Browser.Dom.window?__INIT_MODEL__
        let model = Model.Dematerialize stateJson

        model, Cmd.none

    let fetch<'a> (lang: string) (url: Url) (props: RequestProperties list) =
        promise {
            let init: RequestProperties list = List.append props [ requestHeaders [ Accept "application/json"; AcceptLanguage lang ] ]
            let uri = "/" +/ String.Join("/", url)
            let! response = GlobalFetch.fetch(RequestInfo.Url uri, Fetch.requestProps init)
            let! body = response.text ()
            return
                body
                |> SimpleJson.parseNative
                |> SimpleJson.mapKeys upcase
                |> Json.convertFromJsonAs<ContentResponse>
        }

    let update (msg: Msg) (currentModel: Model) : Model * Cmd<_> =
        match msg with
        | Custom "ToggleBurger" ->
            let extra =
                if currentModel.Extra.ContainsKey "burger"
                then currentModel.Extra.Remove "burger"
                else currentModel.Extra.Add ("burger", "true")
            { currentModel with Extra = extra }, Cmd.none
        | Custom _ -> currentModel, Cmd.none
        | PageNavigation url ->
            currentModel, Router.navigatePath(List.append (currentModel.CurrentSite.BaseSegments ()) url |> Array.ofList)
        | UrlChanged url ->
            printfn "UrlChanged: %A" (url.ToString ())
            let site = currentModel.CurrentSite
            let segments =
                let ba = site.BaseSegments ()
                //printfn "base segments %A" ba
                let xs = url

                // FIXME: remove base segments from URL.
                xs

            printfn "Segments: %A" segments
            let length = List.length segments
            // Remove pagniation from URL.
            let path, pageNum =
                if length >= 2 && segments.[length - 2] = currentModel.CurrentSite.PaginatePath
                then
                    let url = segments |> List.truncate (length - 2)
                    url, int segments.[length - 1]
                else
                    segments, 1
            let result = Model.GetPage path currentModel.CurrentSite.Home
            match result with
            | Some page ->
                let paginator =
                    if page.IsPage then None
                    else Some <| Paginator(page.Pages, site.Paginate, site.PaginatePath, pageNum, path)

                let cmd, loading =
                    if page.IsPage && page.Content.IsNone then
                        Cmd.ofMsg (LoadContent path), true
                    else Cmd.none, false

                { currentModel with CurrentPage = { page with Paginator = paginator }; CurrentUrl = path; PageNumber = pageNum; Loading = loading }, cmd
            | None ->
                printfn "No page found for URL: %A" url
                { currentModel with CurrentUrl = path }, Cmd.none
        | LoadContent url ->
            let lang = currentModel.Language
            let req = fetch lang url
            let props = [ requestHeaders [ AcceptLanguage lang ] ]
            { currentModel with CurrentUrl = url; Loading=true}, Cmd.OfPromise.perform req props ContentLoaded
        | ContentLoaded rsp ->
            let page : Page = { currentModel.CurrentPage with Content = Some rsp.Content }
            { currentModel with CurrentPage = page; Loading=false }, Cmd.none
        | SetLanguage lang ->
            let model = currentModel.SetLanguage lang
            let url =
                let segments = model.CurrentUrl
                if Seq.length segments > 0 && segments.[0] = lang then segments
                elif lang <> model.CurrentSite.DefaultContentLanguage then List.append [ lang ] (List.ofSeq segments)
                else segments
            model, Router.navigatePath(url |> Array.ofList)

