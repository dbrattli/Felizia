namespace Felizia

#if FABLE_COMPILER
open Felizia.Arctic.Partials
#else
open Felizia.Arctic.Layouts
#endif

[<AutoOpen>]
module Routing =
    let templates : IRouter = dict [
        [], Index.index
        [ "nb"], Index.index
    ]