[<RequireQualifiedAccess>]
module Samples.Basic

open Feliz
open Feliz.Recoil

let textState = Recoil.atom("textState", "Hello world!")

let textStateTransform =
    Recoil.selector(
        "textStateSelector",
        (fun get ->
            get(textState)
            |> String.filter(fun c -> c <> 'o'))
    )

let inner = React.functionComponent(fun () ->
    let setAtomText = Recoil.useSetState(textState)
    let text = Recoil.useValue(textStateTransform)

    Html.div [
        Html.div [
            prop.text (sprintf "Transformed value: %s" text)
        ]
        Html.input [
            prop.type'.text
            prop.onTextChange setAtomText
        ]
    ])

let otherInner = React.functionComponent(fun () ->
    let textAtom = Recoil.useValue(textState)

    Html.div [
        prop.text (sprintf "Atom current value: %s" textAtom)
    ]
)

let render = React.functionComponent(fun () ->
    Recoil.root [
       inner()
       otherInner()
    ])

