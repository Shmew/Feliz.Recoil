[<RequireQualifiedAccess>]
module Samples.Concurrency

open Css
open Fable.Core
open Fable.SimpleHttp
open Fable.SimpleJson
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

type Pokemon = { Name: string; Image: string }

type SettingMode =
    | WaitForAll
    | WaitForAny
    | WaitForNone

let rng = System.Random()

let settingMode =
    atom {
        key "Concurrency/settingMode"
        def WaitForAll
    }

let queryPokemon = 
    selectorFamily {
        key "Concurrency/queryPokemon"
        get (fun pokemon getter ->
            let _ = getter.get(settingMode) // hack to force cache invalidation for demo
            async {
                do! Async.Sleep (rng.Next(200, 1000))
                let! (statusCode, responseText) = 
                    Http.get (sprintf "https://pokeapi.co/api/v2/pokemon/%s" pokemon)

                return
                    if statusCode <> 200 then None
                    else
                        SimpleJson.tryParse responseText
                        |> Option.map (fun text ->
                            [ SimpleJson.readPath ["sprites";"front_default"] text |> Option.map (fun v -> "Image", v)
                              SimpleJson.readPath ["species"; "name"] text |> Option.map (fun v -> "Name", v) ]
                            |> List.choose id
                            |> Map.ofList
                            |> Json.JObject)
                        |> Option.bind (Json.tryConvertFromJsonAs<Pokemon> >> function | Ok res -> Some res | _ -> None)
            })
    }

let pokemonList = [ "ditto"; "charmander"; "pikachu"; "gengar" ]

let fetchAllPokemon =
    selectorFamily {
        key "Concurrency/fetchAllPokemon"
        get (fun (pokemonList: string list) getter ->
            let pokemonRecoils =
                pokemonList
                |> List.map (queryPokemon)
                    
            let settingMode = getter.get(settingMode)

            match settingMode with
            | WaitForAll -> 
                Recoil.waitForAll(pokemonRecoils) 
                |> getter.get
                |> fun res ->
                    res |> List.iter (fun o -> JS.console.log(o))
                    res
            | WaitForAny -> 
                Recoil.waitForAny(pokemonRecoils) 
                |> getter.get
                |> List.map (fun l -> l.valueMaybe() |> Option.flatten)
                |> fun res ->
                    res |> List.iter (fun o -> JS.console.log(o))
                    res
            | WaitForNone -> 
                Recoil.waitForNone(pokemonRecoils)
                |> getter.get
                |> List.map (fun l -> l.valueMaybe() |> Option.flatten)
            |> List.choose id
        )
    }

let spinner =
    Html.div [
        prop.style [
            style.textAlign.center
            style.marginLeft length.auto
            style.marginRight length.auto
        ]
        prop.children [
            Html.li [
                prop.className [
                    FA.Fa
                    FA.Fa5X
                    FA.FaSpinner
                    FA.FaSpin
                ]
            ]
        ]
    ]

let label = React.functionComponent(fun (input: {| name: string |}) ->
    Html.div [
        prop.style [
            style.paddingBottom (length.em 1)
            style.paddingTop (length.em 1)
        ]
        prop.text input.name
    ])

let optionPanel = React.functionComponent(fun () ->
    let selectedOption,setSelectedOption = Recoil.useState(settingMode)
    
    Html.div [
        prop.style [
            style.marginTop (length.em 1)
            style.maxWidth (length.em 30)
        ]
        prop.classes [ Bulma.Box; Bulma.HasTextCentered ]
        prop.children [
            label {| name = "Concurrency Mode" |}
            Html.div [
                Html.button [
                    prop.classes [ 
                        Bulma.Button
                        Bulma.HasBackgroundPrimary
                        Bulma.HasTextWhite 
                    ]
                    prop.text "WaitForAll"
                    prop.disabled <| (selectedOption = WaitForAll)
                    prop.onClick <| fun _ -> setSelectedOption WaitForAll
                ]
                Html.button [
                    prop.classes [ 
                        Bulma.Button
                        Bulma.HasBackgroundPrimary
                        Bulma.HasTextWhite 
                    ]
                    prop.text "WaitForAny"
                    prop.disabled <| (selectedOption = WaitForAny)
                    prop.onClick <| fun _ -> setSelectedOption WaitForAny
                ]
                Html.button [
                    prop.classes [ 
                        Bulma.Button
                        Bulma.HasBackgroundPrimary
                        Bulma.HasTextWhite 
                    ]
                    prop.text "WaitForNone"
                    prop.disabled <| (selectedOption = WaitForNone)
                    prop.onClick <| fun _ -> setSelectedOption WaitForNone
                ]
            ]
            
        ]
    ])

let imgComp = React.functionComponent(fun (input: {| src: string |}) ->
    Html.div [
        prop.classes [ Bulma.MediaLeft ]
        prop.children [
            Html.figure [
                prop.classes [ Bulma.Image; Bulma.Is96X96 ]
                prop.children [
                    Html.img [
                        prop.src input.src
                    ]
                ]
            ]
        ]
    ])

let detailsComp = React.functionComponent(fun (input: {| name: string |}) ->
    Html.div [
        prop.classes [ Bulma.MediaContent ]
        prop.children [
            Html.div [
                prop.classes [ Bulma.Content ]
                prop.children [
                    Html.h1 input.name
                ]
            ]
        ]
    ])

let pokemonDisplay = React.functionComponent(fun () ->
    let pokemon = Recoil.useValue(fetchAllPokemon(pokemonList))

    Html.div (
        pokemon
        |> List.map (fun p ->
            Html.div [
                prop.classes [ Bulma.Box ]
                prop.children [
                    Html.article [
                        prop.classes [ Bulma.Media ]
                        prop.children [
                            imgComp {| src = p.Image |}
                            detailsComp {| name = p.Name |}
                        ]
                    ]
                ]
            ]
        )
    ))

let render = React.functionComponent(fun () ->
    Recoil.root [
        Html.div [
            prop.style [
                style.maxWidth (length.em 30)
            ]
            prop.children [
                optionPanel()
                Html.div [
                    React.suspense ([
                        pokemonDisplay()
                    ], spinner)
                ]
            ]
        ]
    ])
