namespace Feliz.Recoil.DataSheet

open Fable.Core
open Feliz
open Feliz.Recoil

[<RequireQualifiedAccess>]
type CellState =
    | Inert
    | Editing

type Cell =
    { row: RecoilValue<int,ReadWrite>
      col: RecoilValue<int,ReadWrite>
      selected: RecoilValue<bool,ReadWrite>
      state: RecoilValue<CellState,ReadWrite>
      value: RecoilValue<int option,ReadWrite>
      expr: RecoilValue<string,ReadWrite> }

[<Erase>]
module Keys =
    let [<Literal>] Backspace = 8
    let [<Literal>] Tab = 9
    let [<Literal>] Enter = 13
    let [<Literal>] Escape = 27
    let [<Literal>] Left = 37
    let [<Literal>] Up = 38
    let [<Literal>] Right = 39
    let [<Literal>] Down = 40
    let [<Literal>] Delete = 46

module Atoms =
    module internal Cell =
        let rowFamily =
            atomFamily {
                key "__datasheet__/rowFamily"
                def (fun (row: int, col: int) -> row)
            }

        let colFamily =
            atomFamily {
                key "__datasheet__/colFamily"
                def (fun (row: int, col: int) -> col)
            }

        let selectedFamily =
            atomFamily {
                key "__datasheet__/selectedFamily"
                def (fun (row: int, col: int) -> false)
            }

        let stateFamily =
            atomFamily {
                key "__datasheet__/stateFamily"
                def (fun (row: int, col: int) -> CellState.Inert)
            }

        let valueFamily =
            atomFamily {
                key "__datasheet__/valueFamily"
                def (fun (row: int, col: int) -> None)
            }

        let exprFamily =
            atomFamily {
                key "__datasheet__/exprFamily"
                def (fun (row: int, col: int) -> "")
            }

    type Cell with
        static member internal Create row col =
            { row = Cell.rowFamily(row,col)
              col = Cell.colFamily(row,col)
              selected = Cell.selectedFamily(row,col)
              state = Cell.stateFamily(row,col)
              value = Cell.valueFamily(row,col)
              expr = Cell.exprFamily(row,col) }

    let internal cellFamily =
        atomFamily {
            key "__datasheet__/cellFamily"
            def (fun (row: int, col: int) -> Cell.Create row col)
        }

    let rowCount =
        atom {
            key "__datasheet__/rowCount"
            def 10
        }

    let colCount =
        atom {
            key "__datasheet__/colCount"
            def 10
        }

    let internal selected =
        atom {
            key "__datasheet__/selected"
            def (None: (int * int) option)
        }

    let internal editing =
        atom {
            key "__datasheet__/editing"
            def (None: (int * int) option)
        }

module Selectors =
    let private findCell =
        selectorFamily {
            key "__datasheet__/findCell"
            get (fun (row: int, col: int) getter -> 
                getter.get(Atoms.cellFamily(row, col)))
            set (fun (row: int, col: int) setter (newCell: Cell) ->
                setter.set(Atoms.cellFamily(row, col), newCell))
        }

    let editTracker =
        selector {
            key "__datasheet__/editTracker"
            get (fun getter -> getter.get(Atoms.editing))
            set (fun setter (newValue: (int * int) option) -> 
                setter.get(Atoms.editing)
                |> Option.iter(fun (row,col) ->
                    setter.get(findCell(row,col)).state
                    |> fun res -> setter.set(res, CellState.Inert))
                setter.set(Atoms.editing, newValue))
        }

    let selectedTracker =
        selector {
            key "__datasheet__/selectedTracker"
            get (fun getter -> getter.get(Atoms.selected))
            set (fun setter (newValue: (int * int) option) -> 
                setter.get(Atoms.selected)
                |> Option.iter(fun (row,col) ->
                    setter.get(findCell(row,col)).selected
                    |> fun res -> setter.set(res, false))
                setter.set(Atoms.selected, newValue))
        }

    module Cell =
        let row =
            selectorFamily {
                key "__datasheet__/getRow"
                get (fun (row: int, col:int) getter -> 
                    getter.get(findCell(row,col)).row
                    |> getter.get)
                set (fun (row: int, col:int) setter (newValue: int) ->
                    let row = setter.get(findCell(row,col)).row
                    setter.set(row, newValue))
            }

        let col =
            selectorFamily {
                key "__datasheet__/getCol"
                get (fun (row: int, col:int) getter -> 
                    getter.get(findCell(row,col)).col
                    |> getter.get)
                set (fun (row: int, col:int) setter (newValue: int) ->
                    let col = setter.get(findCell(row,col)).col
                    setter.set(col, newValue))
            }

        let sel =
            selectorFamily {
                key "__datasheet__/getSel"
                get (fun (row: int, col:int) getter -> 
                    getter.get(findCell(row,col)).selected
                    |> getter.get)
                set (fun (row: int, col:int) setter (newValue: bool) ->
                    if newValue then
                        setter.set(selectedTracker, Some(row,col))
                    let selected = setter.get(findCell(row,col)).selected
                    setter.set(selected, newValue))
            }

        let state =
            selectorFamily {
                key "__datasheet__/getState"
                get (fun (row: int, col:int) getter ->
                    getter.get(findCell(row,col)).state
                    |> getter.get)
                set (fun (row: int, col:int) setter (newValue: CellState) ->
                    if newValue = CellState.Editing then
                        setter.set(editTracker, Some(row,col))
                    let state = setter.get(findCell(row,col)).state
                    setter.set(state, newValue))
            }

        let expr =
            selectorFamily {
                key "__datasheet__/getExpr"
                get (fun (row: int, col:int) getter -> 
                    getter.get(findCell(row,col)).expr
                    |> getter.get)
                set (fun (row: int, col:int) setter (newValue: string) ->
                    let expr = setter.get(findCell(row,col)).expr
                    setter.set(expr, newValue))
            }

        let value =
            selectorFamily {
                key "__datasheet__/getValue"
                get (fun (row: int, col:int) getter ->
                    let rec evaluate (currentRefs: Set<Evaluator.Position>) exp =
                        match exp with
                        | Evaluator.Number num -> Some num
                        | Evaluator.Binary(l, op, r) -> 
                            let left = evaluate currentRefs l
                            let right = evaluate currentRefs r
                            match op with
                            | '-' -> Option.map2 (fun l r -> l - r) left right
                            | '+' -> Option.map2 (fun l r -> l + r) left right
                            | '/' -> Option.map2 (fun l r -> l / r) left right
                            | '*' -> Option.map2 (fun l r -> l * r) left right
                            | _ -> None
                        | Evaluator.Reference (cCol,row) ->
                            let col = (cCol |> int) - 65
                            let row = row - 1

                            if currentRefs.Contains(cCol,row) then None
                            else
                                getter.get(expr(row, col)).ToCharArray()
                                |> Evaluator.parse
                                |> Option.bind (evaluate currentRefs)

                    Evaluator.parse (getter.get(expr(row, col)).ToCharArray())
                    |> Option.bind (evaluate ((Set.empty<Evaluator.Position>).Add(char (col + 56), row)))
                )
            }
