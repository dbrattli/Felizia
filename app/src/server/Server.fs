open System
open System.IO

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders

open FSharp.Core.Printf
open Giraffe
open Serilog
open Serilog.Events

open Felizia
open Felizia.Generate
open Felizia.Yaml
open Felizia.Model
open Felizia.Common
open Felizia.Content


let publicPath = Path.GetFullPath "../Client/public"
let staticPath = Path.GetFullPath "../../static"

let port = 8080us
let configPath = Path.GetFullPath "../../"
let i18nPath = Path.GetFullPath "../../i18n/"
let markdownPath = Path.GetFullPath "../../content/"



let sites =
    parseSiteConfig configPath
    // Split site from config into one site for every translation
    |> (fun site ->
        site.AllTranslations
        |> Seq.map (fun lang ->
            let trans = parseI18n i18nPath lang.Lang
            let site' = { site with I18n = trans }

            processContent markdownPath htmlPath [] site' lang.Lang)
     )
     |> List.ofSeq

let model = { Model.Empty with Sites=sites }

let webApp =
    choose [
        let content site lang = choose [
            let model = { model with CurrentSite = site; Language = lang }

            routex "(/?)" >=> Content.page model []
            routef "/%s" (fun page -> Content.page model [ page ])
            routef "/%s/" (fun page -> Content.page model [ page ])
            routef "/%s/%i" (fun (paginationPath, pageNumber) -> Content.paged model paginationPath pageNumber [])
            routef "/%s/%s" (fun (section, page) -> Content.page model [ section; page ])
            routef "/%s/%s/" (fun (section, page) -> Content.page model [ section; page ])
            routef "/%s/%s/%i" (fun (section, paginationPath, pageNumber) -> Content.paged model paginationPath pageNumber [ section ])
            routef "/%s/%s/%s" (fun (section, subsection, page) -> Content.page model [ section; subsection; page ])
            routef "/%s/%s/%s/" (fun (section, subsection, page) -> Content.page model [ section; subsection; page ])
            routef "/%s/%s/%s/%i" (fun (section, subsection, paginationPath, pageNumber) -> Content.paged model paginationPath pageNumber [ section; subsection ])
        ]

        // Add site for each specific language, i.e '/nb', '/en'
        for site in sites do
            let basePath = Uri site.BaseUrl
            printfn "basePath: %A" basePath.AbsolutePath
            subRoute (basePath.AbsolutePath +/ site.Language.BaseUrl) (content site (site.Language.Lang))

        // Add site for default language , i.e ''
        let defaultSite = sites |> List.find (fun site -> site.Language.Lang = site.DefaultContentLanguage)
        content defaultSite defaultSite.DefaultContentLanguage

        //route "" >=> redirectTo false "/"
        RequestErrors.NOT_FOUND "Not Found"
    ]

type CustomNegotiationConfig (baseConfig : INegotiationConfig) =
    interface INegotiationConfig with

        member __.UnacceptableHandler = baseConfig.UnacceptableHandler
        member __.Rules =
                dict [
                    "application/json", Content.json
                    "text/html"       , Content.html
                    "*/*"             , Content.html
                ]

let configureApp (app : IApplicationBuilder) =
    let options =
        let path = Path.Combine(Directory.GetCurrentDirectory(), publicPath)
        StaticFileOptions(
            FileProvider = new PhysicalFileProvider(path),
            RequestPath = (PathString "")
        )
    app
       .UseResponseCompression()
       .UseStaticFiles()
       .UseStaticFiles(options)
       .UseGiraffe webApp
    ()


let configureServices (services : IServiceCollection) =
    services
        .AddGiraffe()
        .AddResponseCompression()
        .AddSingleton<INegotiationConfig>(CustomNegotiationConfig(DefaultNegotiationConfig()))
        |> ignore
    ()

Log.Logger <-
    LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .WriteTo.ColoredConsole(
             LogEventLevel.Verbose,
            "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}"
        )
        .CreateLogger()

WebHost
    .CreateDefaultBuilder()
    .UseWebRoot(publicPath)
    .UseContentRoot(publicPath)
    .Configure(Action<IApplicationBuilder> configureApp)
    .ConfigureServices(configureServices)
    .UseUrls("http://0.0.0.0:" + port.ToString () + "/")
    .UseSerilog()
    .Build()
    .Run()