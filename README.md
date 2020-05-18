# Feliz.Recoil [![Nuget](https://img.shields.io/nuget/v/Feliz.Recoil.svg?maxAge=0&colorB=brightgreen)](https://www.nuget.org/packages/Feliz.Recoil)

Fable bindings in Feliz style for Facebook's experimental state management library [recoil](https://github.com/facebookexperimental/Recoil).

A great intro to the library can be found [here](https://www.youtube.com/watch?v=_ISAA_Jt9kI).

A quick look:

```fs
open Feliz
open Feliz.Recoil

let textState = Recoil.atom("Hello world!")

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    Recoil.selector(fun get ->
        get(textState)
        |> String.filter(fun v -> List.contains v vowels)
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

Full documentation with live examples can be found [here](https://shmew.github.io/Feliz.Recoil/).
