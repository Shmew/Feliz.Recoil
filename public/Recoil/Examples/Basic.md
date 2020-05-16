# Feliz.Recoil - Basic Example

This example shows some basic usage of both atoms and selectors, 
and how you can pick and choose what parts of each you want to
consume within a component.

```fsharp:recoil-basic
open Feliz
open Feliz.Recoil

let textState = Recoil.atom("textState", "Hello world!")

let textStateTransform =
    Recoil.selector(
        "textStateSelector",
        (fun get ->
            get(textState)
            |> String.filter(fun c -> c <> 'o'))
    )

let inner = React.functionComponent(fun () ->
    let setAtomText = Recoil.useSetState(textState)
    let text = Recoil.useValue(textStateTransform)

    Html.div [
        Html.div [
            prop.text (sprintf "Transformed value: %s" text)
        ]
        Html.input [
            prop.type'.text
            prop.onTextChange setAtomText
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

Compare it to how this is done traditionally (see block below) and
then think about what would happen if the two components were on 
completely other sides of your react tree.

You'd have to hoist that state all the way up to the top level and
then pass it down to the other component that needs it. Each of them
re-rendering when that prop changes even though they have no usage of
the value themselves.

```fs
open Feliz

let innerNormal = React.functionComponent(fun (input: {| text: string; callback: string -> unit |}) ->
    Html.div [
        Html.div [
            prop.text (sprintf "Transformed value: %s" (input.text |> String.filter(fun c -> c <> 'o')))
        ]
        Html.input [
            prop.type'.text
            prop.onTextChange input.callback
        ]
    ])

let otherInnerNormal = React.functionComponent(fun (input: {| text: string |}) ->
    Html.div [
        prop.text (sprintf "Atom current value: %s" input.text)
    ]
)

let renderNormal = React.functionComponent(fun () ->
    let text,setText = React.useState "Hello world!"

    let setText = React.useCallback(setText, [||])

    Html.div [
        innerNormal {| text = text; callback = setText |}
        otherInnerNormal {| text = text |}
    ]
)

let renderAll = React.functionComponent(fun () ->
    Html.div [
        render()
        renderNormal()
    ])
```