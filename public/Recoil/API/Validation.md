# Feliz.Recoil - API Reference

## Validation

The companion library `Feliz.Recoil.Validation` can be installed to gain access
to the `Recoil.useValidation` hook, which enables easy validation of inputs.

### ValidationState

This is a discriminated union that represents the state of your validated input.

Signature:
```fs
type ValidationState<'T,'Custom> =
    | Init
    | Invalid of ValidationError<'Custom>
    | Valid of 'T
```

When using the hook on the `onCahnge` (or any other event) it will push the new state
to the atom if the state changes.

### ValidationError

This discriminated union represents a strongly typed error classification from the validators
passed into the hook. You can extend this via the `Custom` case to allow for additional error cases.

Signature:
```fs
type ValidationError<'Custom> =
    | Blank
    | Contains
    | Custom of 'Custom
    | Eq
    | Email
    | Float
    | Gt
    | Gte
    | HasNumbers
    | HasSpecialCharacters
    | Int
    | Length of int
    | Lt
    | Lte
    | Match
    | MaxLength
    | MinLength
    | None
    | NoNumbers
    | NoSpecialCharacters
    | Parse
    | Some
    | Url
```

### Validator

This is a type alias that represents a `RecoilValue<Result<'T,ValidationError<'Custom>>,_> -> RecoilValue<Result<'T,ValidationError<'Custom>>,ReadOnly>`.

Every step of the validation pipeline is remembered based on the input validators, which allows for very performant
validation pipelines to be created; in particular when validation cases require asynchronous requests that may take
a long amount of time to respond.

### validate

This is the main module that houses all of the validators, there are cases for different types such as `string` and `list`.

There are also top level generic validators and a `custom` method to extend functionality to fit any needs.

In addition for cases where you may have nested types within your validation pipeline such as an option or result there are
helper methods to run validation pipelines within those contexts.

For example if you have a validation atom that holds an `int list` you can pass a set of validators like this:

```fs
let setList = Recoil.useValidation(myListAtom, [
    validate.list.maxLengthOf 5
    validate.list.forAll [
        validate.gt 1
        validate.lte 5
    ]
])
```

You can find an example on further usage [here](https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Validation).
