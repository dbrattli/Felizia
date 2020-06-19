namespace Felizia.Partials

open System

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
#else
open Feliz.ViewEngine
open Feliz.Bulma.ViewEngine
#endif

open Felizia.Model

[<AutoOpen>]
module PaginationSingle =

    let paginationSingle (model: Model) dispatch =
        let page = model.CurrentSite

        Bulma.container [
            Bulma.level [
                Bulma.levelLeft [
                    Bulma.levelItem [
                        Bulma.control.p [

                        ]
                    ]
                ]
                Bulma.levelRight [
                     Bulma.levelItem []
                ]
            ]
        ]
(*
<div class="container">
    <div class="level">

      <div class="level-left">
        <div class="level-item">
          <p class="control has-addons">
            {{- if .NextPage }}
            <a class="button" href="{{ .NextPage.Permalink }}">
              <span class="icon is-small"><i class="fa fa-chevron-left"></i></span>
              <span class="is-hidden-touch is-hidden-desktop-only">
                {{ .NextPage.Title | truncate 100 "..." }}
              </span>
              <span class="is-hidden-touch is-hidden-widescreen">
                {{ .NextPage.Title | truncate 75 "..." }}
              </span>
              <span class="is-hidden-mobile is-hidden-desktop">
                {{ .NextPage.Title | truncate 50 "..." }}
              </span>
              <span class="is-hidden-tablet">
                {{ .NextPage.Title | truncate 40 "..." }}
              </span>
              <!-- <span>&nbsp;| {{ title .NextPage.Section | pluralize }}</span> -->
            </a>
            {{- else }}
            <a class="button" href="#" disabled>
              <span class="icon is-small"><i class="fa fa-chevron-left"></i></span>
              <span>Newest</span>
            </a>
            {{- end }}
          </p>
        </div>
      </div>

      <div class="level-right">
        <div class="level-item">
          <p class="control has-addons">
            {{- if .PrevPage }}
            <a class="button" href="{{ .PrevPage.Permalink }}">
              <span class="is-hidden-touch is-hidden-desktop-only">
                {{ .PrevPage.Title | truncate 100 "..." }}
              </span>
              <span class="is-hidden-touch is-hidden-widescreen">
                {{ .PrevPage.Title | truncate 75 "..." }}
              </span>
              <span class="is-hidden-mobile is-hidden-desktop">
                {{ .PrevPage.Title | truncate 50 "..." }}
              </span>
              <span class="is-hidden-tablet">
                {{ .PrevPage.Title | truncate 40 "..." }}
              </span>
              <!-- <span>&nbsp;| {{ title .PrevPage.Section | pluralize }}</span> -->
              <span class="icon is-small"><i class="fa fa-chevron-right"></i></span>
            </a>
            {{- else }}
            <a class="button" href="#" disabled>
              <span>Oldest</span>
              <span class="icon is-small"><i class="fa fa-chevron-right"></i></span>
            </a>
            {{- end }}
          </p>
        </div>
      </div>

    </div>
  </div>
  *)