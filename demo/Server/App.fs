namespace BridgeApp

module App =
    open Elmish.Bridge
    open Saturn
    open System

    module Setup =
        open Microsoft.Extensions.DependencyInjection
        open Thoth.Json.Giraffe

        let services (services: IServiceCollection) =
            services
                .AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(ThothSerializer())

    [<EntryPoint>]
    let main args =
        try
            let app =
                application {
                    app_config Giraffe.useWebSockets
                    service_config Setup.services
                    url (sprintf "http://0.0.0.0:%i/" <| Env.getPortsOrDefault 8085us)
                    use_router Router.appRouter
                    use_static (Env.clientPath args)
                    use_developer_exceptions
                }

            printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
            run app
            0 // return an integer exit code
        with e ->
            let color = Console.ForegroundColor
            Console.ForegroundColor <- System.ConsoleColor.Red
            Console.WriteLine(e.Message)
            Console.ForegroundColor <- color
            1 // return an integer exit code
