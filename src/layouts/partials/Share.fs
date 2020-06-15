namespace Felizia.Partials

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
#endif

open Felizia.Model

[<AutoOpen>]
module Share =
    // Server side react
    let share (model: Model) dispatch : ReactElement =
        let site = model.CurrentSite
        let page = model.CurrentPage

        Bulma.container [
            Html.div [
                prop.className "is-flex"
                prop.children [
                    // Facebook
                    Bulma.button.a [
                        prop.href <| sprintf "https://www.facebook.com/sharer/sharer.php?u=%s" page.PermaLink
                        prop.children [
                            Bulma.icon [
                                Html.i [
                                    prop.classes [ "fab"; "fa-facebook-f"]
                                ]
                            ]
                        ]
                    ]

                    // Twitter
                    Bulma.button.a [
                        prop.href <| sprintf "https://twitter.com/intent/tweet?url=%s&text=%s" page.PermaLink page.Title
                        prop.children [
                            Bulma.icon [
                                Html.i [
                                    prop.classes [ "fab"; "fa-twitter"]
                                ]
                            ]
                        ]
                    ]

                    // Linkedin
                    Bulma.button.a [
                        prop.href <| sprintf "https://www.linkedin.com/shareArticle?url=%s&title=%s" page.PermaLink page.Title
                        prop.children [
                            Bulma.icon [
                                Html.i [
                                    prop.classes [ "fab"; "fa-linkedin"]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
