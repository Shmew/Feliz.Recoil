namespace Feliz.Recoil

open Fable.Core
open Feliz

[<AutoOpen>]
module Logger =
    let logAction (name: string, atomValue: 'T, prevAtomValue: 'T option) =
        JS.console.groupCollapsed(
            sprintf "[%s]: Atom - %s" 
                (System.DateTime.Now.ToLongTimeString())
                name
        )

        JS.console.group("Current")
        JS.console.log(atomValue)
        JS.console.groupEnd()

        JS.console.group("Previous")
        match prevAtomValue with
        | Some v -> JS.console.log(v)
        | None -> JS.console.log("No previous value.")
        JS.console.groupEnd()

        JS.console.groupEnd()

    type Recoil with
        /// Enables console debugging when in development.
        ///
        /// Similar to `React.strictMode`, this will do nothing
        /// in production.
        static member logger = React.functionComponent(fun () ->
            #if DEBUG
            Recoil.useTransactionObservation <| fun o ->
                o.modifiedAtoms 
                |> Set.iter (fun (name, _) ->
                    o.atomInfo.TryFind(name)
                    |> Option.map (fun o -> o.persistence.type')
                    |> Option.bind (fun _ -> o.atomValues.TryFind(name))
                    |> Option.iter(fun value ->
                        logAction (
                            name,
                            value,
                            o.previousAtomValues.TryFind(name)
                        ))
                )
            #endif

            Html.none)
