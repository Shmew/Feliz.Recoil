[<RequireQualifiedAccess>]
module Samples.ExcelClone

open Feliz
open Feliz.Recoil
open Feliz.Recoil.DataSheet

// The code is in the project repo, it's too large for this page.

let render = React.functionComponent("ExcelClone", fun () ->
    Recoil.root [
        root.timeTravel true

        root.children [
            DataSheet.dataSheet()
        ]
    ])
