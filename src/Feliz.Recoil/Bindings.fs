namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<EditorBrowsable(EditorBrowsableState.Never);RequireQualifiedAccess>]
module Bindings =
    open Fable.Core.JsInterop
    open Feliz

    type InnerDefaultValue =
        [<Emit("new $0()")>]
        abstract Create: unit -> DefaultValue

    type Recoil =
        static member RecoilRoot (props: obj) : ReactElement = import "RecoilRoot" "recoil"
        static member inline defaultValue : InnerDefaultValue = import "DefaultValue" "recoil"
        static member atom<'T> (options: obj) : RecoilValue<'T,ReadWrite> = import "atom" "recoil"
        static member atomFamily<'T,'P> (options: obj) : 'P -> RecoilValue<'T,ReadWrite> = import "atomFamily" "recoil"
        static member constSelector<'T> (constant: 'T) : RecoilValue<'T,ReadOnly> = import "constSelector" "recoil"
        static member noWait (recoilValue: RecoilValue<'T,'Mode>) : RecoilValue<Loadable<'T>,ReadOnly> = import "noWait" "recoil"
        static member selector<'T,'Mode> (options: obj) : RecoilValue<'T,'Mode> = import "selector" "recoil"
        static member selectorFamily<'T,'Mode,'P> (options: obj) : 'P -> RecoilValue<'T,'Mode> = import "selectorFamily" "recoil"
        static member useRecoilValue<'T,'Mode> (recoilValue: RecoilValue<'T,'Mode>) : 'T = import "useRecoilValue" "recoil"
        static member useRecoilValueLoadable<'T,'Mode> (recoilValue: RecoilValue<'T,'Mode>) : Loadable<'T> = import "useRecoilValueLoadable" "recoil"
        static member useRecoilState<'T> (recoilState: RecoilValue<'T,ReadWrite>) : 'T * U2<'T -> unit,('T -> 'T) -> unit> = import "useRecoilState" "recoil"
        static member useRecoilStateLoadable<'T> (recoilState: RecoilValue<'T,ReadWrite>) : Loadable<'T> * U2<'T -> unit,('T -> 'T) -> unit> = import "useRecoilState" "recoil"
        static member useSetRecoilState<'T> (recoilState: RecoilValue<'T,ReadWrite>) : U2<'T -> unit,('T -> 'T) -> unit> = import "useSetRecoilState" "recoil"
        static member useResetRecoilState<'T> (recoilState: RecoilValue<'T,ReadWrite>) : unit -> unit = import "useResetRecoilState" "recoil"
        static member useRecoilCallback<'T,'U> (f: obj, ?deps: ResizeArray<obj>) : 'T -> 'U = import "useRecoilCallback" "recoil"
        static member useTransactionObservation<'Values,'Metadata> (callback: TransactionObservation<'Values,'Metadata> -> unit) : unit = import "useTransactionObservation_UNSTABLE" "recoil"
        static member useTransactionSubscription<'T,'U> (callback: (Store<'T> * TreeState<'T>) -> 'U) : 'U = import "useTransactionSubscription_UNSTABLE" "recoil"
        static member useSetUnvalidatedAtomValues<'Value,'Metadata> (values: JS.Map<string,'Value>, ?transactionMetadata: 'Metadata) : unit = import "useSetUnvalidatedAtomValues_UNSTABLE" "recoil"
        static member waitForAll (recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) : RecoilValue<ResizeArray<'T>,ReadOnly> = import "waitForAll" "recoil"
        static member waitForAny (recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) : RecoilValue<ResizeArray<Loadable<'T>>,ReadOnly> = import "waitForAny" "recoil"
        static member waitForNone (recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) : RecoilValue<ResizeArray<Loadable<'T>>,ReadOnly> = import "waitForNone" "recoil"
