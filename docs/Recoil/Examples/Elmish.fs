[<RequireQualifiedAccess>]
module Samples.Elmish

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

type Model = 
    { Count: int }

module Model =
    let state =
        atom {
            key "Elmish/myModel"
            def {
                Count = 0
            }
        }

    let count = state |> RecoilValue.map (fun m -> m.Count)

type Msg =
    | Increment
    | Decrement
    | IncrementTwice
    | DecrementTwice

let update (msg: Msg) (state: Model) =
    match msg with
    | Increment ->
        { state with Count = state.Count + 1 }
    | Decrement ->
        { state with Count = state.Count - 1 }
    | IncrementTwice ->
        { state with Count = state.Count + 2 }
    | DecrementTwice ->
        { state with Count = state.Count - 2 }

let countComp = React.functionComponent(fun () ->
    let count = Recoil.useValue(Model.count)

    Html.div (sprintf "Count: %i" count))

let actionsComp = React.functionComponent(fun () ->
    let dispatch = Recoil.useSetReducer(Model.state, update)

    Html.div [
        Html.button [
            prop.classes [ 
                Bulma.Button
                Bulma.HasBackgroundPrimary
                Bulma.HasTextWhite 
            ]
            prop.text "Increment"
            prop.onClick <| fun _ -> dispatch Increment
        ]
        Html.button [
            prop.classes [ 
                Bulma.Button
                Bulma.HasBackgroundPrimary
                Bulma.HasTextWhite 
            ]
            prop.text "Decrement"
            prop.onClick <| fun _ -> dispatch Decrement
        ]
        Html.button [
            prop.classes [ 
                Bulma.Button
                Bulma.HasBackgroundPrimary
                Bulma.HasTextWhite 
            ]
            prop.text "Increment Twice"
            prop.onClick <| fun _ -> dispatch IncrementTwice
        ]
        Html.button [
            prop.classes [ 
                Bulma.Button
                Bulma.HasBackgroundPrimary
                Bulma.HasTextWhite 
            ]
            prop.text "Decrement Twice"
            prop.onClick <| fun _ -> dispatch DecrementTwice
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        Html.div [
            countComp()
            actionsComp()
        ]
    ])
