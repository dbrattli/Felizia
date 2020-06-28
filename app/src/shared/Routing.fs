namespace Felizia

open System.Collections.Generic

#if FABLE_COMPILER
open Felizia.Arctic.Partials
#else
open Felizia.Arctic.Layouts
#endif

open Felizia.Model

[<AutoOpen>]
module Routing =
    let templates : IRouter = dict [
        [], Index.index
        [ "nb"], Index.index
    ]