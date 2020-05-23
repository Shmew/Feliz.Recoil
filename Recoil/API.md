# Feliz.Recoil - API Reference

## Recoil Types

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

## Recoil

Most of the API is accessible via the `Recoil` type, with the only exceptions
being computation expressions.

### Recoil.atom

Creates a RecoilValue with the given default value.

**Keys mut be unique across the application!**

Signature:
```fs
(key: string, defaultValue: 'T, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: JS.Promise<'T>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: Async<'T>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: RecoilValue<'T,_>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) -> RecoilValue<'T,ReadWrite>
```

Usage:
```fs
let myText = Recoil.atom("myAtomKey", "some text")
```

### Recoil.defaultValue

This is an empty object that when passed into a setter resets the 
value of the RecoilValue to the original state.

Usage:
```fs
...
Recoil.selector(... fun setter -> setter.set(someAtom, Recoil.defaultValue))
```

### Recoil.logger

Enables console debugging when in development.

Similar to `React.strictMode`, this will do nothing
in production.

This must be a descendant of a `Recoil.root` component.

If you have persistence settings already enabled, they will
already log, this below usage is for non-persisted atoms.

Usage:
```fs

let myAtom = 
    Recoil.atom (
        "myAtomKey", 
        1, 
        { Type = PersistenceType.Log
          Backbutton = None
          Validator = (fun _ -> None) }
    )

// or

let myOtherAtom =
    atom {
        key "myOtherAtomKey"
        def 1
        log
    }

...
Recoil.root [
    Recoil.logger()
    ...
]
```

### Recoil.root

Provides the context in which atoms have values. 

Must be an ancestor of any component that uses any Recoil hooks. 

Multiple roots may co-exist; atoms will have distinct values 
within each root. If they are nested, the innermost root will 
completely mask any outer roots.

The initilizer parameter is a function that will be called when 
the root is first rendered, which can set initial values for atoms.

Signature:
```fs
type RootInitializer =
    /// Sets the initial value of a single atom to the provided value.
    member set (recoilValue: RecoilValue<'T,ReadWrite>, currentValue: 'T) : unit
    /// Sets the initial value for any number of atoms whose keys are the
    /// keys in the provided map. 
    ///
    /// As with useSetUnvalidatedAtomValues, the validator for each atom will be 
    /// called when it is next read, and setting an atom without a configured 
    /// validator will result in an exception.
    member setUnvalidatedAtomValues (atomValues: Map<string,'T>) : unit
    member setUnvalidatedAtomValues (atomValues: (string * 'T) list) : unit

(children: ReactElement list) -> ReactElement

(initializer: RootInitializer -> unit, children: ReactElement list)
    -> ReactElement
```

Usage:
```fs
let myComp = React.functionComponent(fun () ->
    Recoil.root [
        Html.div [
            ...
        ]
        ...
    ])
```

### Recoil.selector

Derives state and returns a RecoilValue via the provided get function.

**Keys mut be unique across the application!**

When a setter is not provided the selector is *ReadOnly*. If you try to 
use hooks like `Recoil.useState` you will get compiler errors.

Signature:
```fs
(key: string, get: SelectorGetter -> 'U, ?cacheImplementation: CacheImplementation<'U,'U>) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> JS.Promise<'U>, ?cacheImplementation: CacheImplementation<'U,JS.Promise<'U>>) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> Async<'U>, ?cacheImplementation: CacheImplementation<'U,Async<'U>>) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> RecoilValue<'U,_>, ?cacheImplementation: CacheImplementation<'U,RecoilValue<'U,_>>) -> RecoilValue<'U,ReadOnly>

(key: string, get: SelectorGetter -> 'U, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,'U>) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,JS.Promise<'U>>) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> Async<'U>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,Async<'U>>) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> RecoilValue<'U,_>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,RecoilValue<'U,_>>) -> RecoilValue<'U,ReadWrite>
```

Usage:
```fs
let textState = Recoil.atom("textState", "Hello world!")

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    Recoil.selector("textStateTransform", fun getter ->
        getter.get(textState)
        |> String.filter(fun v -> List.contains v vowels)
    )
```

### Recoil.useValue

Returns the value represented by the RecoilValue.

If the value is pending, it will throw a Promise to suspend the component.

If the value is an error, it will throw it for the nearest React error boundary.

This will also subscribe the component for any updates in the value.

Signature:
```fs
(recoilValue: RecoilValue<'T,_>) -> 'T
```

Usage:
```fs
let myAtom = Recoil.atom("some text value")

let myComp = React.functionComponent(fun () ->
    let someText = Recoil.useValue(myAtom)

    Html.div [
        prop.text someText
    ])
```

### Recoil.useValueLoadable

Returns the Loadable of a RecoilValue.

This will also subscribe the component for any updates in the value.

