namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open Feliz

[<Erase;RequireQualifiedAccess>]
type Recoil =
    static member inline atom<'T> (key: string, defaultValue: 'T) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> defaultValue ]
            |> createObj
        )

    static member inline atom<'T> (key: string, defaultValue: JS.Promise<'T>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> defaultValue ]
            |> createObj
        )

    static member inline atom<'T> (key: string, defaultValue: Async<'T>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> (defaultValue |> Async.StartAsPromise) ]
            |> createObj
        )

    static member inline atom<'T> (key: string, defaultValue: RecoilValue<'T,_>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> defaultValue ]
            |> createObj
        )

    static member inline defaultValue = Bindings.Recoil.defaultValue

    static member inline root (children: ReactElement list) =
        Bindings.Recoil.RecoilRoot(createObj ["children" ==> Interop.reactApi.Children.toArray children])
    static member inline root (initializer: RecoilValue<'T,ReadWrite> -> 'T -> unit , children: ReactElement list) =
        Bindings.Recoil.RecoilRoot(createObj [
            "props" ==> (createObj [ "initializeState" ==> System.Func<_,_,_>(fun o _  -> o?set |> initializer) ])
            "children" ==> Interop.reactApi.Children.toArray children
        ])

    static member inline selector<'T,'Perm,'U> (key: string, get: (RecoilValue<'T,'Perm> -> 'T) -> JS.Promise<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> fun o -> o?get |> get ]
            |> createObj
        )

    static member inline selector<'T,'Perm,'U> (key: string, get: (RecoilValue<'T,'Perm> -> 'T) -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> fun o -> o?get |> get
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )

    static member inline selector<'T,'Perm,'U> (key: string, get: (RecoilValue<'T,'Perm> -> 'T) -> Async<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> ((fun o -> o?get |> get) >> Async.StartAsPromise) ]
            |> createObj
        )

    static member inline selector<'T,'Perm,'U> (key: string, get: (RecoilValue<'T,'Perm> -> 'T) -> Async<'U>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> ((fun o -> o?get |> get) >> Async.StartAsPromise)
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )

    static member inline selector<'T,'Perm,'U> (key: string, get: (RecoilValue<'T,'Perm> -> 'T) -> RecoilValue<'U,_>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> (fun o -> o?get |> get) ]
            |> createObj
        )

    static member inline selector<'T,'Perm,'U> (key: string, get: (RecoilValue<'T,'Perm> -> 'T) -> RecoilValue<'U,_>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> (fun o -> o?get |> get)
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )

    static member inline useValue<'T> (recoilValue: RecoilValue<'T,ReadOnly>) =
        Bindings.Recoil.useRecoilValue<'T,ReadOnly>(recoilValue)

    static member inline useValue<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilValue<'T,ReadWrite>(recoilValue)

    static member inline useRecoilValueLoadable<'T> (recoilValue: RecoilValue<'T,ReadOnly>) =
        Bindings.Recoil.useRecoilValueLoadable<'T,ReadOnly>(recoilValue)

    static member inline useRecoilValueLoadable<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilValueLoadable<'T,ReadWrite>(recoilValue)

    static member inline useState<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilState<'T>(recoilValue)
        |> unbox<'T * ('T -> unit)>

    static member inline useStatePrev<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilState<'T>(recoilValue)
        |> unbox<'T * ('T -> 'T -> unit)>

    static member inline useStateLoadable<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilStateLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T> * ('T -> unit)>

    static member inline useStateLoadablePrev<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilStateLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T> * ('T -> 'T -> unit)>

    static member inline useSetState<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useSetRecoilState<'T>(recoilValue)
        |> unbox<'T -> unit>

    static member inline useSetStatePrev<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useSetRecoilState<'T>(recoilValue)
        |> unbox<'T -> 'T -> unit>

    static member inline useResetState<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useResetRecoilState<'T>(recoilValue)

    static member inline useCallback<'T,'U> (f: (CallbackMethods -> 'T -> 'U), ?deps: obj []) =
        Bindings.Recoil.useRecoilCallback<'T,'U>(System.Func<_,_,_>(f), ?deps = (deps |> Option.map ResizeArray))

    static member inline useCallbackRef<'T,'U> (f: (CallbackMethods -> 'T -> 'U)) =
        Bindings.Recoil.useRecoilCallback<'T,'U>(System.Func<_,_,_>(f))
        |> React.useCallbackRef

[<AutoOpen;Erase>]
module RecoilMagic =
    type Recoil with
        static member inline selector<'T,'Perm,'U> (key: string, get: (RecoilValue<'T,'Perm> -> 'T) -> 'U) =
            Bindings.Recoil.selector<'U,ReadOnly> (
                [ "key" ==> key
                  "get" ==> fun o -> o?get |> get ]
                |> createObj
            )

        static member inline selector<'T,'Perm,'U> (key: string, get: (RecoilValue<'T,'Perm> -> 'T) -> 'U, set: SelectorMethods -> 'T -> unit) =
            Bindings.Recoil.selector<'U,ReadWrite> (
                [ "key" ==> key
                  "get" ==> fun o -> o?get |> get
                  "set" ==> System.Func<_,_,_>(set) ]
                |> createObj
            )

        static member inline useCallback<'U> (f: (CallbackMethods -> 'U), ?deps: obj []) =
            Bindings.Recoil.useRecoilCallback<unit,'U>(System.Func<_,_>(f), ?deps = (deps |> Option.map ResizeArray))

        static member inline useCallbackRef<'U> (f: (CallbackMethods -> 'U)) =
            Bindings.Recoil.useRecoilCallback<unit,'U>(System.Func<_,_>(f))
            |> React.useCallbackRef
