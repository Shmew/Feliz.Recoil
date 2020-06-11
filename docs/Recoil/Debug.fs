module Debug

open Css
open Elmish
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let renderCount = React.functionComponent(fun () ->
    let countRef = React.useRef 0
    
    let mutable currentCount = countRef.current

    React.useEffect(fun () -> countRef.current <- currentCount)

    currentCount <- currentCount + 1

    Html.div [
        prop.text (sprintf "Render count: %i" currentCount)
    ])

let drawBorder' = React.functionComponent(fun (input: {| children: ReactElement list|}) ->
    Html.div [
        prop.classes [ Bulma.Box ]
        prop.children input.children
    ])

let inline drawBorder (children: ReactElement list) = 
    drawBorder' {| children = children |}
