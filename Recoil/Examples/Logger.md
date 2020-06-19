# Feliz.Recoil - Debug Logging Example

This example shows how you can enable debug logging.

```fsharp:recoil-logger
open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let textState = 
    atom {
        key "Logger/textState"
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
        root.log true

        root.children [
            inner()
        ]
    ])
```
