namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open Feliz

[<Erase;RequireQualifiedAccess>]
type Recoil =
    /// Creates a RecoilValue with a default value of what the 
    /// promise resolves to.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline atom<'T> (defaultValue: JS.Promise<'T>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> System.Guid.NewGuid()
              "default" ==> defaultValue ]
            |> createObj
        )
    /// Creates a RecoilValue with a default value of what the 
    /// promise resolves to.
    static member inline atom<'T> (key: string, defaultValue: JS.Promise<'T>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> defaultValue ]
            |> createObj
        )
    /// Creates a RecoilValue with a default value of what the 
    /// async returns.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline atom<'T> (defaultValue: Async<'T>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> System.Guid.NewGuid()
              "default" ==> (defaultValue |> Async.StartAsPromise) ]
            |> createObj
        )
    /// Creates a RecoilValue with a default value of what the 
    /// async returns.
    static member inline atom<'T> (key: string, defaultValue: Async<'T>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> (defaultValue |> Async.StartAsPromise) ]
            |> createObj
        )
    /// Creates a RecoilValue with a default value the given RecoilValue.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline atom<'T,'Mode> (defaultValue: RecoilValue<'T,'Mode>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> System.Guid.NewGuid()
              "default" ==> defaultValue ]
            |> createObj
        )
    /// Creates a RecoilValue with a default value the given RecoilValue.
    static member inline atom<'T,'Mode> (key: string, defaultValue: RecoilValue<'T,'Mode>) =
        Bindings.Recoil.atom<'T> (
            [ "key" ==> key
              "default" ==> defaultValue ]
            |> createObj
        )

    /// Used in selectors to get the RecoilValue's default value or to 
    /// set the RecoilValue to the default value.
    static member inline defaultValue = Bindings.Recoil.defaultValue

    /// Provides the context in which atoms have values. 
    /// 
    /// Must be an ancestor of any component that uses any Recoil hooks. 
    /// 
    /// Multiple roots may co-exist; atoms will have distinct values 
    /// within each root. If they are nested, the innermost root will 
    /// completely mask any outer roots.
    static member inline root (children: ReactElement list) =
        Bindings.Recoil.RecoilRoot(createObj ["children" ==> Interop.reactApi.Children.toArray children])
    /// Provides the context in which atoms have values. 
    /// 
    /// Must be an ancestor of any component that uses any Recoil hooks. 
    /// 
    /// Multiple roots may co-exist; atoms will have distinct values 
    /// within each root. If they are nested, the innermost root will 
    /// completely mask any outer roots.
    static member inline root (initializer: RecoilValue<'T,ReadWrite> -> 'T -> unit, children: ReactElement list) =
        Bindings.Recoil.RecoilRoot(createObj [
            "props" ==> (createObj [ "initializeState" ==> System.Func<_,_,_>(fun o _  -> o?set |> initializer) ])
            "children" ==> Interop.reactApi.Children.toArray children
        ])

    /// Derives state and returns a RecoilValue via the provided get function.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline selector<'T,'Mode,'U> (get: SelectorGetter -> JS.Promise<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> System.Guid.NewGuid()
              "get" ==> get ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    static member inline selector<'T,'Mode,'U> (key: string, get: SelectorGetter -> JS.Promise<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> get ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline selector<'T,'Mode,'U> (get: SelectorGetter -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> System.Guid.NewGuid()
              "get" ==> get
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    static member inline selector<'T,'Mode,'U> (key: string, get: SelectorGetter -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> get
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline selector<'T,'Mode,'U> (get: SelectorGetter -> Async<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> System.Guid.NewGuid()
              "get" ==> (get >> Async.StartAsPromise) ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    static member inline selector<'T,'Mode,'U> (key: string, get: SelectorGetter -> Async<'U>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> (get >> Async.StartAsPromise) ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline selector<'T,'Mode,'U> (get: SelectorGetter -> Async<'U>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> System.Guid.NewGuid()
              "get" ==> (get >> Async.StartAsPromise)
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    static member inline selector<'T,'Mode,'U> (key: string, get: SelectorGetter -> Async<'U>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> (get >> Async.StartAsPromise)
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline selector<'T,'Mode,'U> (get: SelectorGetter -> RecoilValue<'U,'Mode>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> System.Guid.NewGuid()
              "get" ==> get ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    static member inline selector<'T,'Mode,'U> (key: string, get: SelectorGetter -> RecoilValue<'U,'Mode>) =
        Bindings.Recoil.selector<'U,ReadOnly> (
            [ "key" ==> key
              "get" ==> get ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    /// 
    /// Uses a generated GUID for the key.
    ///
    /// This should not be used if you need data persistence.
    static member inline selector<'T,'Mode,'U> (get: SelectorGetter -> RecoilValue<'U,'Mode>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> System.Guid.NewGuid()
              "get" ==> get
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )
    /// Derives state and returns a RecoilValue via the provided get function.
    ///
    /// Applies state changes via the provided set function.
    static member inline selector<'T,'Mode,'U> (key: string, get: SelectorGetter -> RecoilValue<'U,_>, set: SelectorMethods -> 'T -> unit) =
        Bindings.Recoil.selector<'U,ReadWrite> (
            [ "key" ==> key
              "get" ==> get
              "set" ==> System.Func<_,_,_>(set) ]
            |> createObj
        )

    /// Returns the value represented by the RecoilValue.
    /// 
    /// If the value is pending, it will throw a Promise to suspend the component.
    /// 
    /// If the value is an error, it will throw it for the nearest React error boundary.
    /// 
    /// This will also subscribe the component for any updates in the value.
    static member inline useValue<'T> (recoilValue: RecoilValue<'T,ReadOnly>) =
        Bindings.Recoil.useRecoilValue<'T,ReadOnly>(recoilValue)

    /// Returns the value represented by the RecoilValue.
    /// 
    /// If the value is pending, it will throw a Promise to suspend the component.
    /// 
    /// if the value is an error it will throw it for the nearest React error boundary.
    /// 
    /// This will also subscribe the component for any updates in the value.
    static member inline useValue<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilValue<'T,ReadWrite>(recoilValue)

    /// Returns the Loadable of a RecoilValue.
    ///
    /// This will also subscribe the component for any updates in the value.
    static member inline useValueLoadable<'T> (recoilValue: RecoilValue<'T,ReadOnly>) =
        Bindings.Recoil.useRecoilValueLoadable<'T,ReadOnly>(recoilValue)

    /// Returns the Loadable of a RecoilValue.
    ///
    /// This will also subscribe the component for any updates in the value.
    static member inline useValueLoadable<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilValueLoadable<'T,ReadWrite>(recoilValue)

    /// Allows the value of the RecoilValue to be read and written.
    /// 
    /// Subsequent updates to the RecoilValue will cause the component to re-render. 
    /// 
    /// If the RecoilValue is pending, this will suspend the compoment and initiate the
    /// retrieval of the value. If evaluating the RecoilValue resulted in an error, this will
    /// throw the error so that the nearest React error boundary can catch it.
    static member inline useState<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilState<'T>(recoilValue)
        |> unbox<'T * ('T -> unit)>

    /// Allows the value of the RecoilValue to be read and written.
    /// 
    /// Subsequent updates to the RecoilValue will cause the component to re-render. 
    /// 
    /// The setter function takes a function that takes the current value and returns 
    /// the new one.
    static member inline useStatePrev<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilState<'T>(recoilValue)
        |> unbox<'T * (('T -> 'T) -> unit)>

    /// Allows the value of the RecoilValue to be read and written.
    /// 
    /// Subsequent updates to the RecoilValue will cause the component to re-render. 
    /// 
    /// Returns a Loadable which can indicate whether the RecoilValue is available, pending, or
    /// unavailable due to an error.
    static member inline useStateLoadable<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilStateLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T> * ('T -> unit)>

    /// Allows the value of the RecoilValue to be read and written.
    /// 
    /// Subsequent updates to the RecoilValue will cause the component to re-render. 
    /// 
    /// Returns a Loadable which can indicate whether the RecoilValue is available, pending, or
    /// unavailable due to an error.
    ///
    /// The setter function takes a function that takes the current value and returns 
    /// the new one.
    static member inline useStateLoadablePrev<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useRecoilStateLoadable<'T>(recoilValue)
        |> unbox<Loadable<'T> * (('T -> 'T) -> unit)>

    /// Returns a function that allows the value of a RecoilValue to be updated, but does
    /// not subscribe the compoment to changes to that RecoilValue.
    static member inline useSetState<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useSetRecoilState<'T>(recoilValue)
        |> unbox<'T -> unit>

    /// Returns a function that allows the value of a RecoilValue to be updated via
    /// a function that accepts the current value and produces the new value.
    static member inline useSetStatePrev<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useSetRecoilState<'T>(recoilValue)
        |> unbox<('T -> 'T) -> unit>

    /// Returns a function that will reset the value of a RecoilValue to its default.
    static member inline useResetState<'T> (recoilValue: RecoilValue<'T,ReadWrite>) =
        Bindings.Recoil.useResetRecoilState<'T>(recoilValue)

    /// Creates a callback function that allows for fetching values of RecoilValue(s).
    static member inline useCallback<'T,'U> (f: (CallbackMethods -> 'T -> 'U), ?deps: obj []) =
        Bindings.Recoil.useRecoilCallback<'T,'U>(System.Func<_,_,_>(f), ?deps = (deps |> Option.map ResizeArray))

    /// Creates a callback function that allows for fetching values of RecoilValue(s),
    /// but will always stay up-to-date with the required depencencies and reduce re-renders.
    ///
    /// This should *not* be used when the callback determines the result of the render.
    static member inline useCallbackRef<'T,'U> (f: (CallbackMethods -> 'T -> 'U)) =
        Bindings.Recoil.useRecoilCallback<'T,'U>(System.Func<_,_,_>(f))
        |> React.useCallbackRef
        
