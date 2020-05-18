# Feliz.Recoil - Bi-Directional Selectors Example

This example builds upon the mix-and-match example, 
but also demonstrates that selectors can also have a
setter pipeline.

```fsharp:recoil-bidirectionalselectors
open Feliz
open Feliz.Recoil

let textState = Recoil.atom("textState", "Hello world!")

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    Recoil.selector (
        key = "textStateSelector", 
        get = 
            (fun getter ->
                getter.get(textState)
                |> String.filter(fun v -> List.contains v vowels)),
        set =
            (fun selector newValue ->
                selector.set(textState, newValue + "whoa"))
    )

let inner = React.functionComponent(fun () ->
    let text, setText = Recoil.useState(textStateTransform)

    Html.div [
        Html.div [
            prop.text (sprintf "Transformed value: %s" text)
        ]
        Html.input [
            prop.type'.text
            prop.onTextChange setText
        ]
    ])

let otherInner = React.functionComponent(fun () ->
    let textAtom = Recoil.useValue(textState)

    Html.div [
        prop.text (sprintf "Atom current value: %s" textAtom)
    ]
)

let render = React.functionComponent(fun () ->
    Recoil.root [
       inner()
       otherInner()
    ])
```
