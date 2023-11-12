[<RequireQualifiedAccess>]
module Samples.Async

open Css
open Fable.Core
open Fable.SimpleHttp
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let pokemon = 
    atom {
        key "Async/pokemon"
        def "pikachu"
    }

let askIsPokemon = 
    selector {
        key "Async/isPokemon"
        get (fun getter ->
            async {
                let pokemonQuery = getter.get(pokemon)
                do! Async.Sleep 400
                if pokemonQuery <> "" then
                    let! (statusCode, _) = 
                        Http.get (sprintf "https://pokeapi.co/api/v2/pokemon/%s" pokemonQuery)

                    return (statusCode = 200)
                else return false
            })
    }
    (*
        Can also be written as:

        Recoil.selector("Async/isPokemon", fun getter ->
            async {
                let pokemonQuery = getter.get(pokemon)

                do! Async.Sleep 400
                if pokemonQuery <> "" then
                    let! (statusCode, _) = 
                        Http.get (sprintf "https://pokeapi.co/api/v2/pokemon/%s" pokemonQuery)

                    return (statusCode = 200)
                else return false
            }
        )
    *)

let spinner =
    Html.div [
        prop.style [
        ]
        prop.children [
            Html.li [
                prop.className [
                    FA.Fa
                    FA.FaSpinner
                    FA.FaSpin
                ]
            ]
        ]
    ]

let pokemonAsker = React.functionComponent(fun () ->
    let isPokemon = Recoil.useValue(askIsPokemon)
    
    Html.div [
        prop.text (sprintf "%b" isPokemon)
    ])

let inner = React.functionComponent(fun () ->
    let pokeStr, askIsPokemon = Recoil.useState(pokemon)
    let currentText,setCurrentText = React.useState(pokeStr)

    React.useLayoutEffect((fun () ->
        let handler = JS.setTimeout (fun () -> askIsPokemon(currentText)) 200

        React.createDisposable(fun () -> JS.clearTimeout(handler))
    ), [| currentText :> obj |])

    Html.div [
        Html.div [
            prop.style [
                style.display.flex
            ]
            prop.children [
                Html.span "Is it a pokemon?:"
                Html.div [
                    prop.style [
                        style.paddingLeft (length.em 0.2)
                    ]
                    prop.children [
                        React.suspense ([
                            pokemonAsker()  
                        ], spinner)
                    ]
                ]
            ]
        ]
        Html.input [
            prop.classes [ Bulma.Input ]
            prop.style [ style.maxWidth (length.em 30) ]
            prop.type'.text
            prop.onTextChange <| fun s ->
                setCurrentText s
            prop.defaultValue pokeStr
        ]
    ])

// let render = React.functionComponent("Async", fun () ->
//     Recoil.root [
//         inner()
//     ])

[<ReactComponent>]
let render() =
    Recoil.root [ inner () ]

