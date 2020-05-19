[<RequireQualifiedAccess>]
module Samples.Reset

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let textState = Recoil.atom("textState", "Hello world!")

let inner = React.functionComponent(fun () ->
    let inputBoxValue = React.useRef ""
    let text,setText = Recoil.useState(textState)
    let reset = Recoil.useResetState(textState)

    Html.div [
        Html.div [
            prop.text (sprintf "Atom current value: %s" text)
        ]
        Html.input [
            prop.classes [ Bulma.Input ]
            prop.style [ style.maxWidth (length.em 30) ]
            prop.type'.text
            prop.value inputBoxValue.current
            prop.onTextChange <| fun s ->
                inputBoxValue.current <- s
                setText s
        ]
        Html.button [
            prop.classes [ 
                Bulma.Button
                Bulma.HasBackgroundPrimary
                Bulma.HasTextWhite 
            ]
            prop.text "Reset"
            prop.onClick <| fun _ -> 
                inputBoxValue.current <- ""
                reset()
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
       inner()
    ])

