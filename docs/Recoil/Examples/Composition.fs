[<RequireQualifiedAccess>]
module Samples.Composition

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let textState = Recoil.atom("Composition/textState", "Hello world!")

let otherTextState = Recoil.atom("Composition/otherTextState", "")

let textStateTransformed =
    recoil {
        let! text = 
            textState
            |> RecoilValue.map(fun s -> s + " wow")

        let! otherText = otherTextState

        return
            if otherText = "" then text
            else sprintf "%s - %s" text otherText
    }

let label = React.functionComponent(fun (input: {| name: string |}) ->
    Html.div [
        prop.style [
            style.paddingBottom (length.em 1)
            style.paddingTop (length.em 1)
        ]
        prop.text input.name
    ])

let maxWidth = style.maxWidth (length.em 30)

let textView = React.functionComponent(fun () ->
    let text = Recoil.useValue(textStateTransformed)

    Html.div [
        prop.text text
    ])

let textStateComp = React.functionComponent(fun () ->
    let setText = Recoil.useSetState(textState)

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.maxLength 40
            prop.onTextChange setText
        ]
    ])

let otherTextStateComp = React.functionComponent(fun () ->
    let setText = Recoil.useSetState(otherTextState)

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.maxLength 40
            prop.onTextChange setText
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        label {| name = "textState Atom:" |}
        textStateComp()
        label {| name = "otherTextState Atom:" |}
        otherTextStateComp()
        label {| name = "Output:" |}
        textView()
    ])
