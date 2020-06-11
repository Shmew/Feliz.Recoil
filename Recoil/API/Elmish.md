# Feliz.Recoil - API Reference

## Elmish

The companion library `Feliz.Recoil.Elmish` can be installed to allow
for more traditional elmish abstractions.

The entirety of the library is one additional hook:

### Recoil.useDispatch

Returns an elmish dispatch function.

Be aware that currently **using multiple useDispatch that uses commands is not safe!**
If you know of a way to solve this please let me know!

Signature:
```fs
(model: RecoilValue<'Model,ReadWrite>, update: 'Msg -> 'Model -> 'Model) -> ('Msg -> unit)
(model: RecoilValue<'Model,ReadWrite>, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>) -> ('Msg -> unit)
```

See the [elmish example](https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Elmish) for how to use.
