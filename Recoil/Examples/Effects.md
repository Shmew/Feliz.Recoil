# Feliz.Recoil - Effects

This example shows how you can use atom effects.

```fsharp:recoil-effects
open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let count = 
    atom {
        key "Effects/count"
        def 0
        effect (fun e -> 
            e.onSet(fun i ->
                if i % 2 = 0 then
                    e.setSelf (i + 1)
            )
        )
    }

    (* 
        Can also be written as:
    
        Recoil.atom (
            key = "Effects/countTest", 
            defaultValue = 0, 
            effects = [ 
                AtomEffect (fun e -> 
                    e.onSet(fun i ->
                        if i % 2 = 0 then
                            e.setSelf (i + 1)
                    )
                ) 
            ]
        )
    *)
    
let increment = React.functionComponent(fun () ->
    let setCount = Recoil.useSetStatePrev count

    Html.div [
        Html.button [
            prop.classes [ 
                Bulma.Button
                Bulma.HasBackgroundPrimary
                Bulma.HasTextWhite 
            ]
            prop.text "Increment"
            prop.onClick <| fun _ -> setCount (fun c -> c + 1)
        ]
    ])

let combine = React.functionComponent(fun () ->
    let count = Recoil.useValue count

    Html.div [
        prop.children [
            Html.div [
                prop.text (sprintf "Sum: %i" count)
            ]
        ]
    ])

let render = React.functionComponent("Effects", fun () ->
    Recoil.root [
        increment()
        combine()
    ])
```
