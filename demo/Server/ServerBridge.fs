namespace BridgeApp

/// Websocket bridge
module ServerBridge =
    open Elmish
    open Elmish.Bridge

    type Model =
        { Empty: string }
            
    let init (clientDispatch: Dispatch<Bridge.Response>) (httpContext: Model) =
        clientDispatch(Bridge.Response.Howdy)
        httpContext, Cmd.none

    let update (clientDispatch: Dispatch<Bridge.Response>) (msg: Bridge.Action) (model: Model) =
        let withClientDispatch cmd (rsp: Bridge.Response) =
            clientDispatch rsp
            model, cmd

        match msg with
        | Bridge.Action.SayHello ->
            Bridge.Response.Howdy
            |> withClientDispatch Cmd.none
        | Bridge.Action.IncrementCount i ->
            Bridge.Response.NewCount(i + 1)
            |> withClientDispatch Cmd.none
        | Bridge.Action.DecrementCount i ->
            Bridge.Response.NewCount(i - 1)
            |> withClientDispatch Cmd.none
        | Bridge.Action.RandomCharacter ->
            let characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"

            System.Random().Next(0,characters.Length-1)
            |> fun i -> characters.ToCharArray().[i]
            |> string
            |> Bridge.Response.RandomCharacter
            |> withClientDispatch Cmd.none
