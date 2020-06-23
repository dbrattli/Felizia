namespace Felizia.Model

open System.Collections.Generic
open Feliz.ViewEngine

type View = Model -> Dispatch -> ReactElement
type IRouter = IDictionary<Url, View>


