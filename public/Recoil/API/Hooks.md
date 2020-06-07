# Feliz.Recoil - API Reference

## Hooks

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
(f: CallbackMethods -> 'T -> 'U, ?deps: obj []) -> ('T -> 'U)
(f: CallbackMethods -> 'T -> 'U -> 'V, ?deps: obj []) -> ('T -> 'U -> 'V)
(f: CallbackMethods -> 'T -> 'U -> 'V -> 'W, ?deps: obj []) -> ('T -> 'U -> 'V -> 'W)
...
```

Usage: See the [callback example](https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Callback).

### Recoil.useCallbackRef

Creates a callback function that allows for fetching values of RecoilValue(s),
but will always stay up-to-date with the required depencencies and reduce re-renders.

This should *not* be used when the callback determines the result of the render.

Signature:
```fs
(f: (CallbackMethods -> 'T -> 'U) -> ('T -> 'U)
(f: (CallbackMethods -> 'T -> 'U -> 'V) -> ('T -> 'U -> 'V)
(f: (CallbackMethods -> 'T -> 'U -> 'V -> 'W) -> ('T -> 'U -> 'V -> 'W)
...
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
