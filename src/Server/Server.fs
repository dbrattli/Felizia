open System
open System.IO

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection

open Reaction.AspNetCore.Middleware
open FSharp.Control
open FSharp.Control.Tasks.V2
open Giraffe
open Serilog
open Serilog.Events
open Thoth.Json.Net
open Thoth.Json.Giraffe

open Shared
open Shared.Model
open Shared.Json

open ServerCode
open ServerCode.Pages
open Giraffe.Serialization

let publicPath = Path.GetFullPath "../Client/public"

let port = 8080us

let webApp =
    choose [
        GET >=> choose [
            route PageUrls.Home

            routeStartsWith PageUrls.Activity

            routeStartsWith PageUrls.Page

            routef "%s" (fun x -> redirectTo false "/")
        ]
        POST >=>
            route "/api/msg" >=>
                fun next ctx ->
                    task {
                        let! msg = ctx.BindModelAsync<Msg> ()
                        let! msg' = handlePage msg
                        match msg' with
                        | Some msg ->
                            return! json msg next ctx
                        | None ->
                            return! Successful.NO_CONTENT next ctx
                    }
    ]

let stream (connectionId: ConnectionId) (msgs: IAsyncObservable<Msg*ConnectionId>) : IAsyncObservable<Msg*ConnectionId> =
    msgs
    |> AsyncRx.filter (fun (msg, id) -> id = connectionId)
    //|> AsyncRx.tapOnNext (printfn "Server got: %A")
    //|> AsyncRx.chooseAsync handlePage
    //|> AsyncRx.tapOnNext (printfn "Server sent: %A")

let decoder (input: string) : Msg option =
    let ret = Decode.fromString Msg.Decoder input
    match ret with
    | Error _ -> None
    | Ok value -> Some value

let configureApp (app : IApplicationBuilder) =
    app.UseWebSockets()
       .UseStream<Msg>(fun options ->
       { options with
           Stream = stream
           Encode = (fun msg -> Encode.toString 0 (Msg.Encoder msg))
           Decode = decoder
       })
       .UseHttpsRedirection()
       .UseStaticFiles()
       .UseGiraffe webApp

    let home = processContent markdownPath htmlPath true
    printfn "%A" home
    ()

let configureServices (services : IServiceCollection) =
    let extraCoders =
        Extra.empty
        |> Extra.withCustom Msg.Encoder Msg.Decoder

    services
        .AddGiraffe()
        .AddSingleton<IJsonSerializer>(ThothSerializer (extra=extraCoders))
        .AddAntiforgery(Action<_> (fun o -> o.SuppressXFrameOptionsHeader <- true))
        |> ignore

Log.Logger <-
    LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel.Debug()
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