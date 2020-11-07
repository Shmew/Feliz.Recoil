# Feliz.Recoil - API Reference

## Computation Expressions

There are computation expressions for both atoms and selectors, to help make composition easier.

### atom

The atom computation expression has five operations: 

* `key` - The atom key.
* `def` - The default value.
* `effect` - Adds an AtomEffect.
* `log` - Enables logging when using the `Recoil.logger` component.
* `persist` - Allows modifications to atom persistence settings.
* `dangerouslyAllowMutability` - Prevents object deep freezing, *use at your own risk!*

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

### atomFamily

The atomFamily computation expression has five operations:

* `key` - The atom key.
* `def` - The default value.
* `effect` - Adds an AtomEffect.
* `log` - Enables logging when using the `Recoil.logger` component.
* `persist` - Allows modifications to atom persistence settings.
* `dangerouslyAllowMutability` - Prevents object deep freezing, *use at your own risk!*

They are used like so:

```fs
let myAtom = 
    atomFamily {
        key "myAtomKey"
        def (fun s -> "myDefaultValue" + s)
        log
    }
```

### selector

The selector computation expression has six operations: 

* `key` - Sets the key for the selector.
* `get` - Sets the function to get a value.
* `set` - Sets the function to set a value.
* `set_only` - Sets the function for a write only selector.
* `cache` - Sets the `CacheImplementation<'T,'U>` interface for read only and read write selectors.
* `no_cache` - Disables caching for the selector for read only and read write selectors.
* `dangerouslyAllowMutability` - Prevents object deep freezing, *use at your own risk!*

These can be used like so:

```fs
let mySelector =
    selector {
        key "mySelectorKey"
        get (fun getter ->
            let myTextValue = getter.get(myAtom)
            myTextValue + " and some more text"
        )
        set (fun setter newValue ->
            let myTextValue = setter.get(myAtom)
            setter.set(myAtom, myTextValue + newValue + " and some other text")
        )
    }
```

### selectorFamily

The selectorFamily computation expression has six operations: 

* `key` - Sets the key for the selector.
* `get` - Sets the function to get a value.
* `set` - Sets the function to set a value.
* `set_only` - Sets the function for a write only selector.
* `cache` - Sets the `CacheImplementation<'T,'U>` interface for read only and read write selectors.
* `param_cache` - Sets the `CacheImplementation<'T,'U>` interface for parameters for read only and read write selectors.
* `dangerouslyAllowMutability` - Prevents object deep freezing, *use at your own risk!*

These can be used like so:

```fs
let mySelector =
    selectorFamily {
        key "mySelectorKey"
        get (fun andEvenMoreText getter ->
            let myTextValue = getter.get(myAtom)
            myTextValue + " and some more text" + andEvenMoreText
        )
        set (fun andEvenMoreText setter newValue ->
            let myTextValue = setter.get(myAtom)
            setter.set(myAtom, myTextValue + newValue + " and some other text" + andEvenMoreText)
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
            if otherText = "" then text
            else sprintf "%s - %s" text otherText
    }
```

### recoilResult

A standard computation expression for dealing with results
inside recoil values.

Accessible by opening the Feliz.Recoil.Result namespace.

### recoilOption

A standard computation expression for dealing with options
inside recoil values.

Accessible by opening the Feliz.Recoil.Option namespace.
