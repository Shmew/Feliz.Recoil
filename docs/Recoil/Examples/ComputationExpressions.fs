[<RequireQualifiedAccess>]
module Samples.ComputationExpressions

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let textState = 
    atom {
        key "ComputationExpressions/textState"
        def ""
    }

let textStateAddition =
    selector {
        key "ComputationExpressions/textStateSelector"
        get (fun getter ->
            let text = getter.get(textState)
            text + " wow"
        )
    }

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    selector {
        key "ComputationExpressions/textStateTransform"
        get (fun getter ->
            let addedText = getter.get(textStateAddition)
            
            addedText 
            |> String.filter(fun v -> 
                List.contains v vowels)
        )
    }

let transformer = React.functionComponent(fun () ->
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
