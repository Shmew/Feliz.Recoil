# Feliz.Recoil - Persistence Example

This example shows how you can persist atoms to local and session storage
as well as re-hydrate them on initialization.

```fsharp:recoil-persistence
open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let localStorageText = 
    atom {
        key "Storage/localTextState"
        def "Hello world!"
        local_storage
    }
    
let sessionStorageText =
    atom {
        key "Storage/sessionTextState"
        def "Hello world!"
        session_storage
    }

(* This can also be implented like so:

let localStorageText = 
    Recoil.atom (
        key = "Storage/localTextState", 
        defaultValue = "Hello world!", 
        effects = [ 
            AtomEffect Storage.local
        ]
    )

let sessionStorageText = 
    Recoil.atom (
        key = "Storage/sessionTextState", 
        defaultValue = "Hello world!", 
        effects = [ 
            AtomEffect Storage.session
        ]
    )
*)

let label = React.functionComponent(fun (input: {| baseStr: string; recoilValue: RecoilValue<string,ReadWrite> |}) ->
    let text = Recoil.useValue(input.recoilValue)

    Html.div [
        prop.style [
            style.paddingBottom (length.em 1)
            style.paddingTop (length.em 1)
        ]
        prop.text (input.baseStr + text)
    ])

let storageComp = React.functionComponent(fun (input: {| recoilValue: RecoilValue<string,ReadWrite> |}) ->
    let text,setText = Recoil.useState(input.recoilValue)

    Html.div [
        Html.input [
            prop.classes [ Bulma.Input ]
            prop.style [ style.maxWidth (length.em 30) ]
            prop.type'.text
            prop.onTextChange setText
            prop.value (if text = "Hello world!" then "" else text)
        ]
    ])

let render = React.functionComponent("Persistence", fun () ->
    Recoil.root [
        label {| baseStr = "Local storage: "; recoilValue = localStorageText |}
        storageComp {| recoilValue = localStorageText |}
        label {| baseStr = "Session storage: "; recoilValue = sessionStorageText |}
        storageComp {| recoilValue = sessionStorageText |}
    ])
```
