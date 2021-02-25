module App

open Sutil
open Sutil.DOM
open Sutil.Attr
open Sutil.Styling

open Types
open State
open System
open Remote
open Sutil.Transition


let appStyle = [
    rule ".aside" [
        Css.display "block"
        Css.backgroundColor "#F9F9F9"
        Css.borderRight "1px solid #DEDEDE"
    ]
    rule ".aside .main" [
        Css.padding "40px"
        Css.color "#6F7B7E"
    ]
    rule ".aside .main .item" [
        Css.display "block"
        Css.padding "10px 0"
        Css.color "#6F7B7E"
    ]

    rule ".aside .main .item:hover, .aside .main .item:active" [
        Css.backgroundColor "#F2F2F2"
        Css.margin (Px 0.0, Px -50.0)
        Css.paddingLeft "50px"
    ]

    rule ".sutil-logo-badge" [
        Css.display "inline-flex"
        Css.fontFamily "'Coda Caption'"
        Css.alignItems "center"
        Css.justifyContent "center"
        Css.width "32px"
        Css.height "24px"
        Css.background "#444444"
        Css.color "white"
    ]
]

let viewPage model dispatch page =
    let errorMessage = (model |> Store.map getErrorMessage)
    let greeting = (model |> Store.map getGreeting)
    match page with
    | Home ->
        Home.create greeting
    | Users ->
        Users.create()
    | Login ->
        Login.create Login.LoginDetails.Default errorMessage (dispatch << LogUserIn) (fun _ -> Home |> SetPage |> dispatch)

let userIsSet (user : LoggedInUser option) = user.IsSome

// In Sutil, the view() function is called *once*
let view() =

    // Remote services
    let server = Server()

    // model is an IStore<ModeL>
    // This means we can write to it if we want, but when we're adopting
    // Elmish, we treat it like an IObservable<Model>
    let model, dispatch = () |> Store.makeElmish init (update server) ignore

    // Projections from model. These will be bound to elements below
    let page : IObservable<Page> = model |> Store.map getPage |> StoreXs.distinct
    let isLoggedIn : IObservable<bool> = model |> Store.map (getUser >> userIsSet)

    // Local store to connect hamburger to nav menu. We *could* route this through Elmish
    let navMenuActive = Store.make false
    // Local store for spotting media change. Also, could go through Elmish
    let isMobile = Store.make false
    let showAside = StoreXs.zip isMobile navMenuActive |> Store.map (fun (m,a) -> not m || a)

    // Listen to browser-sourced events
    let routerSubscription  = Navigable.listenLocation Router.parseRoute (dispatch << SetPage)
    let mediaSubscription = MediaQuery.listenMedia "(max-width: 768px)" (Store.set isMobile)

    fragment [
        el "nav" [
            class' "navbar has-shadow"
            attr("role","navigation")

            Html.div [
                class' "navbar-brand"

                // Temporary logo
                Html.h1 [
                    class' "title is-4 sutil-logo"
                    Html.a [
                        class' "navbar-item"
                        href "https://github.com/davedawkins/Sutil"
                        Html.div [ class' "sutil-logo-badge"; Html.span [ text "<>" ] ]
                        Html.span [ style [ Css.marginLeft "6px" ]; text "SUTIL" ]
                    ]
                ]
                Html.a [
                    role "button"
                    class' "navbar-burger"
                    Bindings.bindClass navMenuActive "is-active"
                    ariaLabel "menu"
                    ariaExpanded "false"
                    dataTarget "appNavMenu"
                    Html.span[ ariaHidden "true" ]
                    Html.span[ ariaHidden "true" ]
                    Html.span[ ariaHidden "true" ]
                    onClick (fun _ -> navMenuActive |> Store.modify not) [ PreventDefault ]
                ]
            ]


            Html.div [
                class' "navbar-menu"
                id' "appNavMenu"

                Bindings.bindClass navMenuActive "is-active"

                Html.div [
                    class' "navbar-end"
                    Html.div [
                        class' "navbar-item"
                        Html.div [
                            class' "buttons"
                            Bind.fragment isLoggedIn <| fun loggedIn ->
                                Html.a [
                                    class' "button is-light"
                                    if (loggedIn) then
                                        fragment [
                                            text "Logout"
                                            onClick (fun _ -> dispatch LogUserOut) [ PreventDefault ]
                                        ]
                                    else
                                        fragment [
                                            text "Login"
                                            onClick (fun _ -> dispatch (SetPage Login)) [ PreventDefault ]
                                        ]
                                ]
                        ]
                    ]
                ]
            ]
        ]

        Html.div [
            class' "columns"
            disposeOnUnmount [ model ]
            unsubscribeOnUnmount [ routerSubscription; mediaSubscription ]

            el "aside" [
                class' "column is-2 aside hero is-fullheight"
                Html.div [
                    class' "main"
                    Html.a [ class' "item"; href "#home"; text "Home" ]
                    Html.a [ class' "item"; href "#users"; text "Users" ]
                ]
            ] |> transition [fly |> withProps [ Duration 500.0; X -500.0 ] |> In] showAside

            Html.div [
                class' "column is-10"
                Bind.fragment page <| viewPage model dispatch
            ]
        ]
    ] |> withStyle appStyle

// Start the app
view() |> mountElement "sutil-app"