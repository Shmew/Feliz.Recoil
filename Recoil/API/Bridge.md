# Feliz.Recoil - API Reference

## Bridge

The companion library `Feliz.Recoil.Bridge` can be installed to allow
easy compatibilty with [Elmish.Bridge](https://github.com/Nhowka/Elmish.Bridge/).

### RecoilBridge

This is a record that you create to setup your websocket connection.

Signature:
```fs
type RecoilBridge<'AtomRecord,'Model,'Msg,'ElmishMsg> =
    { Key: string
      Model: 'AtomRecord
      Update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>
      BridgeConfig: BridgeConfig<'Msg,'Msg> }
```

### Recoil.bridge

Creates a websocket bridge that will update atoms as messages are recieved from the server.

Signature:
```fs
(config: RecoilBridge<'AtomRecord,'Model,'Msg,'ElmishMsg>) -> ReactElement
```

See the [example](https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Websockets) for 
more information on usage.
