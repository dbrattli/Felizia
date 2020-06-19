namespace Felizia

open System.Collections.Generic

#if FABLE_COMPILER
open Felizia.Partials
#else
open Felizia.Layouts
#endif

open Felizia.Model

[<AutoOpen>]
module Routing =
    let templates : IDictionary<Url, View> = dict [
        [], Index.index
        [ "nb"], Index.index
    ]