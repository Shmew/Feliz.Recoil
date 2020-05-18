[<RequireQualifiedAccess>]
module Samples.BidirectionalSelectors

open Feliz
open Feliz.Recoil

let textState = Recoil.atom("textState", "Hello world!")

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    Recoil.selector (
        key = "textStateSelector", 
        get = 
            (fun getter ->
                getter.get(textState)
                |> String.filter(fun v -> List.contains v vowels)),
        set =
            (fun selector newValue ->
                Fable.Core.JS.console.log(selector)
                Fable.Core.JS.console.log(newValue)
                selector.set(textState, newValue + "whoa"))
    )

let inner = React.functionComponent(fun () ->
    let text, setText = Recoil.useState(textStateTransform)

    Html.div [
        Html.div [
            prop.text (sprintf "Transformed value: %s" text)
        ]
        Html.input [
            prop.type'.text
            prop.onTextChange (fun s ->
                Fable.Core.JS.console.log(s)
                Fable.Core.JS.console.log(setText)
                Fable.Core.JS.console.log(text)
                setText s)
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
