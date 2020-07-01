module Felizia.Generate

open System
open System.Globalization
open System.IO

open FSharp.Markdown
open FSharp.Literate

open Felizia
open Felizia.Common
open Felizia.Yaml

type FileInfo = {
    FileName: string
    FileNameWithoutLocale: string
    Locale: string
    Content: string option
    FrontMatter: YamlPage option
}

let generateHtml (site: Site) (root: string) (path: string) (files: FileInfo list) =
    files |> List.iter (fun file ->
        //printfn "fileInfo: %A" (file.FileName, file.Locale)

        let locale = file.Locale
        let fileName = file.FileNameWithoutLocale
        let pathName = Path.Combine(root, locale, path)

        match file.Content with
        | Some content ->
            let html =
                match site.Params.Literate with
                | true ->
                    content
                    |> Literate.ParseMarkdownString
                    |> (fun doc -> Literate.WriteHtml(doc, lineNumbers=true))
                | _ ->
                    content
                    |> FSharp.Markdown.Markdown.TransformHtml

            let bulmify =
                html
                    .Replace("<h1>", "<h1 class=\"title is-2\">")
                    .Replace("<h2>", "<h2 class=\"subtitle is-2\">")
                    .Replace("<ul>", "<ul class=\"content\">")

            let fileName =
                if file.FileName.StartsWith "index." || file.FileName.StartsWith "_index."
                then
                    do Directory.CreateDirectory pathName |> ignore
                    Path.Join(pathName, "index.html")
                else
                    let path = Path.Join(pathName, fileName)
                    do Directory.CreateDirectory path |> ignore
                    Path.Join(path, "index.html")

            printfn "Writing HTML file to: %s" fileName
            File.WriteAllText(fileName, bulmify)
        | _ ->
            printfn "Skipping file without content: %s" fileName
            ()
    )

let getSummary (site: Site) (document: string option) =
    let space = " "
    match document with
    | Some text ->
        let md = Markdown.Parse text

        md.Paragraphs
        |> Seq.choose (function | Paragraph(body=[Literal(text=text)]) -> Some text | _ -> None)
        |> Seq.collect (fun p -> p.Split space)
        |> Seq.truncate site.SummaryLength
        |> String.concat space
        |> Some
    | _ -> None

/// Processes Markdown pages and generates cards.json and a corresponding html
/// file for every markdown file
let rec processContent (contentPath: string) (genPath: string) (segments: string list) (site: Site) (locale: string) : Site =
    // Get all files in this folder
    let files =
        let pattern = "*.md"
        System.IO.Directory.GetFiles(contentPath, pattern) |> List.ofArray
        |> List.map (fun file ->
            let fileName = Path.GetFileName file
            let nameWithoutExtension = Path.GetFileNameWithoutExtension fileName
            let nameWithoutLocale = Path.GetFileNameWithoutExtension nameWithoutExtension
            let fileLocale = Path.GetExtension nameWithoutExtension |> (fun name -> name.Trim('.'))

            let text = File.ReadAllText file
            let yaml, md = Yaml.parseContent text
            let fm = Yaml.YamlPage.Deserialize yaml

            { FileName = fileName; Content=md; FrontMatter=fm; Locale=fileLocale; FileNameWithoutLocale = nameWithoutLocale }
        )

    // Recursively get all sections in this folder.
    let sections : Page list =
        let dirs = System.IO.Directory.GetDirectories(contentPath) |> List.ofArray
        dirs
        |> List.map (fun dir ->
            let dirName = DirectoryInfo(dir).Name
            let contentPath = Path.Combine (contentPath, dirName)

            processContent contentPath genPath (dirName :: segments) site locale
            |> (fun site -> site.Home)
        )

    // Generate menues from sections
    let menues =
        sections
        |> List.filter (fun section -> (not << List.isEmpty) section.Pages) // Hide empty menues
        |> List.map (fun section ->
            {
                URL = "/" +/ (String.Join("/", section.Url))
                Url = section.Url
                Name = section.Title
                Weight = section.Weight
            }
        )
        |> List.sortBy (fun m -> m.Weight)

    let site' = { site with Menus = menues }

    // Convert files to pages (some files are sections)
    let pages : Page list =
        files
        |> List.map (fun fileInfo ->
            //Log.Information("Processing {file}", fileInfo.FileName)

            let page =
                match fileInfo.FrontMatter with
                | Some fm -> fm.ToModel ()
                | None -> Page.Empty ()

            let summary = getSummary site' fileInfo.Content
            let baseFileName = fileInfo.FileNameWithoutLocale
            let file = { LogicalName=fileInfo.FileName; Path=contentPath; BaseFileName=baseFileName } |> Some
            let isPage = not (fileInfo.FileName.StartsWith "index." || fileInfo.FileName.StartsWith "_index.")
            let weight = fileInfo.FrontMatter |> Option.bind (fun p -> p.Weight) |> Option.defaultValue 0
            let title =
                if page.Title = String.Empty && isPage
                then CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileInfo.FileName.ToLower())
                else page.Title

            let language = List.tryFind (fun lang -> lang.Lang=fileInfo.Locale) site.AllTranslations
            let lang = language |> Option.map (fun lang -> lang.Lang) |> Option.defaultValue ""

            let relLangUrl =
                [
                    if language.IsSome && lang <> site.DefaultContentLanguage then
                        lang
                    yield! segments
                    if isPage then
                        baseFileName
                ]

            let permaLink = site.BaseUrl +/ String.Join("/", relLangUrl)

            { page with
                Summary = summary
                Title = title
                PermaLink = permaLink
                Url = relLangUrl
                File = file
                Language = language |> Option.defaultValue page.Language
                IsPage = isPage
                Weight = weight
            }
        )
        |> List.filter (fun p -> p.Language.Lang = locale)
        |> List.sortBy (fun p -> p.Weight)

    do generateHtml site genPath (Path.Join(segments |> Array.ofList)) files

    let sections', pages' =
        pages
        |> List.partition (fun page -> not page.IsPage)

    let index' = if Seq.isEmpty sections' then Page.Empty () else sections' |> Seq.head
    let subtree =
        List.append pages' sections
    let title =
        if index'.Title = String.Empty
        then
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase((segments |> List.tryLast)
                |> Option.map (fun title -> title.ToLower())
            |> Option.defaultValue site.Title)
        else index'.Title

    let url =
        [
            if locale <> site.DefaultContentLanguage then
                locale
            yield! segments
        ]

    let home =
        { index' with
            IsHome = List.isEmpty segments
            Title = title
            Url = url
            IsPage = false
            Pages = subtree
            Paginator = None //Some { Pages = List.collect (fun s -> List.filter (fun p -> p.IsPage) (s :: s.Pages) ) subtree }
        }
    let language = List.tryFind (fun lang -> lang.Lang = locale) site.AllTranslations |> Option.defaultValue site.Language
    { site' with Home = home; Language = language; BaseUrl = site'.BaseUrl }
