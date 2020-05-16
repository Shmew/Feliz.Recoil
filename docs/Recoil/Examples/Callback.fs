[<RequireQualifiedAccess>]
module Samples.Callback

open Feliz
open Feliz.Recoil

let firstCount = Recoil.atom("firstCount", 0)
let secondCount = Recoil.atom("secondCount", 0)

let countOne = React.functionComponent(fun () ->
    let firstCount,setFirstCount = Recoil.useState(firstCount)

    Html.div [
        Html.div [
            prop.text (sprintf "First count current value: %i" firstCount)
        ]
        Html.button [
            prop.text "Increment"
            prop.onClick <| fun _ -> setFirstCount (firstCount + 1)
        ]
    ])

let countTwo = React.functionComponent(fun () ->
    let secondCount,setSecondCount = Recoil.useState(secondCount)

    Html.div [
        Html.div [
            prop.text (sprintf "First count current value: %i" secondCount)
        ]
        Html.button [
            prop.text "Increment"
            prop.onClick <| fun _ -> setSecondCount (secondCount + 1)
        ]
    ])

let combine = React.functionComponent(fun () ->
    let sum,setSum = React.useState(0)

    let setSum = 
        Recoil.useCallback(fun caller ->
            async {
                let! one = caller.getAsync(firstCount)
                let! two = caller.getAsync(secondCount)

                do setSum(one + two)
            }
            |> Async.StartImmediate
        )

    Html.div [
        prop.children [
            Html.div [
                prop.text (sprintf "Sum: %i" sum)
            ]
            Html.button [
                prop.text "Async sum"
                prop.onClick <| fun _ -> setSum()
            ]
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
       countOne()
       countTwo()
       combine()
    ])

