module Felizia.Content

open System
open System.IO
open System.Threading.Tasks

open FSharp.Control.Tasks.V2
open Feliz.ViewEngine
open Giraffe
open Microsoft.AspNetCore.Http
open Serilog

open Felizia
open Felizia.Model
open Felizia.Generate

let htmlPath = Path.GetFullPath "../client/public/gen"

/// Render output as JSON. Only Url with Content.
let json (model: obj) : HttpHandler = fun next ctx ->
    match model with
    | :? Model as model ->
        let currentPage = model.CurrentPage
        let content = currentPage.Content |> Option.defaultValue ""
        let segments = currentPage.Url
        ctx.WriteJsonAsync { Url=segments; Content=content }
    | _ -> failwith "Missing model"

/// Render output as HTML
let html (model: obj) : HttpHandler = fun next ctx ->
    match model with
    | :? Model as model ->
        let currentPage = model.CurrentPage
        printfn "currentPage.Url: %A" currentPage.Url
        printfn "template: %A" templates
        let template =
            let segments = currentPage.Url
            let url =
                // Remove default language from URL.
                if (not << List.isEmpty) segments  && List.head segments = model.Language then
                    segments
                    |> List.skip 1
                else
                    segments

            match templates.TryGetValue url with
            | true, tmpl -> tmpl
            | false, _ ->
                Log.Debug "Did not find template for {url}, using defaults"
                if currentPage.IsPage
                then Layouts.SinglePage.singlePage
                else Layouts.ListPage.listPage

        ctx.WriteHtmlStringAsync (template model ignore |> Render.htmlDocument)
    | _ -> ctx.WriteStringAsync "Not found"


let getLanguage (model: Model) (site: Site) (ctx: HttpContext) =
    let hdr = ctx.GetRequestHeader "Accept-Language"

    model.Language
(*
    match model.Language, hdr with
    | Some lang, _ -> lang
    | None, Ok lang ->
        let browserLangs = lang.Split(",") |> Seq.map (fun lang -> lang.Split(";") |> (fun xs -> xs.[0])) |> List.ofSeq
        let languages = model.Sites |> List.map (fun lang -> lang.Language.Lang)
        match List.tryFind (fun lang -> List.contains lang languages) browserLangs with
        | Some lang -> lang
        | None -> site.DefaultContentLanguage
    | _ -> site.DefaultContentLanguage
*)

let paged (model: Model) (paginationPath: string) (pageNumber: int) (segments: string list): HttpHandler = fun next ctx ->
    let site = model.CurrentSite
    let language = getLanguage model site ctx
    let url, lang =
        if language = site.DefaultContentLanguage
        then segments, ""
        else List.append [ language ] segments, language

    task {
        let fileName = "index.html"
        let pathName = Path.Combine(htmlPath, Path.Combine(segments |> Array.ofList), fileName)

        let! html =
            if File.Exists pathName
            then File.ReadAllTextAsync pathName
            else Task.FromResult String.Empty

        let model' = model.SetLanguage language

        let page = Model.GetPage url model'.CurrentSite.Home
        match page with
        | Some page ->
            let paginator =
                if page.IsPage then None
                else Paginator(page.Pages, site.Paginate, site.PaginatePath, pageNumber, url) |> Some
            let model'' = {
               model' with
                    CurrentPage = { page with Content = Some html; Paginator = paginator }
                    CurrentUrl = url
                    PageNumber = pageNumber
            }

            return! Successful.OK model'' next ctx
        | None ->
            printfn "Did not find page: %A" url
            return! next ctx
    }

let page (model: Model) (segments: string list): HttpHandler =
    paged model "" 1 segments
