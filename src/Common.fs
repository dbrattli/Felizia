namespace Felizia.Common

open System

[<AutoOpen>]
module Utils =
    let [<Literal>] SlashChar = '/'
    let [<Literal>] SlashStr = "/"

    // Helpers for using optional values in Feliz templates
    let inline value a = Option.defaultValue String.Empty a
    let inline ( *.) (opt: option<'TSource>) (func: 'TSource -> option<'TResult>) : option<'TResult> = Option.bind func opt
    let inline ($) (opt: option<'T>) (func: option<'T> -> 'T) : 'T = opt |> func

    /// URL concatenate
    let (+/) (path1: string) (path2: string) =
        if path2.Length = 0
        then path1
        else if path1.Length = 0
        then path2
        else
            let ch = path1.[path1.Length - 1]
            if ch <> SlashChar
            then
                String.Join(SlashStr, [| path1; path2.TrimStart(SlashChar) |]).Trim()
            else
                path1 + path2.TrimStart(SlashChar).TrimEnd()

    let formatPath (segments: string list) =
        "/" + String.Join("/", segments)

    let upcase (key: string) =
        key.[0] |> string |> (fun (s: string) -> s.ToUpper()) |> (fun s -> s+ key.Substring(1))

