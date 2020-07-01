namespace Felizia

open System
open System.IO

open Felizia
open Feliz.ViewEngine

type Theme = {
    Name: string
    SinglePage: View
    ListPage: View
}

module Theme =
    let adapt (page: Type) (method: string) (model: Model) (dispatch: Dispatch) =
        let ret = page.GetMethod(method).Invoke(null, [| box model; box dispatch |])
        ret :?> ReactElement

    let set (theme: string) (tmplPath: string) =
        let ListPage = Type.GetType(sprintf "%s.Layouts.ListPage, %s" theme theme)
        let SinglePage = Type.GetType(sprintf "%s.Layouts.SinglePage, %s" theme theme)
        let tmpl = File.ReadAllText (Path.Join(tmplPath, "Theme.fs.tmpl"))
        let themeFile = String.Format(tmpl, theme)
        do File.WriteAllText (Path.Join(tmplPath, "Theme.fs"), themeFile)

        let singlePage = adapt SinglePage "singlePage"
        let listPage = adapt ListPage "listPage"

        { Name = theme; SinglePage = singlePage; ListPage = listPage}


