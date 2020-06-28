namespace Felizia.Arctic.Layouts

open Feliz.ViewEngine

open Felizia.Model
open Felizia.Partials
open Felizia.Arctic.Partials

[<AutoOpen>]
module ListPage =
    // Server side react
    let listPage (model: Model) dispatch =
        let page = model.CurrentPage

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
                                listView model dispatch
                            ]
                        ]

                        scripts model
                    ]
                ]
            ]
        ]
