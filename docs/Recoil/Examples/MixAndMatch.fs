[<RequireQualifiedAccess>]
module Samples.MixAndMatch

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let textState = Recoil.atom("MixAndMatch/textState", "Hello world!")

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    Recoil.selector("MixAndMatch/textStateTransform", fun getter ->
        getter.get(textState)
        |> String.filter(fun v -> List.contains v vowels)
    )

let inner = React.functionComponent(fun () ->
    let setAtomText = Recoil.useSetState(textState)
    let text = Recoil.useValue(textStateTransform)

    Html.div [
        Html.div [
            prop.text (sprintf "Transformed value: %s" text)
        ]
        Html.input [
            prop.classes [ Bulma.Input ]
            prop.style [ style.maxWidth (length.em 30) ]
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

let render = React.functionComponent("MixAndMatch", fun () ->
    Recoil.root [
        inner()
        otherInner()
    ])

