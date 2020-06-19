namespace Feliz.Recoil.DataSheet

open Browser.Types
open Browser.Dom
open Fable.Core
open Feliz
open System.ComponentModel

[<Erase>]
module EventListeners =
    [<Erase>]
    module Keys =
        let [<Literal>] Backspace = "Backspace"
        let [<Literal>] Tab = "Tab"
        let [<Literal>] Enter = "Enter"
        let [<Literal>] Escape = "Escape"
        let [<Literal>] Left = "ArrowLeft"
        let [<Literal>] Up = "ArrowUp"
        let [<Literal>] Right = "ArrowRight"
        let [<Literal>] Down = "ArrowDown"
        let [<Literal>] Delete = "Delete"
        let [<Literal>] y = "y"
        let [<Literal>] z = "z"

    [<RequireQualifiedAccess;StringEnum>]
    type KeyDirection =
        | Down
        | Left
        | Right
        | Up

    [<Erase;RequireQualifiedAccess>]
    module KeyDirection =
        let inline fromEv (ev: KeyboardEvent) =
            match ev.key with
            | Keys.Down -> 
                ev.preventDefault()

                Some (false, KeyDirection.Down)
            | Keys.Left -> 
                Some (false, KeyDirection.Left)
            | Keys.Tab when ev.shiftKey -> 
                ev.preventDefault()

                Some (true, KeyDirection.Left)
            | Keys.Right ->
                Some (false, KeyDirection.Right)
            | Keys.Tab -> 
                ev.preventDefault()

                Some (true, KeyDirection.Right)
            | Keys.Up -> 
                ev.preventDefault()

                Some (false, KeyDirection.Up)
            | _ -> None

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
module EventListenerExtensions =
    type React with
        static member inline useKeyDownListener (action: KeyboardEvent -> unit) =
            React.useEffect((fun () ->
                let fn = unbox<KeyboardEvent> >> action
                document.addEventListener("keydown", fn, false)        
                React.createDisposable(fun () -> document.removeEventListener("keydown", fn))
            ), [| action :> obj |])
