namespace Felizia.Layouts

open Feliz.ViewEngine

open Felizia.Model
open Felizia.Partials

module Index =

    // Index HTML rendered by server (SSR)
    let index (model: Model) dispatch =
        Html.html [
            head model

            Html.body [
                Html.div [
                    prop.id "feliz-app"
                    prop.children [
                        index model dispatch
                    ]
                ]

                scripts model
            ]
        ]

