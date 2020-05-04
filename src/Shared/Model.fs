module Shared.Model

open System
open Legivel.Attributes

// The model holds data that you want to keep track of while the
// application is running in this case, we are keeping track of a
// counter we mark it as optional, because initially it will not be
// available from the client the initial value will be requested from
// server

type Msg =
    | PageNavigation of Pages*bool
    | Burger

// Page structure
type SiteContext = {
    Title: string
    PermaLink : Uri
    IsHome: bool
    Params: Map<string, string>
}

type FrontMatter = {
    [<YamlField("title")>] Title: string option
    [<YamlField("date")>] Date: DateTimeOffset option
    [<YamlField("publishDate")>] PublishDate: DateTimeOffset option
    [<YamlField("expiryDate")>] ExpieryDate: DateTimeOffset option
    [<YamlField("draft")>] Draft: bool option
    [<YamlField("image")>] Image: string option
    [<YamlField("weight")>] Weight: int option

    //Params: Map<string, string>
}

type Path = Path of string list
type File = {
    Path: string
    LogicalName: string
    BaseFileName: string
}

type SinglePage = {
    FrontMatter: FrontMatter option

    Summary: string option
    File: File
    /// The url is the relative URL for the piece of content. The url is based on the content’s location within the
    /// directory structure OR is defined in front matter and overrides all the above
    Url: Uri     // Uri to page content
}

type ListPage = {
    FrontMatter: FrontMatter option
    Summary: string option

    Url: Uri         // Uri to page content
    Pages: Page list // Sub-pages / sections
}

/// Main page type
and Page =
    | Home of ListPage
    | Section of ListPage
    | Page of SinglePage

type Model = {
    Burger: bool
    Site: SiteContext
    Pages: Page
    CurrentPage: Page
}

