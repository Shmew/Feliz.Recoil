[<RequireQualifiedAccess>]
module Samples.Elmish

open Css
open Elmish
open Feliz
open Feliz.Recoil
open Feliz.Recoil.Elmish
open Zanaptak.TypedCssClasses

let renderCount = React.functionComponent(fun () ->
    let countRef = React.useRef 0
    
    let mutable currentCount = countRef.current

    React.useEffect(fun () -> countRef.current <- currentCount)

    currentCount <- currentCount + 1

    Html.div [
        prop.text (sprintf "Render count: %i" currentCount)
    ])

let drawBorder' = React.functionComponent(fun (input: {| children: ReactElement list|}) ->
    Html.div [
        prop.classes [ Bulma.Box ]
        prop.children input.children
    ])

let inline drawBorder (children: ReactElement list) = 
    drawBorder' {| children = children |}

type Model = 
    { Count: int }

module Model =
    let atom =
        atom {
            key "Elmish/myModel"
            def {
                Count = 0
            }
        }

    let count = atom |> RecoilValue.map (fun m -> m.Count)

type Msg =
    | Increment
    | Decrement
    | IncrementIndirect
    | IncrementTwice
    | IncrementDelayed
    | IncrementTwiceDelayed

let update (msg: Msg) (state: Model) : Model * Cmd<Msg> =
    match msg with
    | Increment ->
        { state with Count = state.Count + 1 }, Cmd.ofSub (fun dispatch -> printfn "Increment")

    | Decrement ->
        { state with Count = state.Count - 1 }, Cmd.ofSub (fun dispatch -> printfn "Decrement")

    | IncrementIndirect ->
        state, Cmd.ofMsg Increment

    | IncrementTwice ->
        state, Cmd.batch [ Cmd.ofMsg Increment; Cmd.ofMsg Increment ]

    | IncrementDelayed ->
        state, Cmd.OfAsync.perform (fun () ->
            async {
                do! Async.Sleep 1000;
                return IncrementIndirect
            }) () (fun msg -> msg)

    | IncrementTwiceDelayed ->
        state, Cmd.batch [ Cmd.ofMsg IncrementDelayed; Cmd.ofMsg IncrementDelayed ]

let countComp = React.functionComponent(fun () ->
    let count = Recoil.useValue(Model.count)

    Html.div [
        prop.children [
            //renderCount()
            Html.div [
                prop.text (sprintf "Count: %i" count)
            ]
        ]
    ])

let actionsComp = React.functionComponent(fun () ->
    let dispatch = Recoil.useDispatch(Model.atom, update)

    Html.div [
        //renderCount()
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
            prop.text "Increment Indirect"
            prop.onClick <| fun _ -> dispatch IncrementIndirect
        ]
        Html.button [
            prop.classes [ 
                Bulma.Button
                Bulma.HasBackgroundPrimary
                Bulma.HasTextWhite 
            ]
            prop.text "Increment Delayed"
            prop.onClick <| fun _ -> dispatch IncrementDelayed
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
            prop.text "Increment Twice Delayed"
            prop.onClick <| fun _ -> dispatch IncrementTwiceDelayed
        ]
    ])

let render = React.functionComponent(fun () ->
    //drawBorder [
    Recoil.root [
        //Recoil.logger()
        Html.div [
            //renderCount()
            //drawBorder [
                countComp()
            //]
            //drawBorder [
                actionsComp()
            //]
        ]
        //]
    ])
