namespace Felizia.Partials

open System

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
open Feliz.Bulma.Operators
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
open Feliz.Bulma.ViewEngine.Operators
#endif
open Felizia.Common
open Felizia.Model

[<AutoOpen>]
module Navbar =

    /// <summary>
    /// Generate Bulma navbar
    /// </summary>
    /// <param name="color">The color of the Navbar, e.g color.isDark or color.isWhite.</param>
    /// <param name="model">The current model.</param>
    /// <param name="dispatch">The Elmish dispatch function.</param>
    /// <returns>Bulma Navbar react element.</returns>
    let navbar color (model: Model) dispatch =
        let site = model.CurrentSite

        Bulma.navbar [
            color
            prop.children [
                Bulma.container [
                    Bulma.navbarBrand.div [
                        Bulma.navbarItem.a [
                            prop.href site.Home.PermaLink
                            prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation site.Home.Url))
                            prop.children [
                                Html.img [
                                    prop.alt "Brand"
                                    prop.src (site.Params.Brand |> Option.defaultValue "")
                                ]
                                Bulma.title.h4 site.Title
                            ]
                        ]
                        Bulma.navbarBurger [
                            if model.Burger
                                then navbarMenu.isActive
                            prop.onClick (fun _ -> dispatch ToggleBurger)
                            prop.custom("data-target", "navbarMenu")
                            prop.children [
                                Html.span []
                                Html.span []
                                Html.span []
                            ]
                        ]
                    ]
                    Bulma.navbarMenu [
                        if model.Burger
                        then navbarMenu.isActive
                        prop.id "navbarMenu"
                        prop.children [
                            Bulma.navbarStart.div [
                                // Only show language selector for top level navigation bar.
                                if model.CurrentUrl = site.Home.Url then
                                    for lang in site.AllTranslations do
                                        Bulma.navbarItem.a [
                                            prop.href lang.BaseUrl
                                            prop.text lang.Title
                                            prop.onClick (fun ev -> ev.preventDefault (); dispatch (SetLanguage lang.Lang))
                                        ]
                            ]

                            Bulma.navbarEnd.div [
                                for menu in site.Menus do
                                    Bulma.navbarItem.a [
                                        if menu.IsMenuCurrent model
                                        then navbarItem.isActive
                                        prop.href menu.URL
                                        prop.text menu.Name
                                        prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation menu.Url))
                                    ]
                            ]
                        ]
                    ]
                ]
            ]
        ]


(*

<nav class="navbar has-shadow is-white"
     role="navigation" aria-label="main navigation">
  <div class="container">

    <div class="navbar-brand">
      <a class="navbar-item" href="/">
        {{- if (fileExists (print "/static" .Site.Data.defaults.icons.brand)) }}
        <img alt="Brand" src="{{ .Site.Data.defaults.icons.brand }}">
        {{- end }}
        <div class="title is-4">&nbsp;{{ .Site.Title }}</div>
      </a>

      <a role="button" class="navbar-burger" data-target="navMenu" aria-label="menu" aria-expanded="false">
        <span aria-hidden="true"></span>
        <span aria-hidden="true"></span>
        <span aria-hidden="true"></span>
      </a>
    </div>

    <div class="navbar-menu" id="navMenu">
      <div class="navbar-end">
      {{- $node := . }}
      {{- range .Site.Menus.main }}
      <a href="{{ .URL }}"
         class="navbar-item
                {{ if or ($node.IsMenuCurrent "main" .) ($node.HasMenuCurrent "main" .) -}}
                  is-active
                {{- end }}">{{ .Name }}
      </a>
      {{- end }}
      </div>
    </div>

  </div>
</nav>

*)