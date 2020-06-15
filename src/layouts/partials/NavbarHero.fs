namespace Felizia.Partials

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
open Feliz.Bulma.Operators
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
open Feliz.Bulma.ViewEngine.Operators
#endif

open Felizia.Model

[<AutoOpen>]
module NavbarHero =
    let navbarHero (model: Model) dispatch =
        let site = model.CurrentSite

        Html.section [
            prop.className "hero"
            ++ section.isMedium
            ++ color.isDark
            prop.children [
                Html.div [
                    // Hero header: will stick at the top
                    prop.className "hero-head"
                    prop.children [
                        Navbar.navbar color.isDark model dispatch
                    ]
                ]
                Html.div [
                    prop.className "hero-body"
                    prop.style [
                        style.backgroundImageUrl (site.Params.Banner |> Option.defaultValue "")
                        style.backgroundPosition "center"
                        style.backgroundSize.cover
                    ]
                    prop.children [
                        Bulma.title.h2 (site.Title.ToUpper ())
                        Bulma.subtitle.h4 (site.Language.Params |> Option.bind (fun p -> p.Description) |> Option.orElse site.Params.Description |> Option.defaultValue "")
                    ]
                ]
            ]
        ]
