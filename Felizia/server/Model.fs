namespace Felizia

open System.Collections.Generic
open Feliz.ViewEngine

type View = Model -> Dispatch -> ReactElement
type IRouter = IDictionary<Url, View>

type Theme = {
    Name: string
    Index: View
    Single: View
    List: View
}


