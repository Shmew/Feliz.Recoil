[<RequireQualifiedAccess>]
module Samples.BidirectionalSelectors

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let textState = Recoil.atom("BidirectionalSelectors/textState", "Hello world!")

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    Recoil.selector (
        key = "BidirectionalSelectors/textStateTransform", 
        get = 
            (fun getter ->
                getter.get(textState)
                |> String.filter(fun v -> List.contains v vowels)),
        set =
            (fun selector newValue ->
                selector.set(textState, newValue + "whoa"))
    )

let inner = React.functionComponent(fun () ->
    let text, setText = Recoil.useState(textStateTransform)

    Html.div [
        Html.div [
            prop.text (sprintf "Transformed value: %s" text)
        ]
        Html.input [
            prop.classes [ Bulma.Input ]
            prop.style [ style.maxWidth (length.em 30) ]
            prop.type'.text
            prop.onTextChange setText
        ]
    ])

let otherInner = React.functionComponent(fun () ->
    let textAtom = Recoil.useValue(textState)

    Html.div [
        prop.text (sprintf "Atom current value: %s" textAtom)
    ]
)

let render = React.functionComponent("BidirectionalSelectors", fun () ->
    Recoil.root [
        inner()
        otherInner()
    ])
