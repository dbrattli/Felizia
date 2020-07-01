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

open Feliz.ViewEngine

open Felizia
open Felizia.Common
open Felizia.Generate
open Felizia.Yaml
open Felizia.Content
open Felizia.Arctic


let publicPath = Path.GetFullPath "../Client/public"
let staticPath = Path.GetFullPath "../../static"
let tmplPath = Path.GetFullPath "../shared/"

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
        Content.felizia model

        //route "" >=> redirectTo false "/"
        RequestErrors.NOT_FOUND "Not Found"
    ]

let theme = Theme.set ((List.head sites).Theme) tmplPath
Log.Information("Using theme {theme}", theme.Name)

type CustomNegotiationConfig (baseConfig : INegotiationConfig) =
    interface INegotiationConfig with

        member __.UnacceptableHandler = baseConfig.UnacceptableHandler
        member __.Rules =
                dict [
                    "application/json", Content.json
                    "text/html"       , (Content.html templates theme.SinglePage theme.ListPage)
                    "*/*"             , (Content.html templates theme.SinglePage theme.ListPage)
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
        .AddSingleton<IRouter>(templates)
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