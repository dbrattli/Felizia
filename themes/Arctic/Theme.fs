namespace Felizia.Arctic

open Felizia

module Theme =
    let name = "Felizia.Arctic"

    let theme () : Theme =
#if FABLE_COMPILER
        { Name = name; Index = Partials.Index.index; Single = Partials.SingleView.singleView; List = Partials.ListView.listView }
#else
        { Name = name; Index = Layouts.Index.index; Single = Layouts.SinglePage.singlePage; List = Layouts.ListPage.listPage }
#endif