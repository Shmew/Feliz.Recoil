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

Atoms optionally take a key, which represents a unique ID of the atom.

If you do not specify the key, a generated GUID will be used instead.
Please note that if you need *persistence* the unnamed overloads *should
not be used*.

Signature:
```fs
(defaultValue: 'T) -> RecoilValue<'T,ReadWrite>
(defaultValue: JS.Promise<'T>) -> RecoilValue<'T,ReadWrite>
(defaultValue: Async<'T>) -> RecoilValue<'T,ReadWrite>
(defaultValue: RecoilValue<'T,'Mode>) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: 'T) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: JS.Promise<'T>) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: Async<'T>) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: RecoilValue<'T,'Mode>) -> RecoilValue<'T,ReadWrite>
```

Usage:
```fs
let myText = Recoil.atom("some text")
```

### Recoil.defaultValue

This is an empty object that when passed into a setter resets the 
value of the RecoilValue to the original state.

Usage:
```fs
...
Recoil.selector(... fun setter -> setter.set(someAtom, Recoil.defaultValue))
```

### Recoil.root

Provides the context in which atoms have values. 

Must be an ancestor of any component that uses any Recoil hooks. 

Multiple roots may co-exist; atoms will have distinct values 
within each root. If they are nested, the innermost root will 
completely mask any outer roots.

Signature:
```fs
(children: ReactElement list) -> ReactElement

(initializer: RecoilValue<'T,ReadWrite> -> 'T -> unit, 
 children: ReactElement list) -> ReactElement
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

Like an atom, it also optionally takes a key, where if not provided, a 
GUID is generated. Please note that if you need *persistence* the 
unnamed overloads *should not be used*.

When a setter is not provided the selector is *ReadOnly*. If you try to 
use hooks like `Recoil.useState` you will get compiler errors.

Signature:
```fs
(get: SelectorGetter -> 'U) -> RecoilValue<'U,ReadOnly>
(get: SelectorGetter -> JS.Promise<'U>) -> RecoilValue<'U,ReadOnly>
(get: SelectorGetter -> Async<'U>) -> RecoilValue<'U,ReadOnly>
(get: SelectorGetter -> RecoilValue<'U,_>)  -> RecoilValue<'U,ReadOnly>

(key: string, get: SelectorGetter -> 'U) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> JS.Promise<'U>) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> Async<'U>) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> RecoilValue<'U,_>) -> RecoilValue<'U,ReadOnly>

(get: SelectorGetter -> 'U, set: SelectorMethods -> 'T -> unit) -> RecoilValue<'U,ReadWrite>
(get: SelectorGetter -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit) -> RecoilValue<'U,ReadWrite>
(get: SelectorGetter -> Async<'U>, set: SelectorMethods -> 'T -> unit) -> RecoilValue<'U,ReadWrite>
(get: SelectorGetter -> RecoilValue<'U,_>, set: SelectorMethods -> 'T -> unit) -> RecoilValue<'U,ReadWrite>

(key: string, get: SelectorGetter -> 'U, set: SelectorMethods -> 'T -> unit) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> Async<'U>, set: SelectorMethods -> 'T -> unit) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> RecoilValue<'U,_>, set: SelectorMethods -> 'T -> unit) -> RecoilValue<'U,ReadWrite>
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

### Recoil.useValue

Returns the value represented by the RecoilValue.

If the value is pending, it will throw a Promise to suspend the component.

If the value is an error, it will throw it for the nearest React error boundary.

This will also subscribe the component for any updates in the value.

Signature:
```fs
(recoilValue: RecoilValue<'T,ReadOnly>) -> 'T
(recoilValue: RecoilValue<'T,ReadWrite>) -> 'T
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
(recoilValue: RecoilValue<'T,ReadOnly>) -> 'T
(recoilValue: RecoilValue<'T,ReadWrite>) -> 'T
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
(recoilValue: RecoilValue<'T,ReadWrite>) -> 'T * ('T -> unit)
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
(recoilValue: RecoilValue<'T,ReadWrite>) -> 'T * (('T -> 'T) -> unit)
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
(recoilValue: RecoilValue<'T,ReadWrite>) -> Loadable<'T> * ('T -> unit)
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
(recoilValue: RecoilValue<'T,ReadWrite>) -> Loadable<'T> * (('T -> 'T) -> unit)
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
(recoilValue: RecoilValue<'T,ReadWrite>) -> ('T -> unit)
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
(recoilValue: RecoilValue<'T,ReadWrite>) -> (('T -> 'T) -> unit)
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
(recoilValue: RecoilValue<'T,ReadWrite>) -> (unit -> unit)
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

## Computation Expressions

There are computation expressions for both atoms and selectors, to help make composition easier.

### atom

The atom computation expression only has two operations: `key` and `def`.

They are used like so:

```fs
let myAtom = 
    atom {
        key "myAtomKey"
        def "myDefaultValue"
    }
```

The key and default operations support all the overloads that `Recoil.atom` does.

### selector

The selector computation expression has three operations: 

`key`, `get`, and `set`.

`key` is self explainatory, it sets the key (if you want) for the selector.

`get` is when you want to use the getter function format.

`set` is when you want to use the setter function format.

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
