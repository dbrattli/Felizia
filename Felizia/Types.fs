namespace Felizia

open System
open Felizia.Common


[<RequireQualifiedAccess>]
module Defaults =
    [<Literal>]
    let SummaryLength = 70

    [<Literal>]
    let Paginate = 10

    [<Literal>]
    let PaginatePath = "page"

type Url = string list

type Path = Path of string list
type FileInfo = {
    Path: string
    LogicalName: string
    BaseFileName: string
}

type Translation = {
    Other: string
}

type Author = {
    GivenName: string option
    FamilyName: string option
    DisplayName: string option
} with
    static member Empty =
        {
            GivenName = None
            FamilyName = None
            DisplayName = None
        }

type SiteParams = {
    Title: string option
    Author: Author
    Share: bool
    SearchUrl: string option
    Description: string option
    Banner: string option
    Logo: string option
    Brand: string option
    Social: Map<string, string>
    Keywords: string list
    Literate: bool
} with
    static member Empty =
        {
            Title = None
            Author = Author.Empty
            Share = false
            SearchUrl = None
            Description = None
            Banner = None
            Logo = None
            Brand = None
            Social = Map.empty
            Keywords = []
            Literate = false
        }


type Language = {
    BaseUrl: string
    LanguageName: string
    Title: string
    Weight: int
    /// Language code
    Lang: string
    Params: SiteParams option
} with
    static member Default =
        { Lang = "en"; BaseUrl = ""; LanguageName = "English"; Title="English"; Weight=0; Params=None }

type PageLevelParams = {
    Image: string
} with
    static member Empty =
        { Image = String.Empty }

type Menu = {
    Url: Url
    URL: string
    Name: string
    Weight: int
}

and Paginator (pages: Page list, pagination: int, paginationPath: string, pageNumber: int, url: Url) =
    //do printfn "Paginator%A" (pageNumber, url)
    let allPages = List.collect (fun s -> List.filter (fun p -> p.IsPage) (s :: s.Pages)) pages
    let maxSkip = min ((pageNumber - 1) * pagination) (List.length allPages)
    let paginatorPages = if pageNumber > 0 then allPages |> List.skip maxSkip |> List.truncate pagination else []
    let totalPages = (List.length allPages + pagination - 1) / pagination

    //do printfn "TotalPages, AllPages: %A" (totalPages, allPages |> List.length)

    let pagerUrl =
        if pageNumber = 1
        then url
        else [ paginationPath; string pageNumber ]

    member x.Pages = paginatorPages
    member x.PageNumber = pageNumber
    member x.HasPrev = pageNumber > 1
    member x.HasNext = pageNumber < totalPages
    member x.TotalPages = totalPages
    member x.TotalNumberOfElements = 0
    member x.Url = pagerUrl
    member x.URL = "/" + (String.Join("/", pagerUrl))

    member x.Next =
        let pageNumber = x.PageNumber + 1
        Paginator(pages, pagination, paginationPath, pageNumber, url)
    member x.Prev =
        let pageNumber = x.PageNumber - 1
        Paginator(pages, pagination, paginationPath, pageNumber, url)
    member x.First =
        Paginator(pages, pagination, paginationPath, 1, url)
    member x.Last =
        let last = x.TotalPages
        Paginator(pages, pagination, paginationPath, last, url)

/// Main page type
and Page = {

    AllTranslations: Language list
    /// The content itself, defined below the front matter.
    Content: string option
    /// The description for the page.
    Description: string option
    /// A boolean, true if the content is marked as a draft in the front matter.
    Draft: bool
    /// Filesystem-related data for this content file.
    File: FileInfo option
    /// true in the context of the homepage.
    IsHome: bool
    /// Always true for regular content pages.
    IsPage: bool
    /// A language object that points to the language’s definition in the site config. .Language.Lang gives you the
    /// language code.
    Language: Language
    /// A collection of associated pages. This value will be empty within the context of regular content pages.
    Pages: Page list
    Paginator: Paginator option
    /// The Permanent link for this page.
    PermaLink: string
    Params: PageLevelParams
    //Site: Site
    /// A generated summary of the content for easily showing a snippet in a summary view.
    Summary: string option
    /// the title for this page.
    Title: string
    /// A list of translated versions of the current page. See Multilingual Mode for more information.
    Translations: Page list
    /// The url is the relative URL for the piece of content. The url is based on the content’s location within the
    /// directory structure OR is defined in front matter and overrides all the above
    Url: Url // Url segments to page content
    /// Assigned weight (in the front matter) to this content, used in sorting.
    Weight: int
    /// The date associated with the page.
    Date: DateTimeOffset

} with
    member this.IsMenuCurrent () =
        false

    static member Empty () =
        {
            AllTranslations = []
            Translations = []
            Content = None
            Description = None
            Draft = false
            File = None
            IsHome = false
            IsPage = true
            Language = Language.Default
            Pages = []
            Paginator = None
            Params = PageLevelParams.Empty
            PermaLink = String.Empty
            //Site = Site.Empty
            Summary = None
            Title = String.Empty
            Url = []
            Weight = 0
            Date = DateTimeOffset.Now
        }

// Page structure
and Site = {
    /// A string representing the theme of the site.
    Theme: string
    /// A string representing the title of the site.
    Title: string
    /// The base URL for the site as defined in the site configuration.
    BaseUrl : string
    /// Top-level directories of the site.
    Sections: Page list
    /// A container holding the values from the params section of your site configuration.
    Params: SiteParams
    /// Default summary length.
    SummaryLength: int
    /// All of the menus in the site.
    Menus: Menu list
    /// Reference to the homepage’s page object
    Home: Page
    /// Default = 10.
    Paginate: int
    /// Default = page. Allows you to set a different path for your pagination pages.
    PaginatePath: string
    /// List of all pages, regardless of their translation.
    AllPages: Page list
    /// List of all content ordered by Date with the newest first. This array contains only the pages in the current
    /// language.
    Pages: Page list
    Language: Language

    DefaultContentLanguage: string
    AllTranslations: Language list

    I18n: Map<string, Translation>

} with
    member x.BaseSegments () =
        let uri = Uri x.BaseUrl
        uri.AbsolutePath.Trim(SlashChar).Split(SlashChar)
        |> List.ofSeq

    static member Empty =
        {
            Theme = "Felizia.Arctic"
            Title = String.Empty
            BaseUrl = String.Empty
            DefaultContentLanguage = "en"
            Sections = []
            AllPages = []
            Pages = []
            Params = SiteParams.Empty
            SummaryLength = Defaults.SummaryLength
            Menus = []
            Home = Page.Empty ()
            Paginate = Defaults.Paginate
            PaginatePath = Defaults.PaginatePath
            AllTranslations = []
            Language = Language.Default

            I18n = Map.empty
        }
