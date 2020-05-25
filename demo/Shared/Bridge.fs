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
