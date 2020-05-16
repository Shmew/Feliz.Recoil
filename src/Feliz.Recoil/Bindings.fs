namespace Feliz.Recoil

open Fable.Core

[<RequireQualifiedAccess>]
module Bindings =
    open Fable.Core.JsInterop
    open Feliz

    type Recoil =
        static member RecoilRoot (props: obj) : ReactElement = import "RecoilRoot" "recoil"
        static member defaultValue : DefaultValue = import "DefaultValue" "recoil"
        static member atom<'T> (options: obj) : RecoilValue<'T> = import "atom" "recoil"
        static member selector<'T> (options: obj) : RecoilValue<'T> = import "selector" "recoil"
        static member useRecoilValue<'T> (recoilValue: RecoilValue<'T>) : 'T = import "useRecoilValue" "recoil"
        static member useRecoilValueLoadable (recoilValue: RecoilValue<'T>) : Loadable<'T> = import "useRecoilValueLoadable" "recoil"
        static member useRecoilState (recoilState: RecoilValue<'T>) : 'T * U2<'T -> unit,'T -> 'T -> unit> = import "useRecoilState" "recoil"
        static member useRecoilStateLoadable (recoilState: RecoilValue<'T>) : Loadable<'T> * U2<'T -> unit,'T -> 'T -> unit> = import "useRecoilState" "recoil"
        static member useSetRecoilState (recoilState: RecoilValue<'T>) : U2<'T -> unit,'T -> 'T -> unit> = import "useSetRecoilState" "recoil"
        static member useResetRecoilState (recoilState: RecoilValue<'T>) : unit -> unit = import "useResetRecoilState" "recoil"
        static member useRecoilCallback (f: 'T -> 'U, ?deps: ResizeArray<obj>) : 'T -> 'U = import "useRecoilCallback" "recoil"
