module ServerCode.Pages

open System
open System.Collections.Generic
open System.IO

open FSharp.Markdown

open Thoth.Json.Net

open Giraffe
open FSharp.Control.Tasks.V2
open Serilog

open Feliz.ViewEngine
open Legivel.Serialization

open Shared
open Shared.Home
open Shared.Model

type FileInfo = {
    FileName: string
    Text: string option
    FrontMatter: FrontMatter option
}


let markdownPath = Path.GetFullPath "../../content/"
let htmlPath = Path.GetFullPath "../Client/public/gen"

let sortedPages = Dictionary<string, string> ()


let handlePage (msg : Msg) : Async<(Msg) option> = async {
    match msg with
    (*
    | PageNavigation (Pages.Page page, update) ->
        let fileName = Path.GetFileName page

        let pathName = Path.Combine(htmlPath, "pages", fileName + ".html")
        let! html = File.ReadAllTextAsync pathName |> Async.AwaitTask
        let msg' = PageMsg (MarkdownPage.Msg.Page (page, html))
        return Some msg'
    *)
    | _ ->
        return None
}

(*
let page: HttpHandler = fun _ ctx ->
    let path = ctx.Request.Path.Value
    let fileName = Path.GetFileName path
    task {
        let pathName = Path.Combine(htmlPath, "pages", fileName + ".html")
        let! html = File.ReadAllTextAsync pathName

        let currentModel = MarkdownPage.Model.init ()
        let pages = [(fileName, html)]

        let model: Model = {
            CurrentPage = Pages.Page fileName
            Burger = false
            Site = { Title=""; PermaLink= Uri ""; IsHome = true; Params = Map.empty }
            Pages = List.Empty
        }
        return! ctx.WriteHtmlStringAsync (Templates.index (Some model) |> Render.htmlDocument)
    }
*)

let frontMatter (source: string) =
    let partitions = source.TrimStart().Split([| "\r\n---\r\n"; "\n---\n"; "\r---\r" |], StringSplitOptions.RemoveEmptyEntries) |> List.ofSeq
    match partitions with
    | [ ft ] -> ft.Trim('-'), None
    | fm :: md -> String.Concat(fm.TrimStart('-'), "\n"), String.concat "\n---\n" md |> Some
    | _ -> Environment.NewLine, Some source

let generateHtml (files: FileInfo list) genPath =
    files |> List.iter (fun file ->
        let pathName = Path.Combine(genPath, file.FileName + ".html")
        match file.Text with
        | Some text ->
            let html =
                text |> FSharp.Markdown.Markdown.TransformHtml

            let bulma =
                html
                    .Replace("<h1>", "<h1 class=\"title is-2\">")
                    .Replace("<h2>", "<h2 class=\"subtitle is-2\">")
                    //.Replace("<p>", "<p class=\"content\">")
                    .Replace("<ul>", "<ul class=\"content\">")

            printfn "Writing HTML file to: %s" pathName
            File.WriteAllText(pathName, bulma)
        | _ -> ()
    )

let getSummary (document: string option) =
    match document with
    | Some text ->
        let md = Markdown.Parse text

        let firstParagraph =
            md.Paragraphs
            |> List.choose (fun par ->
                match par with
                | Paragraph(body=[Literal(text=text)]) -> Some text
                | _ -> None)
            |> List.tryHead |> Option.defaultValue ""

        // Take 70 first words (TODO: make configurable)
        firstParagraph.Split " "
        |> Seq.truncate 70
        |> String.concat " "
        |> Some
    | _ -> None

/// Processes Markdown pages and generates cards.json and a corresponding html
/// file for every markdown file
let rec processContent (contentPath: string) (genPath: string) (root: bool) : Page =
    let files =
        System.IO.Directory.GetFiles(contentPath, "*.md") |> List.ofArray
        |> List.map (fun file ->
            let fileName = Path.GetFileName file
            let text = File.ReadAllText file
            let fm, md = frontMatter text
            let fm' =
                match Deserialize<FrontMatter> fm with
                | [ Success info ]  -> Some info.Data
                | [ Error err ] -> failwith <| sprintf "Unable to parse front-matter: %s" (err.ToString())
                | _ -> None

            printfn "%A" fm'
            { FileName = fileName; Text=md; FrontMatter=fm' }
        )

    let dirs = System.IO.Directory.GetDirectories(contentPath) |> List.ofArray
    Directory.CreateDirectory genPath |> ignore

    let pages : Page list =
        files
        |> List.map (fun fileInfo ->
            Log.Information("Processing {file}", fileInfo.FileName)

            let summary = getSummary fileInfo.Text
            let baseFileName = Path.GetFileNameWithoutExtension fileInfo.FileName
            let file : File = { LogicalName=fileInfo.FileName; Path=contentPath; BaseFileName=baseFileName }
            Page { FrontMatter=fileInfo.FrontMatter; Summary=summary; Url=Uri "http://test"; File=file }
        )

    let sections : Page list =
        dirs
        |> List.map (fun dir ->
            let dirName = DirectoryInfo(dir).Name;
            let cPath = Path.Combine (contentPath, dirName)
            let gPath = Path.Combine (genPath, dirName)

            processContent cPath gPath false
        )

    do generateHtml files genPath

    let index =
        pages
        |> List.choose (fun page ->
            match page with
            | Page page -> if page.File.LogicalName = "index.md" then Some page else None
            | _ -> None
        )
        |> List.tryExactlyOne
    let fm = index |> Option.bind (fun page -> page.FrontMatter)
    let summary = index |> Option.bind (fun page -> page.Summary)

    { FrontMatter=fm; Summary=summary; Url=Uri "http://test"; Pages = List.append pages sections }
    |> match root with | true -> Home | false -> Section
