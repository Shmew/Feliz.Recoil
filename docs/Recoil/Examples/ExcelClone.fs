[<RequireQualifiedAccess>]
module Samples.ExcelClone

open Feliz
open Feliz.Recoil
open Feliz.Recoil.DataSheet

let render = React.functionComponent(fun () ->
    Recoil.root [
        Recoil.logger()
        DataSheet.dataSheet()
    ])
