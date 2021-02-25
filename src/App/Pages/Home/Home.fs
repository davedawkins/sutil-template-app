module Home

open Sutil
open Sutil.DOM
open Sutil.Bulma
open Sutil.Attr
open System

let create (message : IObservable<string>) =
    bulma.section [
        Html.h2 [ class' "title is-2"; text "Home" ]
        el "article" [
            class' "message is-info"
            Html.div [
                class' "message-body"
                Bind.fragment message text
            ]
        ]
    ]