namespace Felizia.Arctic.Layouts

open Feliz.ViewEngine

open Felizia
open Felizia.Partials
open Felizia.Arctic.Partials

[<AutoOpen>]
module SinglePage =
    // Server side react
    let singlePage (model: Model) dispatch =

        Html.html [
            prop.custom ("itemtype", "http://schema.org/Article")
            prop.children [
                head model

                Html.body [
                    prop.style [ style.display.flex; style.flexDirection.column; ] // style.minHeight "100uv" ]

                    prop.children [
                        Html.div [
                            prop.id "feliz-app"
                            prop.children [
                                singleView model dispatch
                            ]
                        ]

                        scripts model
                    ]
                ]
            ]
        ]
