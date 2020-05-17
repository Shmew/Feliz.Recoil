namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open System.ComponentModel

[<EditorBrowsable(EditorBrowsableState.Never)>]
[<StringEnum(CaseRules.None);RequireQualifiedAccess>]
type RecoilValueTag =
    | Writeable
    | Readonly

/// Marker interface to designate read only RecoilValues.
type ReadOnly = interface end

/// Marker interface to designate mutable RecoilValues.
type ReadWrite = interface end

[<Erase>]
type RecoilValue<'T,'ReadPerm> =
    [<Emit("$0.key")>]
    member _.key : string = jsNative
    [<Emit("$0.tag"); EditorBrowsable(EditorBrowsableState.Never)>]
    member _.tag : RecoilValueTag = jsNative

[<EditorBrowsable(EditorBrowsableState.Never)>]
type DefaultValue = interface end

/// Methods provided in selectors for composing new RecoilValues.
[<Erase>]
type SelectorMethods =
    /// Gets the value of a RecoilValue.
    [<Emit("$0.get($1)")>]
    member _.get (recoilValue: RecoilValue<'T,_>) : 'Return = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: DefaultValue) : unit = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T -> 'T) : unit = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T -> DefaultValue) : unit = jsNative
    /// Sets the value of a RecoilValue back to the default value.
    [<Emit("$0.reset($1)")>]
    member _.reset (recoilValue: RecoilValue<'T,ReadWrite>) : unit = jsNative

[<AutoOpen;Erase>]
module SelectorMagic =
    type SelectorMethods with
        /// Sets the value of a RecoilValue.
        [<Emit("$0.set($1, $2)")>]
        member _.set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T) : unit = jsNative

[<StringEnum;RequireQualifiedAccess;EditorBrowsable(EditorBrowsableState.Never)>]
type LoadableStateStr =
    | HasValue
    | HasError
    | Loading

/// Represents the current possible state of a Loadable.
type LoadableState<'T> =
    | HasValue of 'T
    | HasError of exn
    | Loading of JS.Promise<'T>

[<EditorBrowsable(EditorBrowsableState.Never)>]
type ResolvedLoadablePromiseInfo<'T> =
    abstract value: 'T

/// A RecoilValue that may not yet be resolved to a value.
[<Erase>]
type Loadable<'T> =
    /// Tries to get the async operation of a Loadable.
    member inline this.asyncMaybe () : Async<'T> option =
        this.promiseMaybe() |> Option.map Async.AwaitPromise
    /// Gets the async operation of a Loadable or throws.
    member inline this.asyncOrThrow () : Async<'T> =
        this.promiseOrThrow() |> Async.AwaitPromise

    /// Tries to get the exception of a Loadable.
    [<Emit("$0.errorMaybe()")>]
    member _.errorMaybe () : exn option = jsNative

    /// Gets the exception of a Loadable or throws.
    [<Emit("$0.errorOrThrow()")>]
    member _.errorOrThrow () : exn = jsNative

    [<Emit("$0.getValue()");EditorBrowsable(EditorBrowsableState.Never)>]
    member _.getValue' () : 'T = jsNative
    /// Gets the result of a Loadable.
    member inline this.getValue () = 
        try this.getValue'() |> Ok
        with e -> Error e

    /// Maps the value of a Loadable.
    [<Emit("$0.map($1)")>]
    member _.map (mapping: 'T -> JS.Promise<'U>) : Loadable<'U> = jsNative
    /// Maps the value of a Loadable.
    member inline this.map (mapping: 'T -> Async<'U>) =
        let test = fun o -> o |> mapping |> Async.StartAsPromise
        this.map(test)
    
    /// Tries to get the promise of a Loadable.
    [<Emit("$0.promiseMaybe()")>]
    member _.promiseMaybe () : JS.Promise<'T> option = jsNative

    /// Gets the promise of a Loadable or throws.
    [<Emit("$0.promiseOrThrow()")>]
    member _.promiseOrThrow () : JS.Promise<'T> = jsNative

    [<Emit("$0.state");EditorBrowsable(EditorBrowsableState.Never)>]
    member _.state' : LoadableStateStr = jsNative
    /// Gets the current state and corresponding value of a Loadable.
    member inline this.state () =
        match this.state' with
        | LoadableStateStr.Loading ->
            unbox<JS.Promise<ResolvedLoadablePromiseInfo<'T>>> this.contents
            |> Promise.map (fun o -> o.value)
            |> LoadableState.Loading 
        | LoadableStateStr.HasError ->
            LoadableState.HasError (unbox<exn> this.contents)
        | LoadableStateStr.HasValue ->
            LoadableState.HasValue (unbox<'T> this.contents)

    [<Emit("$0.contents");EditorBrowsable(EditorBrowsableState.Never)>]
    member _.contents : U3<'T,exn,JS.Promise<ResolvedLoadablePromiseInfo<'T>>> = jsNative

    /// Converts the Loadable to an async operation.
    member inline this.toAsync () : Async<'T> =
        this.toPromise'() 
        |> Promise.map(fun o -> o?value)
        |> Async.AwaitPromise

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.toPromise()")>]
    member _.toPromise' () : JS.Promise<obj> = jsNative

    /// Converts the Loadable to a promise.
    member inline this.toPromise () : JS.Promise<'T> =
        this.toPromise'()
        |> Promise.map(fun o -> o?value)

    /// Tries to get the value of a Loadable.
    [<Emit("$0.valueMaybe()")>]
    member _.valueMaybe () : 'T option = jsNative

    /// Gets the value of a Loadable or throws.
    [<Emit("$0.valueOrThrow()")>]
    member _.valueOrThrow () : 'T = jsNative

[<AutoOpen;Erase>]
module LoadableMagic =
    type Loadable<'T> with
        /// Maps the value of a Loadable.
        [<Emit("$0.map($1)")>]
        member _.map (mapping: 'T -> 'U) : Loadable<'U> = jsNative

[<EditorBrowsable(EditorBrowsableState.Never)>]
type RootInitializer =
    abstract set<'T> : RecoilValue<'T,ReadWrite> -> 'T -> unit
    abstract setUnvalidatedAtomValues : JS.Map<string,obj> -> unit

[<Erase>]
type CallbackMethods =
    /// Gets the async operation of a RecoilValue.
    member inline this.getAsync<'T,'Mode> (recoilValue: RecoilValue<'T,'Mode>) = 
        this.getPromise<'T,'Mode>(recoilValue) |> Async.AwaitPromise

    /// Gets the Loadable of a RecoilValue.
    [<Emit("$0.getLoadable($1)")>]
    member _.getLoadable<'T,'Mode> (recoilValue: RecoilValue<'T,'Mode>) : Loadable<'T> = jsNative

    /// Gets the promise of a RecoilValue.
    [<Emit("$0.getPromise($1)")>]
    member _.getPromise<'T,'Mode> (recoilValue: RecoilValue<'T,'Mode>) : JS.Promise<'T> = jsNative

    /// Sets a RecoilValue to the default value.
    [<Emit("$0.reset($1)")>]
    member _.reset<'T> (recoilValue: RecoilValue<'T,ReadWrite>) : unit = jsNative

    /// Sets a RecoilValue using the updater function.
    [<Emit("$0.set($1, $2)")>]
    member _.set<'T> (recoilValue: RecoilValue<'T,ReadWrite>, updater: 'T -> 'T) : unit = jsNative

[<AutoOpen;Erase>]
module CallbackMagic =
    type CallbackMethods with
        /// Sets a RecoilValue to the given value.
        [<Emit("$0.set($1, $2)")>]
        member _.set<'T> (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T) : unit = jsNative
