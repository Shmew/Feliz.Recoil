namespace Feliz.Recoil.DataSheet

open Fable.Core
open Feliz
open Feliz.Recoil

module DataSheet =
    let cellEditor = React.memo(fun (input: {| col: int; row: int |}) ->
        let cellExpr,setCellExpr = Recoil.useState(Selectors.Cell.expr(input.row, input.col))
        let inputRef = React.useInputRef()

        React.useLayoutEffect(fun () -> inputRef.current |> Option.iter(fun e -> e.focus()))

        Html.input [
            prop.classes [ Bulma.Input ]
            prop.style [
                style.padding 0
                style.height 40
                style.borderRadius 0
            ]
            prop.ref inputRef
            prop.type'.text
            prop.value cellExpr
            prop.onTextChange setCellExpr
        ])

    let cellViewer = React.functionComponent(fun (input: {| col: int; row: int |}) ->
        let cellValue = Recoil.useValue(Selectors.Cell.value(input.row, input.col))
        let expr = Recoil.useValue(Selectors.Cell.expr(input.row, input.col))

        let cellValue =
            match cellValue with
            | Some i -> unbox<string> i
            | None ->
                match expr with
                | "" -> ""
                | _ when expr.StartsWith("=") -> "#ERR"
                | _ -> expr

        Html.div [
            prop.classes [
                if cellValue = "#ERR" then
                    Bulma.IsSelected
                    Bulma.HasBackgroundDanger
                elif expr.StartsWith("=") then
                    Bulma.IsSelected
                    Bulma.HasBackgroundLight
            ]
            prop.children [
                Html.span [
                    prop.text cellValue
                ]
            ]
        ])
    
    let cellComp = React.memo(fun (input: {| col: int; row: int |}) ->
        let cellState,setCellState = Recoil.useState(Selectors.Cell.state(input.row, input.col))
        let selected,setSelected = Recoil.useState(Selectors.Cell.sel(input.row, input.col))
        let editting = Recoil.useSetState(Selectors.editTracker)

        Html.tableCell [
            prop.style [
                if selected && cellState <> CellState.Editing then 
                    style.outlineColor "#3273dc"
                    style.outlineStyle.auto
                    style.outlineWidth.initial
                if cellState = CellState.Editing then
                    style.padding 0
                    style.paddingBottom 0
            ]
            prop.onKeyDown <| fun ev ->
                if int ev.keyCode = Keys.Enter then
                    setCellState CellState.Inert
            prop.onClick <| fun _ ->
                setSelected true
                editting None
            prop.onDoubleClick <| fun ev ->
                ev.preventDefault()
                if cellState = CellState.Inert then
                    setCellState CellState.Editing
            prop.children [
                match cellState with
                | CellState.Editing -> cellEditor input
                | CellState.Inert -> cellViewer input
            ]
        ])

    let lazyCell = React.memo(fun (input: {| col: int; row: int |}) ->
        let activated,setActivated = React.useState false
        let setSelected = 
            Recoil.useCallbackRef(fun setter -> 
                setter.set(Selectors.Cell.sel(input.row, input.col), true)
                setter.set(Selectors.editTracker, None))

        if activated then cellComp input
        else 
            Html.tableCell [
                prop.onClick <| fun _ ->
                    setActivated true
                    setSelected()
                prop.children [
                    Html.span [
                        prop.style [
                            style.textOverflow.ellipsis
                        ]
                        prop.text ""
                    ]
                ] 
            ]
        )

    let rowComp = React.memo(fun (input: {| index: int; cols: int |}) ->
        Html.tableRow [
            prop.children [
                Html.tableCell [
                    prop.text (unbox<string> (input.index + 1))
                ]
                yield!
                    [ 0 .. (input.cols - 1) ]
                    |> List.map (fun col -> lazyCell {| col = col; row = input.index |})
            ]
        ])

    let dataSheetHeaders = React.memo(fun (input: {| cols: int |}) ->
        Html.tableRow [
            prop.children [
                Html.tableHeader [
                    prop.text ""
                ]
                yield!
                    [ 65 .. (64 + input.cols) ]
                    |> List.map (fun col ->
                        Html.tableHeader [
                            prop.text (unbox<string>(char col))
                        ])
            ]
        ])

    let activeTableEditor = React.functionComponent(fun (input: {| col: int; row: int |}) ->
        let cellValue,setCellValue = Recoil.useState(Selectors.Cell.expr(input.row, input.col))

        Html.input [
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.value cellValue
            prop.onTextChange setCellValue
        ])

    let tableEditor = React.functionComponent(fun () ->
        let selectedCell = Recoil.useValue(Selectors.selectedTracker)

        match selectedCell with
        | Some(row,col) ->
            activeTableEditor {| col = col; row = row |}
        | None ->
            Html.input [
                prop.classes [ Bulma.Input ]
                prop.type'.text
                prop.disabled true
            ])

    let dataSheet = React.functionComponent(fun () ->
        let rows = Recoil.useValue(Atoms.rowCount)
        let cols = Recoil.useValue(Atoms.colCount)

        Html.div [
            tableEditor()
            Html.table [
                prop.classes [
                    Bulma.Table 
                    Bulma.IsFullwidth
                    Bulma.IsBordered
                ]
                prop.style [
                    style.tableLayout.fixed'
                ]
                prop.children [
                    Html.thead [
                        dataSheetHeaders {| cols = cols |}
                    ]
                    Html.tableBody (
                        [ 0 .. (rows - 1) ]
                        |> List.map (fun row -> rowComp {| index = row; cols = cols |})
                    )
                ]
            ]
        ])
