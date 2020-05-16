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

    static member inline atom<'T> (key: string, defaultValue: RecoilValue<'T>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> defaultValue ]
            |> createObj
        )

    static member inline defaultValue = Bindings.Recoil.defaultValue

    static member inline root (children: ReactElement list) =
        Bindings.Recoil.RecoilRoot(createObj ["children" ==> Interop.reactApi.Children.toArray children])

    static member inline selector<'T> (key: string, get: (RecoilValue<'T> -> 'T) -> 'T, ?set: SelectorMethods<'T> -> unit) =
        Bindings.Recoil.selector<'T> (
            [ "key" ==> key
              "get" ==> fun o -> o?get |> get
              if set.IsSome then "set" ==> set.Value ]
            |> createObj
        )

    static member inline selector<'T> (key: string, get: (RecoilValue<'T> -> 'T) -> JS.Promise<'T>, ?set: SelectorMethods<'T> -> unit) =
        Bindings.Recoil.selector<'T> (
            [ "key" ==> key
              "get" ==> fun o -> o?get |> get
              if set.IsSome then "set" ==> set.Value ]
            |> createObj
        )

    static member inline selector<'T> (key: string, get: (RecoilValue<'T> -> 'T) -> Async<'T>, ?set: SelectorMethods<'T> -> unit) =
        Bindings.Recoil.selector<'T> (
            [ "key" ==> key
              "get" ==> ((fun o -> o?get |> get) >> Async.StartAsPromise)
              if set.IsSome then "set" ==> set.Value ]
            |> createObj
        )

    static member inline selector<'T> (key: string, get: (RecoilValue<'T> -> 'T) -> RecoilValue<'T>, ?set: SelectorMethods<'T> -> unit) =
        Bindings.Recoil.selector<'T> (
            [ "key" ==> key
              "get" ==> (fun o -> o?get |> get)
              if set.IsSome then "set" ==> set.Value ]
            |> createObj
        )

    static member inline useValue<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useRecoilValue<'T>(recoilValue)

    static member inline useRecoilValueLoadable<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useRecoilValueLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T>>

    static member inline useState<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useRecoilState<'T>(recoilValue)
        |> unbox<'T * ('T -> unit)>

    static member inline useStatePrev<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useRecoilState<'T>(recoilValue)
        |> unbox<'T * ('T -> 'T -> unit)>

    static member inline useStateLoadable<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useRecoilStateLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T> * ('T -> unit)>

    static member inline useStateLoadablePrev<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useRecoilStateLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T> * ('T -> 'T -> unit)>

    static member inline useSetState<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useSetRecoilState<'T>(recoilValue)
        |> unbox<'T -> unit>

    static member inline useSetStatePrev<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useSetRecoilState<'T>(recoilValue)
        |> unbox<'T -> 'T -> unit>

    static member inline useResetState<'T> (recoilValue: RecoilValue<'T>) =
        Bindings.Recoil.useResetRecoilState<'T>(recoilValue)

    static member inline useCallback (f: ('T -> 'U), ?deps: obj []) =
        Bindings.Recoil.useRecoilCallback(f, ?deps = (deps |> Option.map ResizeArray))
