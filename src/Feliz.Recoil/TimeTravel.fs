namespace Feliz.Recoil

open Fable.Core
open Feliz
open System.Threading
open System.ComponentModel

[<RequireQualifiedAccess>]
module TimeTravel =
    /// Functions to time travel the recoil state.
    [<NoComparison;NoEquality>]
    type Actions =
        { /// Travel the recoil state backwards i steps.
          backward: int -> unit
          /// Travel the recoil state backwards until the condition is met
          /// or you reach the beginning of the history buffer 
          /// (or initial application state).
          backwardUntil: (Snapshot -> Async<bool>) -> unit
          /// Travel the recoil state forwards i steps.
          forward: int -> unit
          /// Travel the recoil state forwards until the condition is met
          /// or you reach the present.
          forwardUntil: (Snapshot -> Async<bool>) -> unit }

        [<EditorBrowsable(EditorBrowsableState.Never)>]
        static member empty =
            let inline notInit _ = JS.console.error("Time traveler is not initialized!")

            { backward = notInit
              backwardUntil = notInit
              forward = notInit
              forwardUntil = notInit }

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let timeTraveler = React.createContext("RecoilTimeTravel", Actions.empty)
    
    let internal travelForward (past: 'a list) (future: 'a list) (currentValue: 'a) =
        match past, future with
        | _, [x] -> currentValue::past, [], x
        | _, x::xs -> currentValue::past, xs, x
        | _ -> past, future, currentValue

    let internal travelBackward (past: 'a list) (future: 'a list) (currentValue: 'a) =
        match past, future with
        | [x], _ -> [], currentValue::future, x
        | x::xs, _ -> xs, currentValue::future, x
        | _ -> past, future, currentValue

    let rec internal travelForwards (count: int) (past: 'a list) (future: 'a list) (currentValue: 'a)  =
        if count = 0 || future.IsEmpty then past, future, currentValue
        else
            travelForward past future currentValue
            |||> travelForwards (count - 1)

    let internal travelForwardsUntil (condition: Snapshot -> Async<bool>) (past: Snapshot list) (future: Snapshot list) (currentValue: Snapshot) =
        async {
            let rec travelForwardsCond count (past: Snapshot list) (future: Snapshot list) currentValue =
                async {
                    let! cond = condition currentValue

                    return!
                        if count = 0 || future.IsEmpty || cond then Async.lift (past, future, currentValue)
                        else
                            async {
                                let past, future, currentValue = travelForward past future currentValue
                                return! travelForwardsCond (count - 1) past future currentValue 
                            }
                            
                }

            return! travelForwardsCond future.Length past future currentValue
        }

    let rec internal travelBackwards (count: int) (past: 'a list) (future: 'a list) (currentValue: 'a) =
        if count = 0 || past.IsEmpty then past, future, currentValue
        else
            travelBackward past future currentValue
            |||> travelBackwards (count - 1)

    let internal travelBackwardsUntil (condition: Snapshot -> Async<bool>) (past: Snapshot list) (future: Snapshot list) (currentValue: Snapshot) =
        async {
            let rec travelBackwardsCond count (past: Snapshot list) future currentValue =
                async {
                    let! cond = condition currentValue

                    return!
                        if count = 0 || past.IsEmpty || cond then Async.lift (past, future, currentValue)
                        else
                            async {
                                let past, future, currentValue = travelBackward past future currentValue
                                return! travelBackwardsCond (count - 1) past future currentValue 
                            }
                            
                }

            return! travelBackwardsCond past.Length past future currentValue
        }

    let internal snapshotIdGen = 
        Recoil.Family.selector (
            key = "__recoil_time_travel__", 
            get = (fun (snapshot: RecoilValue<Snapshot,ReadOnly>) (getter: SelectorGetter) -> 
                getter.get(snapshot) 
                |> fun res -> res.getId())
        )

    let inline internal getSnapshotFromId (observedSnapshot: Snapshot) (givenSnapshot: Snapshot) =
        observedSnapshot
        |> Snapshot.getPromise (Bindings.Recoil.constSelector givenSnapshot |> snapshotIdGen)

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let internal observer = React.functionComponent(fun (input: {| setTimeTraveler: Actions -> unit; maxHistory: int option |}) ->
        let past = React.useRef([] : Snapshot list)
        let future = React.useRef([] : Snapshot list)
        let present = React.useRef(None : Snapshot option)
        let traveling = React.useRef(false)

        let cts = React.useRef(new CancellationTokenSource())

        let rollPast =
            React.useCallbackRef(fun (o: SnapshotObservation) ->
            match input.maxHistory with
            | Some i when past.current.Length > i ->
                past.current <- o.previousSnapshot::(past.current |> List.take (i-1))
            | _ -> past.current <- o.previousSnapshot::past.current)

        let snapshotEquals =
            Recoil.useCallbackRef(fun setter (o: SnapshotObservation) ->
                if not cts.current.IsCancellationRequested then
                    promise {
                        let! ss = getSnapshotFromId setter.snapshot o.snapshot
                        let! pastSS =
                            if not past.current.IsEmpty then
                                getSnapshotFromId setter.snapshot past.current.Head
                                |> Promise.map Some
                            else Promise.lift None
                        let! presentSS = 
                            if present.current.IsSome then
                                getSnapshotFromId setter.snapshot present.current.Value
                                |> Promise.map Some
                            else Promise.lift None
                        let! futureSS = 
                            if not future.current.IsEmpty then
                                getSnapshotFromId setter.snapshot future.current.Head
                                |> Promise.map Some
                            else Promise.lift None

                        if not cts.current.IsCancellationRequested then
                            match pastSS, presentSS, futureSS with
                            | Some pastSS, Some presentSS, Some futureSS ->
                                if pastSS <> ss && futureSS <> ss && presentSS <> ss then
                                    #if DEBUG
                                    JS.console.info("TIME TRAVEL: Evaluated as divergence.")
                                    #endif

                                    present.current <- Some o.snapshot
                                    future.current <- []
                                    rollPast o
                                    traveling.current <- false
                            | None, Some presentSS, Some futureSS ->
                                if futureSS <> ss && presentSS <> ss then
                                    #if DEBUG
                                    JS.console.info("TIME TRAVEL: Evaluated as divergence.")
                                    #endif

                                    present.current <- Some o.snapshot
                                    future.current <- []
                                    rollPast o
                                    traveling.current <- false
                            | _, Some presentSS, None ->
                                if presentSS <> ss then
                                    #if DEBUG
                                    JS.console.info("TIME TRAVEL: Evaluated as divergence.")
                                    #endif

                                    present.current <- Some o.snapshot
                                    rollPast o
                                    future.current <- []
                                    traveling.current <- false
                            | _ -> ()
                    }
                    |> Promise.start
            )

        Recoil.useTransactionObserver <| fun o ->
            match traveling.current with
            | false when not future.current.IsEmpty -> snapshotEquals o
            | false ->
                rollPast o
                present.current <- Some o.snapshot
            | true ->  
                traveling.current <- false

        let backward = 
            Recoil.useCallbackRef(fun setter (i: int) ->
                present.current
                |> Option.iter (fun psnt ->
                    let pst,ftr,psnt = travelBackwards i past.current future.current psnt
                    
                    past.current <- pst
                    future.current <- ftr
                    present.current <- Some psnt
                    traveling.current <- true

                    setter.gotoSnapshot(psnt)
                )
            )

        let backwardUntil =
            Recoil.useCallbackRef(fun setter (condition: Snapshot -> Async<bool>) ->
                present.current
                |> Option.iter (fun psnt ->
                    async {
                        let! pst,ftr,psnt = travelBackwardsUntil condition past.current future.current psnt
                        
                        past.current <- pst
                        future.current <- ftr
                        present.current <- Some psnt
                        traveling.current <- true

                        setter.gotoSnapshot(psnt)
                    }
                    |> fun res -> Async.StartImmediate(res, cts.current.Token)
                )
            )

        let forward = 
            Recoil.useCallbackRef(fun setter (i: int) ->
                present.current
                |> Option.iter (fun psnt ->
                    let pst,ftr,psnt = travelForwards i past.current future.current psnt
                    
                    past.current <- pst
                    future.current <- ftr
                    present.current <- Some psnt
                    traveling.current <- true

                    setter.gotoSnapshot(psnt)
                )
            )

        let forwardUntil =
            Recoil.useCallbackRef(fun setter (condition: Snapshot -> Async<bool>) ->
                present.current
                |> Option.iter (fun psnt ->
                    async {
                        let! pst,ftr,psnt = travelForwardsUntil condition past.current future.current psnt
                        
                        past.current <- pst
                        future.current <- ftr
                        present.current <- Some psnt
                        traveling.current <- true

                        setter.gotoSnapshot(psnt)
                    }
                    |> fun res -> Async.StartImmediate(res, cts.current.Token)
                )
            )
        
        React.useEffectOnce(fun () -> 
            input.setTimeTraveler { 
                backward = backward
                backwardUntil = backwardUntil
                forward = forward
                forwardUntil = forwardUntil 
            }
            
            React.createDisposable(fun () -> 
                cts.current.Cancel()
                cts.current.Dispose()))

        Html.none)

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let rootWrapper = React.functionComponent(fun (input: {| otherChildren: ReactElement list; maxHistory: int option |}) ->
        let actions,setActions = React.useState(Actions.empty)

        let setActions = React.useCallback(setActions, [||])

        React.contextProvider(timeTraveler, actions, [
            yield! input.otherChildren
            observer {| setTimeTraveler = setActions; maxHistory = input.maxHistory |}
        ])
    )

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never)>]
module RecoilTimeTravelExtension =
    type Recoil with
        /// Returns a record of time travel actions allowing you to shift state forwards
        /// and backwards.
        ///
        /// This will shift the entire application state within the nearest recoil root component.
        static member inline useTimeTravel () =
            React.useContext(TimeTravel.timeTraveler)
