namespace Felizia.Arctic.Partials

open System

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
#endif

open Felizia.Model

[<AutoOpen>]
module Footer =

    let footer (model: Model) (dispatch: Dispatch) =
        let site = model.CurrentSite

        Bulma.footer [
            Bulma.container [
                Bulma.level [
                    Bulma.levelLeft [
                        text.hasTextCentered
                        prop.children [
                            Bulma.levelItem [
                                Html.form [
                                    prop.classes [ "control"; "has-icons"; "has-icons-right"]
                                    prop.method "get"
                                    prop.action (if site.Params.SearchUrl.IsSome then site.Params.SearchUrl.Value else "https://duckduckgo.com")
                                    prop.children [
                                        Bulma.input.search [
                                            prop.name "q"
                                            prop.maxLength 255
                                            prop.placeholder (model.T "search")

                                        ]
                                        Html.input [
                                            prop.type' "hidden"
                                            prop.name "sitesearch"
                                            prop.value (site.BaseUrl)
                                        ]
                                        Bulma.icon [
                                            icon.isSmall
                                            icon.isRight
                                            prop.children [
                                                Html.i [
                                                    prop.classes [ "fas"; "fa-search"]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    Bulma.levelRight [
                        text.hasTextCentered
                        prop.children [
                            Bulma.levelItem [
                                Bulma.button.a [
                                    prop.href site.BaseUrl
                                    prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation []))
                                    prop.children [
                                        Bulma.icon [
                                            Html.i [
                                                prop.classes [ "fas"; "fa-home"]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                            for (key, value) in site.Params.Social |> Map.toList do
                                Bulma.levelItem [
                                    Bulma.button.a [
                                        prop.href ("https://" + key + ".com/" + value)
                                        prop.children [
                                            Bulma.icon [
                                                Html.i [
                                                    prop.classes [ "fab"; sprintf "fa-%s-f" key]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                        ]
                    ]
                ]
            ]
        ]

