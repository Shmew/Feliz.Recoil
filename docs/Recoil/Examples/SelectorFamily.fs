[<RequireQualifiedAccess>]
module Samples.SelectorFamily

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let numberState = 
    atom {
        key "SelectorFamily/numberState" 
        def 2
    }

let multipliedState =
    selectorFamily {
        key "SelectorFamily/multipliedState"
        get (fun multiplier (getter: SelectorGetter) -> 
            getter.get(numberState) * multiplier)
    }

let maxWidth = style.maxWidth (length.em 30)

let label = React.functionComponent(fun (input: {| name: string |}) ->
    Html.div [
        prop.style [
            style.paddingBottom (length.em 1)
            style.paddingTop (length.em 1)
        ]
        prop.text input.name
    ])

let numberComp = React.functionComponent(fun () ->
    let setNumber = Recoil.useSetState(numberState)
    
    Html.div [
        label {| name = "Set number" |}
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.number
            prop.step 1
            prop.onTextChange <| fun s ->
                try (int s |> setNumber)
                with _ -> ()
            prop.defaultValue 2
        ]
    ])

let multResultComp = React.functionComponent(fun (input: {| multiplier: int |}) ->
    let multValue = Recoil.useValue(multipliedState(input.multiplier))

    React.fragment [
        label {| name = "Result" |}
        Html.div [
            prop.text (string multValue)
        ]
    ])

let multComp = React.functionComponent(fun () ->
    let mult,setMult = React.useState(0)

    Html.div [
        label {| name = "Set multiplier" |}
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.number
            prop.step 1
            prop.onTextChange <| fun s ->
                try (int s |> setMult)
                with _ -> ()
            prop.defaultValue 0
        ]
        multResultComp {| multiplier = mult |}
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        numberComp()
        multComp()
    ])
