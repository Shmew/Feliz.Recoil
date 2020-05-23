# Feliz.Recoil - Elmish Example

Shows how to imlement an elmish model with Recoil.

```fsharp:recoil-elmish
type Model = 
    { Count: int }

module Model =
    type Atoms = 
        { Count: RecoilValue<int,ReadWrite> }

    let [<Literal>] key = "model"

    let atoms =
        { Count = Recoil.atom(sprintf "%s/count" key, 0) }

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
    let count = Recoil.useValue(Model.atoms.Count)

    Html.div [
        prop.children [
            Html.div [
                prop.text (sprintf "Count: %i" count)
            ]
        ]
    ])

let actionsComp = React.functionComponent(fun () ->
    let dispatch = Recoil.useDispatch(Model.key, Model.atoms, update)

    Html.div [
        Html.button [
            prop.text "Increment"
            prop.onClick <| fun _ -> dispatch Increment
        ]
        Html.button [
            prop.text "Decrement"
            prop.onClick <| fun _ -> dispatch Decrement
        ]
        Html.button [
            prop.text "Increment Indirect"
            prop.onClick <| fun _ -> dispatch IncrementIndirect
        ]
        Html.button [
            prop.text "Increment Delayed"
            prop.onClick <| fun _ -> dispatch IncrementDelayed
        ]
        Html.button [
            prop.text "Increment Twice"
            prop.onClick <| fun _ -> dispatch IncrementTwice
        ]
        Html.button [
            prop.text "Increment Twice Delayed"
            prop.onClick <| fun _ -> dispatch IncrementTwiceDelayed
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        countComp()
        actionsComp()
    ])
```