//[<Erase;RequireQualifiedAccess>]
//module Recoil =
    //[<Erase>]
    //type Family =
    //    static member inline atom<'T,'P> (defaultValue: 'P -> 'T) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'P -> 'T) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: JS.Promise<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: 'P -> JS.Promise<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: JS.Promise<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'P -> JS.Promise<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: Async<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> (defaultValue |> Async.StartAsPromise) ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: 'P -> Async<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> (defaultValue >> Async.StartAsPromise) ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: Async<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> (defaultValue |> Async.StartAsPromise) ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'P -> Async<'T>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> (defaultValue >> Async.StartAsPromise) ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: RecoilValue<'T,_>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (defaultValue: 'P -> RecoilValue<'T,_>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: RecoilValue<'T,_>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'P -> RecoilValue<'T,_>) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

[<AutoOpen;Erase>]
module RecoilMagic =
    type Recoil with
        /// Creates a RecoilValue with the given default value.
        /// 
        /// Uses a generated GUID for the key.
        ///
        /// This should not be used if you need data persistence.
        static member inline atom<'T> (defaultValue: 'T) =
            Bindings.Recoil.atom<'T> (
                [ "key" ==> System.Guid.NewGuid()
                  "default" ==> defaultValue ]
                |> createObj
            )
        /// Creates a RecoilValue with the given default value.
        static member inline atom<'T> (key: string, defaultValue: 'T) =
            Bindings.Recoil.atom<'T> (
                [ "key" ==> key
                  "default" ==> defaultValue ]
                |> createObj
            )

        /// Derives state and returns a RecoilValue via the provided get function.
        /// 
        /// Uses a generated GUID for the key.
        ///
        /// This should not be used if you need data persistence.
        static member inline selector<'Mode,'U> (get: SelectorGetter -> 'U) =
            Bindings.Recoil.selector<'U,ReadOnly> (
                [ "key" ==> System.Guid.NewGuid()
                  "get" ==> get ]
                |> createObj
            )
        /// Derives state and returns a RecoilValue via the provided get function.
        static member inline selector<'T,'Mode,'U> (key: string, get: SelectorGetter -> 'U) =
            Bindings.Recoil.selector<'U,ReadOnly> (
                [ "key" ==> key
                  "get" ==> get ]
                |> createObj
            )
        /// Derives state and returns a RecoilValue via the provided get function.
        ///
        /// Applies state changes via the provided set function.
        /// 
        /// Uses a generated GUID for the key.
        ///
        /// This should not be used if you need data persistence.
        static member inline selector<'T,'Mode,'U> (get: SelectorGetter -> 'U, set: SelectorMethods -> 'T -> unit) =
            Bindings.Recoil.selector<'U,ReadWrite> (
                [ "key" ==> System.Guid.NewGuid()
                  "get" ==> get
                  "set" ==> System.Func<_,_,_>(set) ]
                |> createObj
            )
        /// Derives state and returns a RecoilValue via the provided get function.
        ///
        /// Applies state changes via the provided set function.
        static member inline selector<'T,'Mode,'U> (key: string, get: SelectorGetter -> 'U, set: SelectorMethods -> 'T -> unit) =
            Bindings.Recoil.selector<'U,ReadWrite> (
                [ "key" ==> key
                  "get" ==> get
                  "set" ==> System.Func<_,_,_>(set) ]
                |> createObj
            )

        /// Creates a callback function that allows for fetching values of RecoilValue(s).
        static member inline useCallback<'U> (f: (CallbackMethods -> 'U), ?deps: obj []) =
            Bindings.Recoil.useRecoilCallback<unit,'U>(System.Func<_,_>(f), ?deps = (deps |> Option.map ResizeArray))

        /// Creates a callback function that allows for fetching values of RecoilValue(s),
        /// but will always stay up-to-date with the required depencencies and reduce re-renders.
        static member inline useCallbackRef<'U> (f: (CallbackMethods -> 'U)) =
            Bindings.Recoil.useRecoilCallback<unit,'U>(System.Func<_,_>(f))
            |> React.useCallbackRef

    //type Recoil.Family with
    //    static member inline atom<'T,'P> (defaultValue: 'T) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> System.Guid.NewGuid()
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )

    //    static member inline atom<'T,'P> (key: string, defaultValue: 'T) =
    //        Bindings.Recoil.atomFamily<'T,'P> (
    //            [ "key" ==> key
    //              "default" ==> defaultValue ]
    //            |> createObj
    //        )
