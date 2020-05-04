module ServerCode.Templates

open Shared.Model
open Shared.View

open Thoth.Json.Net
open Feliz.ViewEngine
open ServerCode.Version

// Server side react
let index (model: Model option) =
    let jsonState, htmlStr =
        match model with
        | Some model ->
            // Note we call json serialization twice here,
            // because Elmish's model can be some complicated type instead of pojo.
            // The first one will seriallize the state to a json string,
            // and the second one will seriallize the json string to a js string,
            // so we can deseriallize it by Thoth auto decoder and get the correct types.
            Encode.Auto.toString(0, (Encode.Auto.toString(0, model))),
            view model ignore
            |> Render.htmlView
        | None ->
            "null", ""

    Html.html [
        Html.head [
            Html.title [ Html.rawText "Arctic Camp Brattli" ]

            Html.meta [ prop.name "description"; prop.content "Adventures in the Arctic, bikes and hikes" ]
            Html.meta [ prop.name "keywords"; prop.content "Mountain,Bike,Adventure,Hikes,Aurora,Borealis,Tours,Sykkel,Nordlys,Nordlystur,Tur,Truge,Trugetur,Fjelltur,Fatbike,Tromsø,Arctic,Ski,Photo,Foto,Dog,Sledding,Hundeslede,Midnattsol,Midnight,Sun" ]

            Html.meta [ prop.httpEquiv.contentType; prop.content "text/html"; prop.charset.utf8 ]
            Html.meta [ prop.name "viewport"; prop.content "width=device-width, initial-scale=1" ]

            Html.link [
                prop.rel "stylesheet"
                prop.href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.5/css/bulma.min.css"
                prop.crossOrigin.anonymous
            ]
            Html.link [
                prop.rel "stylesheet"
                prop.href "https://use.fontawesome.com/releases/v5.6.1/css/all.css"
                prop.integrity "sha384-gfdkjb5BdAXd+lj+gudLWI+BXq4IuLW5IT+brZEZsLFm++aCMlF1V92rMkPaX4PP"
                prop.crossOrigin.anonymous
            ]
            Html.link [
                prop.rel "stylesheet"
                prop.href "https://fonts.googleapis.com/css?family=Open+Sans"
            ]

            Html.link [ prop.rel "shortcut icon"; prop.type' "image/png"; prop.href "/Images/safe_favicon.png" ]
            Html.script [ prop.src (sprintf "/style.%s.js" Version) ]
        ]
        Html.body [
            prop.className "app-container"
            prop.children [
                Html.div [
                    prop.id "reaction-app";
                    prop.className "elmish-app"
                    prop.children [
                        Html.rawText htmlStr
                    ]
                ]
                Html.script [ Html.rawText (sprintf "var __INIT_MODEL__ = %s" jsonState) ]

                Html.script [ prop.src (sprintf "/vendors.%s.js" Version) ]
                Html.script [ prop.src (sprintf "/app.%s.js" Version) ]
                Html.script [ prop.src (sprintf "/style.%s.js" Version) ]
            ]
        ]
    ]
