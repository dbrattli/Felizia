module Felizia.Yaml

open System
open System.IO
open System.Text.RegularExpressions

open Legivel.Attributes
open Legivel

open Felizia.Model

type YamlAuthor = {
    [<YamlField("given_name")>] GivenName: string option
    [<YamlField("family_name")>] FamilyName: string option
    [<YamlField("display_name")>] DisplayName: string option
} with
    member this.ToModel () : Author =
        {
            GivenName = this.GivenName
            FamilyName = this.FamilyName
            DisplayName = this.DisplayName
        }

type ParamEnum =
    | One=1
    | [<YamlValue("two")>] Two=2

type YamlParams = {
    [<YamlField("title")>]
    Title: string option
    [<YamlField("author")>]
    Author: string option
    [<YamlField("share")>]
    Share: bool option
    [<YamlField("searchURL")>]
    SearchUrl: string option
    [<YamlField("description")>]
    Description: string option
    [<YamlField("banner")>]
    Banner: string option
    [<YamlField("logo")>]
    Logo: string option
    [<YamlField("brand")>]
    Brand: string option
    [<YamlField("social")>]
    Social: Map<string, string> option
    [<YamlField("keywords")>]
    Keywords: string list option
    [<YamlField("literate")>]
    Literate: bool option

} with
    member this.ToModel () : SiteParams =
        {
            Title = this.Title
            Author = { Author.Empty with DisplayName = this.Author }
            Share = this.Share |> Option.defaultValue false
            SearchUrl = this.SearchUrl
            Description = this.Description
            Banner = this.Banner
            Logo = this.Logo
            Brand = this.Brand
            Social = this.Social |> Option.defaultValue Map.empty
            Keywords = this.Keywords |> Option.defaultValue List.empty
            Literate = this.Literate |> Option.defaultValue false
        }

type YamlLanguageInfo = {
    [<YamlField("baseURL")>]
    BaseUrl: string option
    [<YamlField("languageName")>]
    LanguageName: string option
    [<YamlField("title")>]
    Title: string option
    [<YamlField("description")>]
    Description: string option
    [<YamlField("weight")>]
    Weight: int option
    [<YamlField("params")>]
    Params: YamlParams option
} with
    member this.ToModel code : Language =
        {
            BaseUrl = this.BaseUrl |> Option.defaultValue ("/" + code)
            LanguageName = this.LanguageName  |> Option.defaultValue ""
            Title = this.Title |> Option.orElse this.LanguageName |> Option.defaultValue ""
            Params = this.Params |> Option.map(fun p -> p.ToModel ())
            Weight = this.Weight |> Option.defaultValue 0
            Lang = code
        }

type YamlSite = {
    [<YamlField("title")>]
    Title: string option
    [<YamlField("baseURL")>]
    BaseUrl: string option

    [<YamlField("params")>]
    Params: YamlParams option
    [<YamlField("summaryLength")>]
    SummaryLength: int option
    [<YamlField("languages")>]
    Languages: Map<string, YamlLanguageInfo> option
    [<YamlField("defaultContentLanguage")>]
    DefaultContentLanguage: string option
    [<YamlField("paginate")>]
    Paginate: int option
    [<YamlField("paginatePath")>]
    PaginatePath: string option
} with
    member this.ToModel () : Site =

        let empty = Site.Empty
        {  empty with
            Title = this.Title |> Option.defaultValue String.Empty
            BaseUrl = this.BaseUrl |> Option.defaultValue String.Empty
            Params = this.Params |> Option.map (fun p -> p.ToModel()) |> Option.defaultValue SiteParams.Empty
            SummaryLength = this.SummaryLength |> Option.defaultValue empty.SummaryLength

            Paginate = this.Paginate |> Option.defaultValue empty.Paginate
            PaginatePath = this.PaginatePath |> Option.defaultValue empty.PaginatePath

            AllTranslations = this.Languages |> Option.map (fun map -> map |> Map.toList |> List.map (fun (k, v) -> v.ToModel(k))) |> Option.defaultValue []
            DefaultContentLanguage = this.DefaultContentLanguage |> Option.defaultValue "en"
        }

type YamlPage = {
    [<YamlField("title")>]
    Title: string option
    [<YamlField("date")>]
    Date: DateTimeOffset option
    [<YamlField("publishDate")>]
    PublishDate: DateTimeOffset option
    [<YamlField("expiryDate")>]
    ExpieryDate: DateTimeOffset option
    [<YamlField("draft")>]
    Draft: bool option
    [<YamlField("image")>]
    Image: string option
    [<YamlField("weight")>]
    Weight: int option
} with
    member this.ToModel () : Page =
        { Page.Empty () with
            Title = this.Title |> Option.defaultValue String.Empty
            Params = { PageLevelParams.Empty with Image = this.Image |> Option.defaultValue "" }
            Weight = this.Weight |> Option.defaultValue 0
            Draft = this.Draft |> Option.defaultValue false
        }

    static member Deserialize input =
        match Serialization.Deserialize<YamlPage> input with
        | [ Serialization.Success info ] -> Some info.Data
        | [ Serialization.Error err ] -> None
        | _ -> None

let pattern = @"\s*(?:^---$(?<fm>.*?)^---$)?(?<md>^.*)?"
let regex = Regex(pattern, RegexOptions.Compiled ||| RegexOptions.Multiline ||| RegexOptions.Singleline)

let parseContent (source: string) =
    let folder (state: string*string) (m: string*string) =
        let (fm', md') = state
        let (fm, md) = m
        (if fm.Length > 0 then fm'+fm else fm'), (if md.Length > 0 then md'+md else md')

    let matches = regex.Matches(source)
    let fm, md =
        matches
        |> Seq.map (fun m -> m.Groups.["fm"].Value, m.Groups.["md"].Value)
        |> Seq.fold folder (" ", String.Empty)

    if md = String.Empty
    then fm, None
    else fm, Some md

let parseSiteConfig configPath =
    let config = File.ReadAllText (Path.Join(configPath, "config.yaml"))
    match Serialization.Deserialize<YamlSite> config with
    | [ Serialization.Success info ]  -> info.Data.ToModel ()
    | [ Serialization.Error err ] -> failwith <| sprintf "Site config parsing failed: %A" err
    | _ -> failwith "Site config required"


type YamlTranslation = {
    [<YamlField("other")>]
    Other: string
} with
    member this.ToModel () : Translation =
        { Other = this.Other }

let parseI18n i18nPath language =
    let pathName = Path.Join(i18nPath, language + ".yaml")
    match File.Exists pathName with
    | false -> Map.empty
    | true ->
        let file = File.ReadAllText pathName
        match Serialization.Deserialize<Map<string, YamlTranslation>> file with
        | [ Serialization.Success info ]  -> Map.map (fun k (v: YamlTranslation) -> v.ToModel ()) info.Data
        | [ Serialization.Error err ] -> failwith <| sprintf "i18n parsing failed: %A" err
        | _ -> Map.empty
