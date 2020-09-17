namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open System.ComponentModel

[<Measure>]
type SnapshotId

/// Marker interface to designate read only RecoilValues.
type ReadOnly = interface end

/// Marker interface to designate write only RecoilValues.
type WriteOnly = interface end

/// Marker interface to designate read and writable RecoilValues.
type ReadWrite = 
    inherit ReadOnly
    inherit WriteOnly

[<Erase>]
type RecoilValue<'T,'ReadPerm> =
    [<Emit("$0.key")>]
    member _.key : string = jsNative

[<EditorBrowsable(EditorBrowsableState.Never)>]
type DefaultValue = interface end

/// Methods provided in selectors for reading RecoilValues.
[<Erase>]
type SelectorGetter =
    /// Gets the value of a RecoilValue.
    [<Emit("$0.get($1)")>]
    member _.get (recoilValue: RecoilValue<'T, #ReadOnly>) : 'T = jsNative

/// Methods provided in selectors for composing new RecoilValues.
[<Erase>]
type SelectorMethods =
    /// Gets the value of a RecoilValue.
    [<Emit("$0.get($1)")>]
    member _.get (recoilValue: RecoilValue<'T, #ReadOnly>) : 'T = jsNative

    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: DefaultValue) : unit = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: U2<'T,DefaultValue>) : unit = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T -> 'T) : unit = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T -> DefaultValue) : unit = jsNative
    /// Sets the value of a RecoilValue.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T -> U2<'T,DefaultValue>) : unit = jsNative

    /// Sets the value of a RecoilValue back to the default value.
    [<Emit("$0.reset($1)")>]
    member _.reset (recoilValue: RecoilValue<'T,#WriteOnly>) : unit = jsNative

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
module SelectorMagic =
    type SelectorMethods with
        /// Sets the value of a RecoilValue.
        [<Emit("$0.set($1, $2)")>]
        member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T) : unit = jsNative

[<EditorBrowsable(EditorBrowsableState.Never)>]
[<StringEnum;RequireQualifiedAccess>]
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
    member inline this.asyncMaybe () =
        this.promiseMaybe() |> Option.map Async.AwaitPromise
    /// Gets the async operation of a Loadable or throws.
    member inline this.asyncOrThrow () =
        this.promiseOrThrow() |> Async.AwaitPromise

    /// Tries to get the exception of a Loadable.
    [<Emit("$0.errorMaybe()")>]
    member _.errorMaybe () : exn option = jsNative

    /// Gets the exception of a Loadable or throws.
    [<Emit("$0.errorOrThrow()")>]
    member _.errorOrThrow () : exn = jsNative

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.getValue()")>]
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

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.contents")>]
    member _.contents : U3<'T,exn,JS.Promise<ResolvedLoadablePromiseInfo<'T>>> = jsNative

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.state")>]
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

    /// Converts the Loadable to an async operation.
    member inline this.toAsync () =
        this.toPromise'() 
        |> Promise.map(fun o -> o?value)
        |> Async.AwaitPromise

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.toPromise()")>]
    member _.toPromise' () : JS.Promise<obj> = jsNative

    /// Converts the Loadable to a promise.
    member inline this.toPromise () =
        this.toPromise'()
        |> Promise.map(fun o -> o?value)

    /// Tries to get the value of a Loadable.
    [<Emit("$0.valueMaybe()")>]
    member _.valueMaybe () : 'T option = jsNative

    /// Gets the value of a Loadable or throws.
    [<Emit("$0.valueOrThrow()")>]
    member _.valueOrThrow () : 'T = jsNative

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
module LoadableMagic =
    type Loadable<'T> with
        /// Maps the value of a Loadable.
        [<Emit("$0.map($1)")>]
        member _.map (mapping: 'T -> 'U) : Loadable<'U> = jsNative