Signature:
```fs
(recoilValue: RecoilValue<'T,_>) -> 'T
```

Usage:
```fs
let someAsyncSelector = 
    Recoil.selector(fun get ->
        async {
            do! Async.Sleep 400
            return "Howdy!"
        }
    )

let mySelectorComp = React.functionComponent(fun () ->
    let textLoadable = Recoil.useValueLoadable(someAsyncSelector)

    match textLoadable.state() with
    | LoadableState.HasValue text ->
        Html.div [
            prop.text text
        ]
    | _ ->
        Html.div [
            prop.text "I'm still loading!"
        ])
```

### Recoil.useState

Allows the value of the RecoilValue to be read and written.

Subsequent updates to the RecoilValue will cause the component to re-render. 

If the RecoilValue is pending, this will suspend the compoment and initiate the
retrieval of the value. If evaluating the RecoilValue resulted in an error, this will
throw the error so that the nearest React error boundary can catch it.

Signature:
```fs
(recoilValue: RecoilValue<'T,ReadWrite> -> 'T * ('T -> unit)
```

Usage:
```fs
let myAtom = Recoil.atom("some text value")

let myComp = React.functionComponent(fun () ->
    let someText,setSomeText = Recoil.useState(myAtom)

    Html.div [
        Html.div [
            prop.text someText
        ]
        Html.button [
            prop.onClick <| fun _ -> setSomeText "I was clicked!"
        ]
    ])
```

### Recoil.useStatePrev

Allows the value of the RecoilValue to be read and written.

Subsequent updates to the RecoilValue will cause the component to re-render. 

The setter function takes a function that takes the current value and returns 
the new one.

Signature:
```fs
(recoilValue: RecoilValue<'T,ReadWrite> -> 'T * (('T -> 'T) -> unit)
```

Usage:
```fs
let myAtom = Recoil.atom(0)

let myComp = React.functionComponent(fun () ->
    let count,setCount = Recoil.useStatePrev(myAtom)

    Html.div [
        Html.div [
            prop.text (sprintf "I was clicked: %i times!" someText)
        ]
        Html.button [
            prop.onClick <| fun _ -> setCount (fun i -> i + 1)
        ]
    ])
```

### Recoil.useStateLoadable

Allows the value of the RecoilValue to be read and written.

Subsequent updates to the RecoilValue will cause the component to re-render. 

Returns a Loadable which can indicate whether the RecoilValue is available, pending, or
unavailable due to an error.

Signature:
```fs
(recoilValue: RecoilValue<'T,ReadWrite> -> Loadable<'T> * ('T -> unit)
```

Usage:
```fs
let someAsyncSelector = 
    Recoil.selector(fun get ->
        async {
            do! Async.Sleep 400
            return "Howdy!"
        }
    )

let mySelectorComp = React.functionComponent(fun () ->
    let textLoadable,setTextLoadable = Recoil.useStateLoadable(someAsyncSelector)

    match textLoadable.state() with
    | LoadableState.HasValue text ->
        Html.div [
            Html.div [
                prop.text text
            ]
            Html.button [
                prop.onClick <| fun _ -> setTextLoadable "Bonjour!"
                prop.text "Click me!"
            ]
        ]
    | _ ->
        Html.div [
            prop.text "I'm still loading!"
        ])
```

### Recoil.useStateLoadablePrev

Allows the value of the RecoilValue to be read and written.

Subsequent updates to the RecoilValue will cause the component to re-render. 

The setter function takes a function that takes the current value and returns 
the new one.

Signature:
```fs
(recoilValue: RecoilValue<'T,ReadWrite> -> Loadable<'T> * (('T -> 'T) -> unit)
```

Usage:
```fs
let someAsyncSelector = 
    Recoil.selector(fun get ->
        async {
            do! Async.Sleep 400
            return "Howdy!"
        }
    )

let mySelectorComp = React.functionComponent(fun () ->
    let textLoadable,setTextLoadable = Recoil.useStateLoadable(someAsyncSelector)

    match textLoadable.state() with
    | LoadableState.HasValue text ->
        Html.div [
            Html.div [
                prop.text text
            ]
            Html.button [
                prop.onClick <| fun _ -> setTextLoadable (fun current -> current + "Bonjour!")
                prop.text "Click me!"
            ]
        ]
    | _ ->
        Html.div [
            prop.text "I'm still loading!"
        ])
```

### Recoil.useSetState

Returns a function that allows the value of a RecoilValue to be updated, but does
not subscribe the compoment to changes to that RecoilValue.

Signature:
```fs
(recoilValue: RecoilValue<'T,ReadWrite> -> ('T -> unit)
```

Usage:
```fs
let myAtom = Recoil.atom("some text value")

let myComp = React.functionComponent(fun () ->
    let setSomeText = Recoil.useSetState(myAtom)

    Html.div [
        Html.button [
            prop.onClick <| fun _ -> setSomeText "I was clicked!"
        ]
    ])
```

