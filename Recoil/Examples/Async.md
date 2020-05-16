# Feliz.Recoil - Async Example

This example shows how you can seamlessly implement
asynchronous transformations.

```fsharp:recoil-async
open Fable.SimpleHttp
open Feliz
open Feliz.Recoil

let pokemon = Recoil.atom("pokemon", "pikachu")

let askIsPokemon = 
    Recoil.selector("isPokemon", fun get ->
        let pokemonQuery = get(pokemon)
        async {
            let! (statusCode, _) = 
                Http.get (sprintf "https://pokeapi.co/api/v2/pokemon/%s" pokemonQuery)

            return (statusCode = 200)
        }
    )

let inner = React.functionComponent(fun () ->
    let isPokemon = Recoil.useValue(askIsPokemon)
    let askIsPokemon = Recoil.useSetState(pokemon)

    Html.div [
        Html.div [
            prop.text (sprintf "Is it a pokemon?: %b" isPokemon)
        ]
        Html.input [
            prop.type'.text
            prop.onTextChange askIsPokemon
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
       inner()
    ])
```
