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
