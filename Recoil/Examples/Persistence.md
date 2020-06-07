﻿# Feliz.Recoil - Persistence Example

This example shows how you can persist atoms to local and session storage
as well as re-hydrate them on initialization.

```fsharp:recoil-persistence
open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let textState = Recoil.atom("Basic/textState", "Hello world!")

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
        inner()
    ])
```
