namespace Felizia.Partials

open System
open ServerCode.Version

open Feliz.ViewEngine
open Felizia.Model
open Felizia.Common

[<AutoOpen>]
module Head =

    let head (model: Model) =
        let page = model.CurrentPage
        let site = model.CurrentSite

        Html.head [
            Html.title [ prop.text (if page.Title <> String.Empty then page.Title else site.Title) ]

            Html.meta [ prop.charset.utf8 ]
            Html.meta [
                prop.name "author"
                prop.content (site.Params.Author.DisplayName$value)
            ]
            Html.meta [
                prop.name "description"
                prop.content (if page.IsHome then site.Params.Description$value else page.Description$value)
            ]

            if not site.Params.Keywords.IsEmpty then
                Html.meta [ prop.name "keywords"; prop.content (String.Join(",", site.Params.Keywords)) ]

            Html.meta [ prop.httpEquiv.contentType; prop.content "text/html"; prop.charset.utf8 ]
            Html.meta [ prop.name "viewport"; prop.content "width=device-width, initial-scale=1" ]

            Html.meta [ prop.custom ("http-equiv", "Cache-Control"); prop.content "no-cache, no-store, must-revalidate" ]
            Html.meta [ prop.custom ("http-equiv", "Pragma"); prop.content "no-cache" ]
            Html.meta [ prop.custom ("http-equiv", "Expires"); prop.content "0" ]

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

            if site.Params.Literate then Html.link [ prop.rel "stylesheet"; prop.href "/style.css" ]

            // Favicons
            Html.link [ prop.rel "shortcut icon"; prop.type' "image/png"; prop.href "/img/safe_favicon.png" ]
            Html.script [ prop.src (sprintf "/style.%s.js" Version) ]
        ]

(*
    {{ template "_internal/opengraph.html" . }}
    {{ template "_internal/google_analytics_async.html" . }}

    <!-- RSS -->
    {{- with .OutputFormats.Get "RSS" }}
      {{ printf `<link rel="%s" type="%s" href="%s" title="%s" />` .Rel .MediaType.Type .Permalink $.Site.Title | safeHTML }}
    {{- end }}

    <!-- Favicons -->
    {{ if (fileExists (print "/static" .Site.Data.defaults.icons.appleicon)) -}}
    <link rel="apple-touch-icon" href="{{ .Site.Data.defaults.icons.appleicon }}"/>
    {{ end -}}
    {{ if (fileExists (print "/static" .Site.Data.defaults.icons.favicon)) -}}
    <link rel="icon" href="{{ .Site.Data.defaults.icons.favicon }}"/>
    {{ end -}}
  </head>
*)