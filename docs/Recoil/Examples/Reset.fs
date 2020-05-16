[<RequireQualifiedAccess>]
module Samples.Reset

open Feliz
open Feliz.Recoil

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
            prop.type'.text
            prop.value inputBoxValue.current
            prop.onTextChange <| fun s ->
                inputBoxValue.current <- s
                setText s
        ]
        Html.button [
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

