# Feliz.Recoil - API Reference

## Bridge

The companion library `Feliz.Recoil.Bridge` can be installed to allow
easy compatibilty with [Elmish.Bridge](https://github.com/Nhowka/Elmish.Bridge/).

### RecoilBridge

This is a record that you create to setup your websocket connection.

Signature:
```fs
type RecoilBridge<'Model,'Msg,'ElmishMsg> =
    { Model: RecoilValue<'Model,ReadWrite>
      Update: 'Msg -> 'Model -> 'Model
      BridgeConfig: BridgeConfig<'Msg,'Msg> }
```

### Recoil.bridge

Creates a websocket bridge that will update atoms as messages are recieved from the server.

Signature:
```fs
(config: RecoilBridge<'Model,'Msg,'ElmishMsg>) -> ReactElement
```

### Usage

Using Recoil.Elmish and Recoil.Bridge we're able to easily
communicate to and from the server via websockets.

Our shared project for the client and server:
```fsharp
// Shared.fs
namespace BridgeApp

[<RequireQualifiedAccess>]
module Bridge =
    [<RequireQualifiedAccess>]
    type Response =
        | Howdy
        | NewCount of int
        | RandomCharacter of string

    [<RequireQualifiedAccess>]
    type Action =
        | IncrementCount of int
        | DecrementCount of int
        | RandomCharacter
        | SayHello

module Endpoints =
    let port = 8080us

    let baseUrl = sprintf "http://localhost:%i" port
    
    let [<Literal>] Root = "ws"
```

Configuration on the server side is the same as before
when using [Elmish.Bridge](https://github.com/Nhowka/Elmish.Bridge/).

Then in our client:

```fsharp
// App.fs
namespace BridgeApp

module Http =
    let buildEndpoint s =
        Browser.Dom.window.location.href.Split('#')
        |> Array.head
        |> sprintf "%s%s"
        <| s

module App =
    open Elmish
    open Elmish.Bridge
    open Feliz
    open Feliz.Recoil
    open Feliz.Recoil.Bridge

    type Model = 
        { Count: int
          Response: string }

    module Model =
        let atom =
            atom {
                key "model"
                def {
                    Response = ""
                    Count = 0
                }
            }
        
        let count = atom |> RecoilValue.map (fun m -> m.Count)
        let response = atom |> RecoilValue.map (fun m -> m.Response)

    type Msg = Bridge.Response

    let update msg model =
        match msg with
        | Bridge.Response.Howdy -> { model with Response = "Howdy!" }
        | Bridge.Response.NewCount i -> { model with Count = i }
        | Bridge.Response.RandomCharacter s -> { model with Response = s }

    let modelView = React.functionComponent(fun () ->
        let model = Recoil.useValue Model.atom

        Html.div [
            prop.text (model.Response + (string model.Count))
        ])

    let buttons = React.functionComponent(fun () ->
        let count = Recoil.useValue Model.count
        
        React.fragment [
            Html.button [
                prop.text "Say Hello"
                prop.onClick <| fun _ ->
                    Bridge.Send(Bridge.Action.SayHello)
            ]
            Html.button [
                prop.text "Increment"
                prop.onClick <| fun _ ->
                    Bridge.Send(Bridge.Action.IncrementCount count)
            ]
            Html.button [
                prop.text "Decrement"
                prop.onClick <| fun _ ->
                    Bridge.Send(Bridge.Action.DecrementCount count)
            ]
            Html.button [
                prop.text "Random Character"
                prop.onClick <| fun _ ->
                    Bridge.Send(Bridge.Action.RandomCharacter)
            ]
        ])

    let bridge = Recoil.bridge {
        Model = Model.atom
        Update = update
        BridgeConfig =
            Bridge.endpoint (Http.buildEndpoint Endpoints.Root)
            |> Bridge.withUrlMode UrlMode.Raw
    }

    let render = React.functionComponent(fun () -> 
        Html.div [
            Recoil.root [
                modelView()
                buttons()
                bridge()
            ]
        ])

    ReactDOM.render(render, Browser.Dom.document.getElementById "app")
```

You can see the full app [here](https://github.com/Shmew/Feliz.Recoil/tree/master/demo).
