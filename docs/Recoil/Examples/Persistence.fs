[<RequireQualifiedAccess>]
module Samples.Persistence

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
        root.localStorage (fun hydrater -> hydrater.setAtom localStorageText)
        root.sessionStorage (fun hydrater -> hydrater.setAtom sessionStorageText)

        root.children [
            label {| baseStr = "Local storage: "; recoilValue = localStorageText |}
            storageComp {| recoilValue = localStorageText |}
            label {| baseStr = "Session storage: "; recoilValue = sessionStorageText |}
            storageComp {| recoilValue = sessionStorageText |}
        ]
    ])