/// An immutable snapshot of the global recoil state.
[<Erase>]
type Snapshot =
    /// Returns a Loadable for the given recoil value.
    [<Emit("$0.getLoadable($1)")>]
    member _.getLoadable (recoilValue: RecoilValue<'T,#ReadOnly>) : Loadable<'T> = jsNative

    /// Returns a unique int that represents the snapshot instance.
    [<Emit("$0.getID()")>]
    member _.getId () : int<SnapshotId> = jsNative

    /// Returns a promise which will resolve to the value of the given recoil value.
    [<Emit("$0.getPromise($1)")>]
    member _.getPromise (recoilValue: RecoilValue<'T,#ReadOnly>) : JS.Promise<'T> = jsNative

    /// Returns an async which will resolve to the value of the given recoil value.
    member inline this.getAsync (recoilValue: RecoilValue<'T,#ReadOnly>) = 
        this.getPromise(recoilValue) |> Async.AwaitPromise

    /// Creates a new snapshot by calling the provided mapper function.
    [<Emit("$0.map($1)")>]
    member _.map (mapper: MutableSnapshot -> unit) : Snapshot = jsNative
    /// Creates a new snapshot by calling the provided mapper function.
    [<Emit("$0.asyncMap($1)")>]
    member _.map (mapper: MutableSnapshot -> JS.Promise<unit>) : JS.Promise<Snapshot> = jsNative
    /// Creates a new snapshot by calling the provided mapper function.
    member inline this.map (mapper: MutableSnapshot -> Async<unit>) =
        this.map(mapper >> Async.StartAsPromise)
        |> Async.AwaitPromise

/// A Snapshot that can be modified.
///
/// These modify the snapshot, *not the global state*.
and [<Erase>] MutableSnapshot =
    inherit Snapshot

    /// Sets the value of a RecoilValue to the default state.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: DefaultValue) : unit = jsNative
    /// Sets the value of a RecoilValue using an updater 
    /// function that provides the previous value.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T -> 'T) : unit = jsNative
    /// Sets the value of a RecoilValue to the default state using an 
    /// updater function that provides the previous value.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T -> DefaultValue) : unit = jsNative

    /// Sets the value of a RecoilValue back to the default value.
    [<Emit("$0.reset($1)")>]
    member _.reset (recoilValue: RecoilValue<'T,#WriteOnly>) : unit = jsNative

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
module MutableSnapshotMagic =
    type MutableSnapshot with
        /// Sets the value of a RecoilValue.
        [<Emit("$0.set($1, $2)")>]
        member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T) : unit = jsNative

[<Erase;RequireQualifiedAccess>]
module Snapshot =
    /// Returns an async which will resolve to the value of the given recoil value.
    let inline getAsync (recoilValue: RecoilValue<'T,#ReadOnly>) (snapshot: Snapshot) = 
        snapshot.getAsync(recoilValue)

    /// Returns a unique int that represents the snapshot instance.
    let inline getId (snapshot: Snapshot) = snapshot.getId()

    /// Returns a Loadable for the given recoil value.
    let inline getLoadable (recoilValue: RecoilValue<'T,#ReadOnly>) (snapshot: Snapshot) = 
        snapshot.getLoadable(recoilValue)

    /// Returns a promise which will resolve to the value of the given recoil value.
    let inline getPromise (recoilValue: RecoilValue<'T,#ReadOnly>) (snapshot: Snapshot) = 
        snapshot.getPromise(recoilValue)

    /// Creates a new snapshot by calling the mapper function on the snapshot.
    let inline map (mapper: MutableSnapshot -> unit) (snapshot: Snapshot) =
        snapshot.map(mapper)
        
    /// Creates a new snapshot by calling the mapper function on the snapshot.
    let inline mapAsync (mapper: MutableSnapshot -> Async<unit>) (snapshot: Snapshot) =
        snapshot.map(mapper)
        
    /// Creates a new snapshot by calling the mapper function on the snapshot.
    let inline mapPromise (mapper: MutableSnapshot -> JS.Promise<unit>) (snapshot: Snapshot) =
        snapshot.map(mapper)

[<Erase>]
type SnapshotObservation =
    /// The current snapshot.
    [<Emit("$0.snapshot")>]
    member _.snapshot : Snapshot = jsNative

    /// The previous snapshot.
    [<Emit("$0.previousSnapshot")>]
    member _.previousSnapshot : Snapshot = jsNative

[<Erase>]
type CallbackMethods =
    /// Goes to the given snapshot.
    [<Emit("$0.gotoSnapshot($1)")>]
    member _.gotoSnapshot (snapshot: Snapshot) : unit = jsNative

    /// Sets a RecoilValue to the default value.
    [<Emit("$0.reset($1)")>]
    member _.reset (recoilValue: RecoilValue<'T,#WriteOnly>) : unit = jsNative

    /// Sets a RecoilValue using the updater function.
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, updater: 'T -> 'T) : unit = jsNative

    /// The current snapshot.
    [<Emit("$0.snapshot")>]
    member _.snapshot : Snapshot = jsNative

[<AutoOpen;Erase;EditorBrowsable(EditorBrowsableState.Never)>]
module CallbackMagic =
    type CallbackMethods with
        /// Sets a RecoilValue to the given value.
        [<Emit("$0.set($1, $2)")>]
        member _.set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T) : unit = jsNative

[<EditorBrowsable(EditorBrowsableState.Never)>]
[<StringEnum;RequireQualifiedAccess>]
type PersistenceTypeWithNone =
    | LocalStorage
    | Log
    | None
    | SessionStorage
    | Url

[<StringEnum;RequireQualifiedAccess>]
type PersistenceType =
    | LocalStorage
    | Log
    | SessionStorage
    | Url

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    static member inline fromTypeWIthNone (pType: PersistenceTypeWithNone) =
        match pType with
        | PersistenceTypeWithNone.None -> None
        | PersistenceTypeWithNone.Log -> Some PersistenceType.Log
        | PersistenceTypeWithNone.LocalStorage -> Some PersistenceType.LocalStorage
        | PersistenceTypeWithNone.SessionStorage -> Some PersistenceType.SessionStorage
        | PersistenceTypeWithNone.Url -> Some PersistenceType.Url

[<Erase>]
type PersistenceInfo =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.type")>]
    member _.typeInner : PersistenceTypeWithNone = jsNative
    
    member inline this.type' =
        PersistenceType.fromTypeWIthNone(this.typeInner)

    [<Emit("$0.backButton")>]
    member _.backButton : bool = jsNative

[<NoComparison;NoEquality>]
type PersistenceSettings<'T,'U> =
    { Type: PersistenceType
      Backbutton: bool option 
      Validator: ('U -> 'T option) }

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    static member inline CreateObj (settings: PersistenceSettings<'T,'U>) =
        [ "type" ==> settings.Type
          if settings.Backbutton.IsSome then
              "backbutton" ==> settings.Backbutton.Value
          "validator" ==> 
              (fun (v: 'U, def: DefaultValue) ->
                  match settings.Validator(v) with
                  | Some res -> box res
                  | None -> box def) ]
        |> createObj

[<Erase>]
type AtomInfo =
    [<Emit("$0.persistence_UNSTABLE")>]
    member inline _.persistence : PersistenceInfo = jsNative

[<Erase>]
type TransactionObservation<'Values,'Metadata> =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.atomValues")>]
    member _.atomValues' : JS.Map<string,'Values> = jsNative
    /// The current value of every atom that is both persistable (persistence
    /// type not set to 'none') and whose value is available (not in an
    /// error or loading state).
    member inline this.atomValues = 
        this.atomValues'.entries() |> Map.ofSeq
        
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.previousAtomValues")>]
    member _.previousAtomValues' : JS.Map<string,'Values> = jsNative
    /// The value of every persistable and available atom before
    /// the transaction began.
    member inline this.previousAtomValues = 
        this.previousAtomValues'.entries() |> Map.ofSeq
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.atomInfo")>]
    member _.atomInfo' : JS.Map<string,AtomInfo> = jsNative
    /// A map containing the persistence settings for each atom. Every key
    /// that exists in atomValues will also exist in atomInfo.
    member inline this.atomInfo = 
        this.atomInfo'.entries() |> Map.ofSeq
    
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.modifiedAtoms")>]
    member _.modifiedAtoms' : JS.Set<string> = jsNative
    /// The set of atoms that were written to during the transaction.
    member inline this.modifiedAtoms = 
        this.modifiedAtoms'.entries() |> Set.ofSeq

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("Object.entries($0.transactionMetadata)")>]
    member _.transactionMetadata' : ResizeArray<string * 'Metadata> = jsNative
    /// Arbitrary information that was added via the
    /// useSetUnvalidatedAtomValues hook. Useful for ignoring the useSetUnvalidatedAtomValues
    /// transaction, to avoid loops.
    member inline this.getTransactionMetadata () =
        this.transactionMetadata'
        |> Map.ofSeq

type CacheImplementation<'T,'U> =
    abstract get: 'U -> 'T option
    abstract set: 'U -> 'T -> CacheImplementation<'T,'U>
