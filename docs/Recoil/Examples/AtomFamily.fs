[<RequireQualifiedAccess>]
module Samples.AtomFamily

open Feliz
open Feliz.Recoil

//let textState = Recoil.Family.atom("textState", (sprintf "Hello world #%i"))

//let inner = React.functionComponent(fun () ->
//    let text,setText = Recoil.useState(textState)

//    Html.div [
//        Html.div [
//            prop.text (sprintf "Atom current value: %s" text)
//        ]
//        Html.input [
//            prop.type'.text
//            prop.onTextChange setText
//        ]
//    ])

//let render = React.functionComponent(fun () ->
//    Recoil.root [
//       inner()
//    ])

