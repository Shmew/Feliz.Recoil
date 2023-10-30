namespace Feliz.Recoil

open Feliz
open System.ComponentModel

module internal Impl =

    let notEqualsButFunctionsFamily<'a> =
        selectorFamily {
            key "__recoil_equalsButFunctions__"
            get (fun (x: 'a, y: 'a) _ ->
                not (JSe.is.equalsButFunctions x y)
            )
        }

    let reducerFamily<'Model,'Msg> =
        selectorFamily {
            key "__recoil_reducer__"
            set_only (fun (update: RecoilValue<'Msg -> 'Model -> 'Model,ReadOnly>, recoilValue: RecoilValue<'Model,ReadWrite>) setter (msg: 'Msg) ->
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
