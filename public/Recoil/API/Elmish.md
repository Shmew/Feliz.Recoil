# Feliz.Recoil - API Reference

## Elmish

The companion library `Feliz.Recoil.Elmish` can be installed to allow
for more traditional elmish abstractions.

The entirety of the library is one additional hook:

### Recoil.useDispatch

Returns an elmish dispatch function.

Signature:
```fs
(selectorKey: string, model: 'AtomRecord, update: 'Msg -> 'Model -> 'Model) -> ('Msg -> unit)
(selectorKey: string, model: 'AtomRecord, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>) -> ('Msg -> unit)
```

See the [elmish example](https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Elmish) for how to use.
