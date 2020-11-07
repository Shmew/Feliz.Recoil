namespace Feliz.Recoil

open Fable.Core

[<RequireQualifiedAccess>]
module Logger =
    /// Logging AtomEffect
    let effect (e: Effector<'T,#ReadWrite>) =
        e.onSet(fun newValue oldValue ->
            JS.console.groupCollapsed(
                sprintf "[%s]: Atom - %s" 
                    (System.DateTime.Now.ToLongTimeString())
                    e.node.key
            )

            JS.console.group("Current")
            JS.console.log(newValue)
            JS.console.groupEnd()

            JS.console.group("Previous")
            JS.console.log(oldValue)
            JS.console.groupEnd()

            JS.console.groupEnd()
        )
