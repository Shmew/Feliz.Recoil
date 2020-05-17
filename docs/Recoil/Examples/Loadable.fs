[<RequireQualifiedAccess>]
module Samples.Loadable

open Fable.Core
open Fable.SimpleHttp
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

type FA = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.13.0/css/fontawesome.min.css", Naming.PascalCase>

let pokemon = Recoil.atom("pokemon", "pikachu")

let askIsPokemon = 
    Recoil.selector("isPokemon", fun get ->
        let pokemonQuery = get(pokemon)
        async {
            do! Async.Sleep 400
            if pokemonQuery <> "" then
                let! (statusCode, _) = 
                    Http.get (sprintf "https://pokeapi.co/api/v2/pokemon/%s" pokemonQuery)

                return (statusCode = 200)
            else return false
        }
    )

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
    let isPokemon = Recoil.useValueLoadable(askIsPokemon)

    match isPokemon.state() with
    | LoadableState.HasValue b ->
        Html.div [
            prop.text (sprintf "%b" b)
        ]
    | _ -> spinner)

let inner = React.functionComponent(fun () ->
    let pokeStr, askIsPokemon = Recoil.useState(pokemon)
    let currentText,setCurrentText = React.useState(pokeStr)

    React.useEffect((fun () ->
        let handler = JS.setTimeout (fun () -> askIsPokemon(currentText)) 200

        React.createDisposable(fun () -> JS.clearTimeout(handler))
    ))

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
                        pokemonAsker()  
                    ]
                ]
            ]
        ]
        Html.input [
            prop.type'.text
            prop.onTextChange <| fun s ->
                setCurrentText s
            prop.defaultValue pokeStr
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
       inner()
    ])

