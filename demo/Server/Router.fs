namespace BridgeApp

module Router =
    open Elmish.Bridge
    open Giraffe.Core
    open Giraffe.ResponseWriters
    open Saturn

    let browser =
        pipeline {
            plug acceptHtml
            plug fetchSession
        }

    let defaultView =
        router {
            get "/" (htmlFile "public/index.html")
            get "" (redirectTo false "/")
            get "/index.html" (redirectTo false "/")
            get "/default.html" (redirectTo false "/")
        }

    let browserRouter =
        router {
            not_found_handler (htmlFile "public/index.html")
            pipe_through browser
            forward "" defaultView
        }

    let bridge =
            Bridge.mkServer ("/" + Endpoints.Root) ServerBridge.init ServerBridge.update
            |> Bridge.withConsoleTrace
            |> Bridge.runWith Giraffe.server { Empty = "" }

    let appRouter =
        router {
            forward "" browserRouter
            get ("/" + Endpoints.Root) (bridge)
        }
        