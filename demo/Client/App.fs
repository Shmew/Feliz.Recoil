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
    open Feliz.Recoil.Elmish

    type Model = 
        { Response: string 
          Count: int }

    module Model =
        type Atom = 
            { Response: RecoilValue<string,ReadWrite>
              Count: RecoilValue<int,ReadWrite> }

        let [<Literal>] Key = "model"

        let atoms = 
            { Response = Recoil.atom(Key + "/response", "")
              Count = Recoil.atom(Key + "/count", 0) }

    type Msg = Bridge.Response

    let update msg model =
        match msg with
        | Bridge.Response.Howdy -> { model with Response = "Howdy!" }, Cmd.none
        | Bridge.Response.NewCount i -> { model with Count = i }, Cmd.none
        | Bridge.Response.RandomCharacter s -> { model with Response = s }, Cmd.none

    let modelView = React.functionComponent(fun () ->
        let count = Recoil.useValue Model.atoms.Count
        let rsp = Recoil.useValue Model.atoms.Response

        Html.div [
            prop.text (rsp + (string count))
        ])

    let buttons = React.functionComponent(fun () ->
        let count = Recoil.useValue Model.atoms.Count
        
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
        Key = Model.Key
        Model = Model.atoms
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
