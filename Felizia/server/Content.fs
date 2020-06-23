module Felizia.Content

open System
open System.IO
open System.Threading.Tasks

open FSharp.Control.Tasks.V2
open Feliz.ViewEngine
open Giraffe
open Microsoft.AspNetCore.Http

open Felizia
open Felizia.Model
open Felizia.Common

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
let html (templates: IRouter) (singlePage: View) (listPage: View) (model: obj) : HttpHandler = fun next ctx ->
    let templates = ctx.GetService<IRouter>()
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
                //Log.Debug "Did not find template for {url}, using defaults"
                if currentPage.IsPage
                then singlePage
                else listPage

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


let renderPaged (model: Model) (paginationPath: string) (pageNumber: int) (segments: string list): HttpHandler = fun next ctx ->
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

let renderPage (model: Model) (segments: string list): HttpHandler =
    renderPaged model "" 1 segments

let felizia (model: Model) =
    let sites = model.Sites

    let content site lang = choose [
        let model = { model with CurrentSite = site; Language = lang }

        routex "(/?)" >=> renderPage model []
        routef "/%s" (fun page -> renderPage model [ page ])
        routef "/%s/" (fun page -> renderPage model [ page ])
        routef "/%s/%i" (fun (paginationPath, pageNumber) -> renderPaged model paginationPath pageNumber [])
        routef "/%s/%s" (fun (section, page) -> renderPage model [ section; page ])
        routef "/%s/%s/" (fun (section, page) -> renderPage model [ section; page ])
        routef "/%s/%s/%i" (fun (section, paginationPath, pageNumber) -> renderPaged model paginationPath pageNumber [ section ])
        routef "/%s/%s/%s" (fun (section, subsection, page) -> renderPage model [ section; subsection; page ])
        routef "/%s/%s/%s/" (fun (section, subsection, page) -> renderPage model [ section; subsection; page ])
        routef "/%s/%s/%s/%i" (fun (section, subsection, paginationPath, pageNumber) -> renderPaged model paginationPath pageNumber [ section; subsection ])
    ]

    // Add site for each specific language, i.e '/nb', '/en'
    for site in sites do
        let basePath = Uri site.BaseUrl
        subRoute (basePath.AbsolutePath +/ site.Language.BaseUrl) (content site (site.Language.Lang))

    // Add site for default language , i.e ''
    let defaultSite = sites |> List.find (fun site -> site.Language.Lang = site.DefaultContentLanguage)
    content defaultSite defaultSite.DefaultContentLanguage
