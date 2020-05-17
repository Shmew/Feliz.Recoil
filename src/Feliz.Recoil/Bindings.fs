namespace Feliz.Recoil

open Fable.Core

[<RequireQualifiedAccess>]
module Bindings =
    open Fable.Core.JsInterop
    open Feliz

    type Recoil =
        static member RecoilRoot (props: obj) : ReactElement = import "RecoilRoot" "recoil"
        static member defaultValue : DefaultValue = import "DefaultValue" "recoil"
        static member atom<'T> (options: obj) : RecoilValue<'T,ReadWrite> = import "atom" "recoil"
        //static member atomFamily<'T,'P> (options: obj) : 'P -> RecoilValue<'T,ReadWrite> = import "atomFamily" "recoil"
        static member selector<'T,'Permissions> (options: obj) : RecoilValue<'T,'Permissions> = import "selector" "recoil"
        static member useRecoilValue<'T,'Permissions> (recoilValue: RecoilValue<'T,'Permissions>) : 'T = import "useRecoilValue" "recoil"
        static member useRecoilValueLoadable<'T,'Permissions> (recoilValue: RecoilValue<'T,'Permissions>) : Loadable<'T> = import "useRecoilValueLoadable" "recoil"
        static member useRecoilState<'T> (recoilState: RecoilValue<'T,ReadWrite>) : 'T * U2<'T -> unit,'T -> 'T -> unit> = import "useRecoilState" "recoil"
        static member useRecoilStateLoadable<'T> (recoilState: RecoilValue<'T,ReadWrite>) : Loadable<'T> * U2<'T -> unit,'T -> 'T -> unit> = import "useRecoilState" "recoil"
        static member useSetRecoilState<'T> (recoilState: RecoilValue<'T,ReadWrite>) : U2<'T -> unit,'T -> 'T -> unit> = import "useSetRecoilState" "recoil"
        static member useResetRecoilState<'T> (recoilState: RecoilValue<'T,ReadWrite>) : unit -> unit = import "useResetRecoilState" "recoil"
        static member useRecoilCallback<'T,'U> (f: obj, ?deps: ResizeArray<obj>) : 'T -> 'U = import "useRecoilCallback" "recoil"
