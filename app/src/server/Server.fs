open System
open System.IO

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders

open Giraffe
open Serilog
open Serilog.Events

open Felizia
open Felizia.Generate
open Felizia.Yaml

let port = 8080us

let config = {
    TemplatePath = Path.GetFullPath "../shared/"
    /// The path where HTML pages will be generated
    HtmlPath = Path.GetFullPath "../client/public/gen"
    /// The path to translation files
    I18nPath = Path.GetFullPath "../../i18n/"
    /// The path of config.yaml
    ConfigPath = Path.GetFullPath "../../"
    /// The path to site content files
    ContentPath = Path.GetFullPath "../../content/"
}

let publicPath = Path.GetFullPath "../client/public"
let staticPath = Path.GetFullPath "../../static"


let sites =
    parseSiteConfig config.ConfigPath
    // Split site from config into one site for every translation
    |> (fun site ->
        site.AllTranslations
        |> Seq.map (fun lang ->
            let trans = parseI18n config.I18nPath lang.Lang
            let site' = { site with I18n = trans }

            processContent config [] site' lang.Lang)
     )
     |> List.ofSeq

let model = { Model.Empty with Sites=sites }

let webApp =
    choose [
        Felizia.Server.route model

        //route "" >=> redirectTo false "/"
        RequestErrors.NOT_FOUND "Not Found"
    ]

let site = List.head sites // Any site will contain theme info
let theme = Generate.theme site.Theme config
Log.Information("Using theme {theme}", theme.Name)

type CustomNegotiationConfig (baseConfig : INegotiationConfig) =
    interface INegotiationConfig with

        member __.UnacceptableHandler = baseConfig.UnacceptableHandler
        member __.Rules =
                dict [
                    "application/json", Felizia.Server.json
                    "text/html"       , (Felizia.Server.html templates theme)
                    "*/*"             , (Felizia.Server.html templates theme)
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
        .AddSingleton<FeliziaConfig>(config)
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