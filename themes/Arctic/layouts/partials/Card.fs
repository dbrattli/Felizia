namespace Felizia.Arctic.Partials

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
#endif

open Felizia

[<AutoOpen>]
module Card =
    let card (page: Page) (model: Model) dispatch =

        Bulma.column [
            column.is4
            prop.children [
                Bulma.card [
                    prop.className "is-shady"
                    prop.children [
                        Bulma.cardImage [
                            Bulma.image [
                                Html.a [
                                    prop.href page.PermaLink
                                    prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation page.Url))
                                    prop.children [
                                        Html.img [
                                            prop.src page.Params.Image
                                        ]
                                    ]
                                ]
                            ]
                        ]

                        Bulma.cardContent [
                            Bulma.content [
                                Html.h4 page.Title
                                Html.p (page.Summary |> Option.defaultValue "")
                                Html.p [
                                    Html.a [
                                        prop.href page.PermaLink
                                        prop.style [ style.fontWeight.bold ]
                                        prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation page.Url))
                                        prop.text (model.T "learnMore")
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]