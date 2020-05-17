# Feliz.Recoil - Atom Family Example

This example shows some basic usage of atom families.

```fsharp:recoil-atomfamily
open Feliz
open Feliz.Recoil

let textState = Recoil.atom("textState", "Hello world!")

let inner = React.functionComponent(fun () ->
    let text,setText = Recoil.useState(textState)

    Html.div [
        Html.div [
            prop.text (sprintf "Atom current value: %s" text)
        ]
        Html.input [
            prop.type'.text
            prop.onTextChange setText
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
       inner()
    ])
```
