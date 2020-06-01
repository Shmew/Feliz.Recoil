[<RequireQualifiedAccess>]
module Samples.Previous

open Css
open Feliz
open Feliz.Recoil
open Zanaptak

let textState = Recoil.atom("Previous/textState", "Hello world!")
let prevTextState = Recoil.atom("Previous/prevTextState", "")

let views = React.functionComponent(fun () ->
    let text = Recoil.useValue(textState)
    let prevText = Recoil.useValue(prevTextState)
    
    React.fragment [
        Html.div [
            prop.text (sprintf "Atom current value: %s" text)
        ]
        Html.div [
            prop.text (sprintf "Atom previous value: %s" prevText)
        ]
    ]
    )

let input = React.functionComponent(fun () ->
    let setText = Recoil.useSetStatePrev(textState)
    let setPrevText = Recoil.useSetState(prevTextState)

    Html.input [
        prop.classes [ Bulma.Input ]
        prop.style [ style.maxWidth (length.em 30) ]
        prop.type'.text
        prop.onTextChange <| fun s -> 
            setText <| fun current -> 
                setPrevText current
                s
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        Html.div [
            views()
            input()
        ]
    ])
