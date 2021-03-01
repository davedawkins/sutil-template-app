module Login

//
// Conversion of https://codepen.io/stevehalford/pen/YeYEOR into an example Sutil Login component
// See LoginExample.fs for usage
//

open Sutil
open Sutil.DOM
open Sutil.Attr
open Sutil.Bulma
open System

type LoginDetails = {
    Username: string
    Password: string
    RememberMe: bool
    }
with
    static member Default = { Username = ""; Password = ""; RememberMe = false }

type Model = LoginDetails

let private username m = m.Username
let private password m = m.Password
let private rememberMe m = m.RememberMe

type private Message =
    | SetUsername of string
    | SetPassword of string
    | SetRememberMe of bool
    | AttemptLogin
    | CancelLogin

let private init details =
    details, Cmd.none

module EventHelpers =
    open Browser.Types

    let inputElement (target:EventTarget) = target |> asElement<HTMLInputElement>

    let validity (e : Event) =
        inputElement(e.target).validity

// View function. Responsibilities:
// - Arrange for cleanup of model on dispose
// - Present model to user
// - Handle input according to Message API

let private defaultView (message : IObservable<string>) model dispatch =
    bulma.hero [
        disposeOnUnmount [ model ]

        //hero.isInfo

        bulma.heroBody [
            bulma.container [
                bulma.columns [
                    columns.isCentered
                    bulma.column [
                        column.tabletIs 10; column.desktopIs 8; column.widescreenIs 6
                        bulma.formBox [
                            on "submit" (fun _ -> AttemptLogin |> dispatch) [PreventDefault]
                            Attr.action ""

                            bulma.field [
                                class' "has-text-danger"
                                Bind.fragment message text
                            ] |> Transition.showIf (message |> Store.map (fun m -> m <> ""))

                            bulma.field [
                                bulma.label "Username"
                                bulma.control [
                                    control.hasIconsLeft
                                    bulma.input [

                                        bindEvent "input" (fun e -> EventHelpers.validity(e).valid |> not) (fun s -> bindClass s "is-danger")

                                        Attr.placeholder "Hint: your-name"
                                        Bind.attr ("value", model .> username , SetUsername >> dispatch)
                                        Attr.required true
                                    ]
                                    bulma.icon [
                                        icon.isSmall
                                        icon.isLeft
                                        fa "user"
                                    ]
                                ]
                            ]
                            bulma.field [
                                bulma.label "Password"
                                bulma.control [
                                    control.hasIconsLeft
                                    bulma.password [
                                        Attr.placeholder "Hint: sutilx9"
                                        Bind.attr("value", model .> password, SetPassword >> dispatch)
                                        Attr.required true]
                                    bulma.icon [
                                        icon.isSmall
                                        icon.isLeft
                                        fa "lock"
                                    ]
                                ]
                            ]
                            bulma.field [
                                bulma.labelCheckbox " Remember me" [
                                    Bind.attr("checked", model .> rememberMe, SetRememberMe >> dispatch)
                                ]
                            ]
                            bulma.field [
                                field.isGrouped
                                bulma.control [
                                    bulma.button [
                                        button.isSuccess
                                        text "Login"
                                    ]
                                ]
                                bulma.control [
                                    bulma.button [
                                        text "Cancel"
                                        onClick (fun _ -> dispatch CancelLogin) [PreventDefault]
                                    ]
                                ]
                            ]
                        ] ] ] ] ] ]

let private createWithView initDetails login cancel view =

    let update msg (model : Model) : Model * Cmd<Message> =
        match msg with
            |SetUsername name -> { model with Username = name }, Cmd.none
            |SetPassword pwd -> { model with Password = pwd}, Cmd.none
            |SetRememberMe z -> { model with RememberMe = z }, Cmd.none
            |AttemptLogin -> model, [ fun _ -> login model ]
            |CancelLogin -> model, [ fun _ -> cancel () ]

    let model, dispatch = initDetails |> Store.makeElmish init update ignore

    view model dispatch

let create initDetails (message : IObservable<string>) login cancel =
    createWithView initDetails login cancel (defaultView message)