### Recoil.useSetStatePrev

Returns a function that allows the value of a RecoilValue to be updated, but does
not subscribe the compoment to changes to that RecoilValue.

Signature:
```fs
(recoilValue: RecoilValue<'T,ReadWrite> -> (('T -> 'T) -> unit)
```

Usage:
```fs
let myAtom = Recoil.atom("some text value")

let myComp = React.functionComponent(fun () ->
    let setSomeText = Recoil.useSetStatePrev(myAtom)

    Html.div [
        Html.button [
            prop.onClick <| fun _ -> setSomeText(fun current -> current + "I was clicked!")
        ]
    ])
```

### Recoil.useResetState

Returns a function that will reset the value of a RecoilValue to its default.

Signature:
```fs
(recoilValue: RecoilValue<'T,ReadWrite> -> (unit -> unit)
```

Usage: Usage: See the [reset example](https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Reset).

### Recoil.useCallback

Creates a callback function that allows for fetching values of RecoilValue(s).

Signature:
```fs
(CallbackMethods -> 'U), ?deps: obj []) -> ('T -> 'U)
(f: (CallbackMethods -> 'T -> 'U), ?deps: obj []) -> ('T -> 'U)
```

Usage: See the [callback example](https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Callback).

### Recoil.useCallbackRef

Creates a callback function that allows for fetching values of RecoilValue(s),
but will always stay up-to-date with the required depencencies and reduce re-renders.

This should *not* be used when the callback determines the result of the render.

Signature:
```fs
(f: (CallbackMethods -> 'U)) -> ('T -> 'U)
(f: (CallbackMethods -> 'T -> 'U), ?deps: obj []) -> ('T -> 'U)
```

Usage: Same usage as this [callback example](https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Callback)
, just replace the function name.

### Recoil.useSetUnvalidatedAtomValues

**This API is unstable.**

Sets the initial value for any number of atoms whose keys are the
keys in the provided key-value list. 

As with useSetUnvalidatedAtomValues, the validator for each atom will be 
called when it is next read, and setting an atom without a configured 
validator will result in an exception.

TransactionMetadata should should be a record or anonymous record mapping
atom/selector keys to the data you want to set alongside them.

Signature:
```fs
(values: Map<string, 'Value>, ?transactionMetadata: 'Metadata) -> unit
(values: (string * 'Value) list, ?transactionMetadata: 'Metadata) -> unit
```

### Recoil.useTransactionObservation

**This API is unstable.**

Calls the given callback after any atoms have been modified and the consequent
component re-renders have been committed. This is intended for persisting
the values of the atoms to storage. The stored values can then be restored
using the useSetUnvalidatedAtomValues hook.

The callback receives the following info:

atomValues: 
The current value of every atom that is both persistable (persistence
type not set to 'none') and whose value is available (not in an
error or loading state).

previousAtomValues: 
The value of every persistable and available atom before the transaction began.

atomInfo: 
A map containing the persistence settings for each atom. Every key
that exists in atomValues will also exist in atomInfo.

modifiedAtoms: The set of atoms that were written to during the transaction.

transactionMetadata: 
Arbitrary information that was added via the useSetUnvalidatedAtomValues hook. 

Useful for ignoring the useSetUnvalidatedAtomValues transaction, to avoid loops.

Signature:
```fs
(callback: TransactionObservation<'Values,'Metadata> -> unit) -> unit
```

### Recoil.useSetUnvalidatedAtomValues

**This API is unstable.**

Subscribes to the store.

Signature:
```fs
(callback: Store<'T> * TreeState<'T> -> 'U) -> 'U
```

## Computation Expressions

There are computation expressions for both atoms and selectors, to help make composition easier.

### atom

The atom computation expression has four operations: 
* `key` - The atom key.
* `def` - The default value.
* `log` - Enables logging when using the `Recoil.logger` component.
* `persist` - Allows modifications to atom persistence settings.

They are used like so:

```fs
let myAtom = 
    atom {
        key "myAtomKey"
        def "myDefaultValue"
        log
    }
```

The key and default operations support all the overloads that `Recoil.atom` does.

### selector

The selector computation expression has three operations: 

* `key` - Sets the key for the selector.
* `get` - Sets the function to get a value.
* `set` - Sets the function to set a value.
* `cache` - Sets the `CacheImplementation<'T>` interface.

These can be used like so:

```fs
let mySelector =
    selector {
        key "mySelectorKey"
        get (fun get ->
            let myTextValue = get(myAtom)
            myTextValue + " and some more text"
        )
        set (fun setter ->
            let myTextValue = setter.get(myAtom)
            setter.set(myAtom, myTextValue + " and some other text")
        )
    }
```
