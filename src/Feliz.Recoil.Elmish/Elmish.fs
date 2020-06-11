namespace Feliz.Recoil

module Elmish =
    open Elmish
    open Fable.Core
    open Fable.Core.JsInterop
    open Feliz

    module internal Impl =
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

        [<Emit("typeof $0 === 'function'")>]
        let private isFunction (x: obj): bool = jsNative

        [<Emit("typeof $0 === 'object' && !$0[Symbol.iterator]")>]
        let private isNonEnumerableObject (x: obj): bool = jsNative 

        let equalsButFunctions (x: 'a) (y: 'a) =
            if obj.ReferenceEquals(x, y) then
                true
            elif isNonEnumerableObject x && not(isNull(box y)) then
                let keys = JS.Constructors.Object.keys x
                let length = keys.Count
                let mutable i = 0
                let mutable result = true
                while i < length && result do
                    let key = keys.[i]
                    i <- i + 1
                    let xValue = x?(key)
                    result <- isFunction xValue || xValue = y?(key)
                result
            else
                (box x) = (box y)

    open Impl

    type Recoil with
        static member useDispatch<'Model,'Msg> (model: RecoilValue<'Model,ReadWrite>, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>) =
            let state = React.useRef(None)
            let getState = Recoil.useCallbackRef(fun setter -> setter.getPromise(model))
            let setState = Recoil.useSetState(model)
            
            let ring = React.useRef(RingBuffer(10))

            let token = React.useRef(new System.Threading.CancellationTokenSource())

            let initializeState () =
                if state.current.IsNone && not token.current.IsCancellationRequested then
                    promise {
                        let! initState = getState()

                        state.current <- Some initState
                    }
                    |> Promise.start

            React.useEffectOnce(initializeState)

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
                        
                        if not (equalsButFunctions state.current (Some state')) then
                            state.current <- Some state'
                            setState state'
                }
                |> Promise.start

            React.useEffectOnce(fun () ->
                React.createDisposable <| fun () ->
                    token.current.Cancel()
                    token.current.Dispose())

            React.useCallbackRef(dispatch)

        /// Returns an elmish dispatch function.
        static member useDispatch (model: RecoilValue<'Model,ReadWrite>, update: 'Msg -> 'Model -> 'Model) =
            let setState = Recoil.useSetState(model)
            let getState = Recoil.useCallbackRef(fun setter -> setter.getPromise(model))

            let token = React.useRef(new System.Threading.CancellationTokenSource())

            let dispatch (msg: 'Msg) =
                promise {
                    let! state = getState()
                    let state' = update msg state

                    if not token.current.IsCancellationRequested && 
                       not (equalsButFunctions state state') then
                        
                        setState state'
                } |> Promise.start

            React.useEffectOnce(fun () ->
                React.createDisposable <| fun () ->
                    token.current.Cancel()
                    token.current.Dispose())

            let dispatch = React.useCallbackRef(dispatch)
            
            React.useCallbackRef(dispatch)

