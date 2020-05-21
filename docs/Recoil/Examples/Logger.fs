[<RequireQualifiedAccess>]
module Samples.Logger

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

// DONT INCLUDE IN DEMO
// _____________________________________________

open Fable.Core

let internal logAction (name: string, atomValue: 'T, prevAtomValue: 'T option) =
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
    static member logger = React.functionComponent(fun () ->
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

        Html.none)

// DONT INCLUDE IN DEMO
// _____________________________________________

let textState = 
    atom {
        key "textState"
        def "Hello world!"
        log
    }

(* This can also be implented like so:

let textState = 
    Recoil.atom (
        "textState", 
        "Hello world!", 
        { Type = PersistenceType.Log
          Backbutton = None
          Validator = (fun _ -> None) }
    )
*)

let inner = React.functionComponent(fun () ->
    let text,setText = Recoil.useState(textState)

    Html.div [
        Html.div [
            prop.text (sprintf "Atom current value: %s" text)
        ]
        Html.input [
            prop.classes [ Bulma.Input ]
            prop.style [ style.maxWidth (length.em 30) ]
            prop.type'.text
            prop.onTextChange setText
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        Recoil.logger()
        inner()
    ])

