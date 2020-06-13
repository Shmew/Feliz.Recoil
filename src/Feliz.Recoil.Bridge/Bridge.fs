namespace Feliz.Recoil.Bridge

[<AutoOpen>]
module ElmishBridge =
    open Browser
    open Browser.Types
    open Elmish
    open Elmish.Bridge
    open Fable.Core
    open Fable.SimpleJson
    open Feliz
    open Feliz.Recoil
    open System.ComponentModel

    type BridgeConfig<'Msg,'ElmishMsg> with
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member inline this.AttachRecoil (dispatch: 'ElmishMsg -> unit) =
            let url =
                match this.urlMode with
                | Replace ->
                    let url = Helpers.getBaseUrl()
                    url.pathname <- this.path
                    url
                | Append ->
                    let url = Helpers.getBaseUrl()
                    url.pathname <- url.pathname + this.path
                    url
                | Calculated f ->
                    let url = Helpers.getBaseUrl()
                    f url.href this.path |> Url.URL.Create
                | Raw ->
                    let url = Browser.Url.URL.Create this.path
                    url.protocol <- url.protocol.Replace("http", "ws")
                    url

            let wsref : WebSocket option ref = ref None

            let rec websocket timeout server =
                match !wsref with
                | Some _ -> ()
                | None ->
                    let socket = WebSocket.Create server

                    wsref := Some socket

                    socket.onclose <- fun _ ->
                        wsref := None

                        this.whenDown |> Option.iter dispatch

                        JS.setTimeout (fun () -> websocket timeout server) timeout
                        |> ignore

                    socket.onmessage <- fun e ->
                        Json.tryParseNativeAs(string e.data)
                        |> function
                        | Ok msg -> msg |> this.mapping |> dispatch
                        | _ -> ()

            websocket (this.retryTime * 1000) (url.href.TrimEnd '#')

            Helpers.mappings <-
                Helpers.mappings
                |> Map.add this.name
                    (this.customSerializers,
                     (fun e callback ->
                        match !wsref with
                        | Some socket -> socket.send e
                        | None -> callback ()))

            React.createDisposable(fun () -> 
                wsref.Value 
                |> Option.iter (fun ws -> ws.close()))

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    module Bridge =
        let inline attach dispatch (bridgeConfig: BridgeConfig<'Msg,'ElmishMsg>) =
            bridgeConfig.AttachRecoil(dispatch)

    type RecoilBridge<'Model,'Msg,'ElmishMsg> =
        { Model: RecoilValue<'Model,ReadWrite>
          Update: 'Msg -> 'Model -> 'Model
          BridgeConfig: BridgeConfig<'Msg,'Msg> }

    type Recoil with
        /// Creates a websocket bridge that will update atoms as messages are recieved from the server.
        static member inline bridge<'Model,'Msg,'ElmishMsg> (config: RecoilBridge<'Model,'Msg,'ElmishMsg>) =
            React.functionComponent(fun () ->
                let dispatch = Recoil.useSetReducer(config.Model, config.Update)

                React.useEffectOnce(fun () ->
                    config.BridgeConfig
                    |> Bridge.attach(dispatch))

                Html.none)
