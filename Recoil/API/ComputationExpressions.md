# Feliz.Recoil - API Reference

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

### recoil

A standard computation expression to make binding values easier.


Usage:
```fs
let textState = Recoil.atom("textState", "Hello world!")

let otherTextState = Recoil.atom("otherTextState", "")

let textStateTransformed =
    recoil {
        let! text = 
            textState
            |> RecoilValue.map(fun s -> s + " wow")

        let! otherText = otherTextState

        return
            selector {
                key "textStateTransformed"
                get (fun _ ->
                    if otherText = "" then text
                    else sprintf "%s - %s" text otherText)
            }
    }
```
