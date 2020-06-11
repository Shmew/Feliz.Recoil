namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open System.ComponentModel

module internal Impl =
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

    let notEqualsButFunctionsFamily<'a> =
        selectorFamily {
            key "__recoil_equalsButFunctions__"
            get (fun (x: 'a, y: 'a) _ ->
                not (equalsButFunctions x y)
            )
        }

    let reducerFamily<'Model,'Msg> =
        selectorFamily {
            key "__recoil_reducer__"
            get (fun (_: RecoilValue<'Msg -> 'Model -> 'Model,ReadOnly>, recoilValue: RecoilValue<'Model,ReadWrite>) getter ->
                getter.get(recoilValue)
                |> unbox<'Msg>)
            set (fun (update: RecoilValue<'Msg -> 'Model -> 'Model,ReadOnly>, recoilValue: RecoilValue<'Model,ReadWrite>) setter (msg: 'Msg) ->
                let state = setter.get(recoilValue)
                let state' = state |> setter.get(update) msg
                
                if setter.get(notEqualsButFunctionsFamily(state, state')) then
                    setter.set(recoilValue, state')
            )
        }

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never)>]
module Reducer =
    open Impl
    
    type Recoil with
        /// Applies changes to an atom using the reducer pattern, and subscribes to the atom state.
        static member useReducer (model: RecoilValue<'Model,ReadWrite>, update: 'Msg -> 'Model -> 'Model) =
            let state = Recoil.useValue(model)

            let setState = Recoil.useSetState(reducerFamily<'Model,'Msg>(RecoilValue.lift(update), model))

            state, setState

        /// Applies changes to an atom using the reducer pattern.
        static member useSetReducer (model: RecoilValue<'Model,ReadWrite>, update: 'Msg -> 'Model -> 'Model) =
            let setState = Recoil.useSetState(reducerFamily<'Model,'Msg>(RecoilValue.lift(update), model))

            setState
