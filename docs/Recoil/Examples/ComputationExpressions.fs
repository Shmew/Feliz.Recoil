[<RequireQualifiedAccess>]
module Samples.ComputationExpressions

open Feliz
open Feliz.Recoil

let textState = 
    atom {
        key "test"
        def ""
    }

let textStateAddition =
    selector {
        key "textStateSelector"
        get (fun get ->
            let text = get(textState)
            text + " wow"
        )
    }

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    selector {
        key "testing"
        getter (Recoil.get {
            let! addedText = textStateAddition
            return 
                addedText 
                |> String.filter(fun v -> 
                    List.contains v vowels)
        })
    }

let transformer = React.functionComponent(fun () ->
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

let atomDisplay = React.functionComponent(fun () ->
    let text = Recoil.useValue(textState)

    Html.div [
        prop.text (sprintf "Atom current value: %s" text)
    ])

let additionSelectorDisplay = React.functionComponent(fun () ->
    let text = Recoil.useValue(textStateAddition)

    Html.div [
        prop.text (sprintf "textStateAddition selector current value: %s" text)
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        transformer()
        atomDisplay()
        additionSelectorDisplay()
    ])
