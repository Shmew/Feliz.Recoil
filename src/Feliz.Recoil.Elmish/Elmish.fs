namespace Feliz.Recoil

module Elmish =
    open Elmish
    open Fable.Core
    open Fable.Core.JsInterop
    open Feliz
    open System.ComponentModel

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    module Impl =
        [<Struct>]
        type RingState<'Item> =
            | Writable of wx: 'Item array * ix: int
            | ReadWritable of rw: 'Item array * wix:int * rix: int
        
        type RingBuffer<'Item>(size) =
            let doubleSize ix (items: 'Item array) =
                seq { yield! items |> Seq.skip ix
                      yield! items |> Seq.take ix
                      for _ in 0..items.Length do
                        yield Unchecked.defaultof<'Item> }
                |> Array.ofSeq
        
            let mutable state : 'Item RingState =
                Writable (Array.zeroCreate (max size 10), 0)
        
            member _.Pop() =
                match state with
                | ReadWritable (items, wix, rix) ->
                    let rix' = (rix + 1) % items.Length

                    match rix' = wix with
                    | true -> 
                        state <- Writable(items, wix)
                    | _ ->
                        state <- ReadWritable(items, wix, rix')

                    Some items.[rix]
                | _ -> None
                |> fun res -> res

            member _.Push (item:'Item) =
                match state with
                | Writable (items, ix) ->
                    items.[ix] <- item

                    let wix = (ix + 1) % items.Length

                    state <- ReadWritable(items, wix, ix)
                | ReadWritable (items, wix, rix) ->
                    items.[wix] <- item

                    let wix' = (wix + 1) % items.Length

                    match wix' = rix with
                    | true -> 
                        state <- ReadWritable(items |> doubleSize rix, items.Length, 0)
                    | _ -> 
                        state <- ReadWritable(items, wix', rix)

        let inline getDisposable (record: 'Model) =
            match box record with
            | :? System.IDisposable as disposable -> Some disposable
            | _ -> None

        [<Emit("Object.entries($0)")>]
        let objEntries (record: 'T) : ResizeArray<string * obj> = jsNative
        
        [<Emit("Object.entries($0)")>]
        let modelAtoms (record: 'T) : ResizeArray<string * RecoilValue<obj,ReadWrite>> = jsNative

        let modelAtomFamily<'AtomRecord> (selectorKey: string) =
            selectorFamily {
                key (sprintf "%s/_atoms_" selectorKey)
                get (fun (model: 'AtomRecord) _ -> modelAtoms(model))
            }

        let modelViewFamily (selectorKey: string) =
            selectorFamily {
                key (sprintf "%s/_view_" selectorKey)
                get (fun modelAtom _ ->
                    recoil {
                        let! modelAtom = modelAtom
                        let! modelSeq =
                            modelAtom
                            |> Seq.map (fun (field, recoilValue) ->
                                recoilValue
                                |> RecoilValue.map (fun v -> field, v))
                            |> RecoilValue.Seq.sequence
                        return unbox<'Model>(createObj !!modelSeq)
                    })
                set (fun (model: RecoilValue<ResizeArray<string * RecoilValue<obj,ReadWrite>>,ReadOnly>) setter (newModel: 'Model) ->
                    objEntries newModel
                    |> Seq.iter2(fun (_,(recoilValue: RecoilValue<obj,ReadWrite>)) (_,newValue) ->
                        if setter.get(recoilValue) <> newValue then
                            setter.set(recoilValue, newValue)
                    ) (setter.get(model)))
            }

    open Impl

    type Recoil with
        /// Returns an elmish dispatch function.
        static member useDispatch<'AtomRecord,'Model,'Msg> (selectorKey: string, model: 'AtomRecord, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>) =
            let modelView = 
                modelAtomFamily<'AtomRecord> selectorKey model
                |> modelViewFamily selectorKey

            let state = React.useRef(None)
            let getState = Recoil.useCallbackRef(fun setter -> setter.getPromise(modelView))
            let setState = Recoil.useSetState(modelView)
            
            let ring = React.useRef(RingBuffer(10))

            let token = React.useRef(new System.Threading.CancellationTokenSource())

            let initializeState () =
                if state.current.IsNone && not token.current.IsCancellationRequested then
                    promise {
                        let! initState = getState()

                        state.current <- Some initState
                    }
                    |> Promise.start

            React.useEffect(initializeState)

            let rec dispatch (msg: 'Msg) =
                promise {
                    while state.current.IsNone && not token.current.IsCancellationRequested do
                        do! Promise.sleep 10

                    let mutable nextMsg = Some msg

                    while nextMsg.IsSome && not token.current.IsCancellationRequested do
                        let msg = nextMsg.Value
                        let (state', cmd') = update msg state.current.Value
                        cmd' |> List.iter (fun sub -> sub dispatch)
                        nextMsg <- ring.current.Pop()
                        state.current <- Some state'
                        setState state'
                }
                |> Promise.start

            let dispatch = React.useCallbackRef(dispatch)

            React.useEffectOnce(fun () ->
                React.createDisposable <| fun () ->
                    token.current.Cancel()
                    token.current.Dispose())

            dispatch

        /// Returns an elmish dispatch function.
        static member useDispatch (selectorKey: string, model: 'AtomRecord, update: 'Msg -> 'Model -> 'Model) =
            let modelView = 
                modelAtomFamily<'AtomRecord> selectorKey model
                |> modelViewFamily selectorKey

            let setModelView = Recoil.useSetState(modelView)
            let getState = Recoil.useCallbackRef(fun setter -> setter.getPromise(modelView))

            let token = React.useRef(new System.Threading.CancellationTokenSource())

            let dispatch (msg: 'Msg) =
                promise {
                    let! state = getState()

                    if not token.current.IsCancellationRequested then
                        update msg state
                        |> setModelView
                } |> Promise.start

            React.useEffectOnce(fun () ->
                React.createDisposable <| fun () ->
                    token.current.Cancel()
                    token.current.Dispose())

            let dispatch = React.useCallbackRef(dispatch)
            
            React.useCallbackRef(dispatch)
