# Feliz.Recoil - API Reference

## Types

### RecoilValue

A `RecoilValue` is the resulting type when you create an atom or selector.

It has type type restrictions with the first being the value that the atom/selector
will return after resolution (or just what can be sent when it's a write-only selector), 
and the second designating the access permissions.

There are three permission modifiers:
* ReadOnly - RecoilValues that cannot have their state set.
* ReadWrite - RecoilValues that can be both read and written to. 
Do note that ReadWrite inherits both other types and thus can be used anywhere.
* WriteOnly - RecoilValues that can only be written to. This is rarely needed, but can enable transformations
when you only ever want to modify the source RecoilValue(s).

### SelectorGetter

This is the type that is passed in as a parameter for selectors when fetching a value.

 Definition:
```fs
type SelectorGetter =
    /// Gets the value of a RecoilValue.
    member get (recoilValue: RecoilValue<'T,#ReadOnly>) : 'T
```

### SelectorMethods

This is the type that is passed in as a parameter for selectors when setting a value.

 Definition:
```fs
[<Erase>]
type SelectorMethods =
    /// Gets the value of a RecoilValue.
    member get (recoilValue: RecoilValue<'T,#ReadOnly>) : 'T

    /// Sets the value of a RecoilValue.
    member set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T) : unit
    member set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: DefaultValue) : unit
    member set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T -> 'T) : unit
    member set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T -> DefaultValue) : unit

    /// Sets the value of a RecoilValue back to the default value.
    member _.reset (recoilValue: RecoilValue<'T,#WriteOnly>) : unit = jsNative
```

### Loadable

A `Loadable` is essentially a discriminated union (or as close as JS can manage) 
that represents the different states that a RecoilValue can be in.

 Definition:
```fs
/// Represents the current possible state of a Loadable.
type LoadableState<'T> =
    | HasValue of 'T
    | HasError of exn
    | Loading of JS.Promise<'T>

/// A RecoilValue that may not yet be resolved to a value.
type Loadable<'T> =
    /// Tries to get the async operation of a Loadable.
    member asyncMaybe () : Async<'T> option

    /// Gets the async operation of a Loadable or throws.
    member asyncOrThrow () : Async<'T>

    /// Tries to get the exception of a Loadable.
    member errorMaybe () : exn option

    /// Gets the exception of a Loadable or throws.
    member errorOrThrow () : exn

    /// Gets the result of a Loadable.
    member getValue () : Result<'T,exn>

    /// Maps the value of a Loadable.
    member map (mapping: 'T -> 'U) : Loadable<'U>
    member map (mapping: 'T -> JS.Promise<'U>) : Loadable<'U>
    member map (mapping: 'T -> Async<'U>) : Loadable<'U>
    
    /// Tries to get the promise of a Loadable.
    member promiseMaybe () : JS.Promise<'T> option

    /// Gets the promise of a Loadable or throws.
    member promiseOrThrow () : JS.Promise<'T>

    /// Gets the current state and corresponding value of a Loadable.
    member state () : LoadableState<'T>

    /// Converts the Loadable to an async operation.
    member toAsync () : Async<'T>

    /// Converts the Loadable to a promise.
    member toPromise () : JS.Promise<'T>

    /// Tries to get the value of a Loadable.
    member valueMaybe () : 'T option

    /// Gets the value of a Loadable or throws.
    member valueOrThrow () : 'T
```

### Recoil.defaultValue

This is an empty object that when passed into a setter resets the 
value of the RecoilValue to the original state.

Usage:
```fs
...
Recoil.selector(... fun setter -> setter.set(someAtom, Recoil.defaultValue))
```

### RootInitializer

The RootInitializer type is passed in when performing atom initialization in your
`Recoil.root` component.

Definition:
```fs
type RootInitializer =
    /// Sets the initial value of a single atom to the provided value.
    member set (recoilValue: RecoilValue<'T,#WriteOnly>, currentValue: 'T) : unit
    
    /// Sets the initial value for any number of atoms whose keys are the
    /// keys in the provided map. 
    ///
    /// As with useSetUnvalidatedAtomValues, the validator for each atom will be 
    /// called when it is next read, and setting an atom without a configured 
    /// validator will result in an exception.
    member setUnvalidatedAtomValues (atomValues: Map<string,'T>) : unit
    member setUnvalidatedAtomValues (atomValues: (string * 'T) list) : unit
```

### CallbackMethods

The type passed in when using the `Recoil.useCallback*` hooks.

Definition:
```fs
type CallbackMethods =
    /// Gets the async operation of a RecoilValue.
    member getAsync (recoilValue: RecoilValue<'T,#ReadOnly>) : Async<'T>

    /// Gets the Loadable of a RecoilValue.
    member getLoadable (recoilValue: RecoilValue<'T,#ReadOnly>) : Loadable<'T>

    /// Gets the promise of a RecoilValue.
    member getPromise (recoilValue: RecoilValue<'T,#ReadOnly>) : JS.Promise<'T>

    /// Sets a RecoilValue to the default value.
    member reset (recoilValue: RecoilValue<'T,#WriteOnly>) : unit

    /// Sets a RecoilValue to the given value.
    member set (recoilValue: RecoilValue<'T,#WriteOnly>, newValue: 'T) : unit
    /// Sets a RecoilValue using the updater function.
    member set (recoilValue: RecoilValue<'T,#WriteOnly>, updater: 'T -> 'T) : unit
```
