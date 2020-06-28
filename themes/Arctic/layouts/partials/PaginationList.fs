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
// https://glennmccomb.com/articles/how-to-build-custom-hugo-pagination/
module PaginationList =

    let paginationList (model: Model) dispatch =
        let page = model.CurrentPage
        let pagination = page.Paginator

        match pagination with
        | None -> Html.none
        | Some paginator ->
            Bulma.pagination [
                if paginator.HasPrev then
                    Bulma.paginationPrevious.a [
                        prop.href (string paginator.Prev.URL)
                        prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Prev.Url))
                        prop.text (model.T "Previous")
                    ]
                else
                    Bulma.paginationPrevious.a [
                        prop.disabled true
                        prop.href (string paginator.Prev.URL)
                        prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Prev.Url))
                        prop.text (model.T "Previous")
                    ]

                if paginator.HasNext then
                    Bulma.paginationNext.a [
                        prop.href (string paginator.Next.URL)
                        prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Next.Url))
                        prop.text (model.T "Next")
                    ]
                else
                    Bulma.paginationNext.a [
                        prop.disabled true
                        prop.href (string paginator.Next.URL)
                        prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Next.Url))
                        prop.text (model.T "Next")
                    ]
                Bulma.paginationList [
                    if paginator.HasPrev then
                        if paginator.Prev.PageNumber = 1 then
                            Bulma.paginationLink.a [
                                prop.href (string paginator.Prev.URL)
                                prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Prev.Url))
                                prop.text paginator.Prev.PageNumber
                            ]
                        else
                            Bulma.paginationLink.a [
                                prop.href (string paginator.First.URL)
                                prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation  paginator.First.Url))
                                prop.text paginator.First.PageNumber
                            ]

                            if paginator.Prev.PageNumber <> 1 then
                                Bulma.paginationEllipsis []
                            Bulma.paginationLink.a [
                                prop.href (string paginator.Prev.URL)
                                prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Prev.Url))
                                prop.text paginator.Prev.PageNumber
                            ]

                    Bulma.paginationLink.a [
                        paginationLink.isCurrent
                        prop.href (string paginator.URL)
                        prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Url))
                        prop.text paginator.PageNumber
                    ]

                    if paginator.HasNext then
                        if paginator.Next.PageNumber = paginator.Last.PageNumber then
                            Bulma.paginationLink.a [
                                prop.href (string paginator.Next.URL)
                                prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Next.Url))
                                prop.text paginator.Next.PageNumber
                            ]
                        else
                            Bulma.paginationLink.a [
                                prop.href (string paginator.Next.URL)
                                prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Next.Url))
                                prop.text paginator.Next.PageNumber
                            ]

                            if paginator.Next.PageNumber <> paginator.Last.PageNumber then
                                Bulma.paginationEllipsis []
                            Bulma.paginationLink.a [
                                prop.href (string paginator.Last.URL)
                                prop.onClick (fun ev -> ev.preventDefault (); dispatch (PageNavigation paginator.Last.Url))
                                prop.text paginator.Last.PageNumber
                            ]
                ]
            ]
