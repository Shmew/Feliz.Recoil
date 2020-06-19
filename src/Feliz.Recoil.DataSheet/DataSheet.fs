namespace Feliz.Recoil.DataSheet

open EventListeners
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
            prop.onClick <| fun _ ->
                if cellState = CellState.Inert then
                    setSelected true
                    editting None
            prop.onDoubleClick <| fun _ ->
                if cellState = CellState.Inert then
                    setCellState CellState.Editing
            prop.children [
                match cellState with
                | CellState.Editing -> cellEditor input
                | CellState.Inert -> cellViewer input
            ]
        ])

    let lazyCell = React.memo(fun (input: {| col: int; row: int; activator: IRefValue<Map<int * int, (unit -> unit)>> |}) ->
        let activated,setActivated = React.useState false
        let setSelected = 
            Recoil.useCallbackRef(fun setter -> 
                setter.set(Selectors.Cell.sel(input.row, input.col), true)
                setter.set(Selectors.editTracker, None))

        let setActivated =
            React.useCallbackRef(fun () -> 
                setActivated true
                setSelected())

        React.useEffect(fun () -> 
            input.activator.current <-
                input.activator.current.Add((input.col, input.row), setActivated)
        )

        if activated then cellComp {| col = input.col; row = input.row |}
        else 
            Html.tableCell [
                prop.onClick <| fun _ ->
                    setActivated()
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

    let rowComp = React.memo(fun (input: {| index: int; cols: int; activator: IRefValue<Map<int * int, (unit -> unit)>> |}) ->
        Html.tableRow [
            prop.children [
                Html.tableCell [
                    prop.text (unbox<string> (input.index + 1))
                ]
                yield!
                    [ 0 .. (input.cols - 1) ]
                    |> List.map (fun col -> lazyCell {| col = col; row = input.index; activator = input.activator |})
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
        let cellActivator = React.useRef(Map.empty<int * int, (unit -> unit)>)
        let timeTravel = Recoil.useTimeTravel()

        let getSelectedIfNotEditing =
            Recoil.useCallbackRef(fun setter ->
                promise {
                    let! selected = setter.snapshot.getPromise(Selectors.selectedTracker)
                    let! editing = setter.snapshot.getPromise(Selectors.editTracker)

                    return
                        match selected, editing with
                        | Some(sRow,sCol), Some(eRow,eCol) ->
                            if sRow = eRow && sCol = eCol then None
                            else Some(sRow,sCol)
                        | selected, _ -> selected
                }
            )

        let moveDirection =
            Recoil.useCallbackRef(fun (setter: CallbackMethods) (bypassEditCheck: bool) (dir: KeyDirection) ->
                promise {
                    let! selected = 
                        if bypassEditCheck then
                            setter.snapshot.getPromise(Selectors.selectedTracker)
                        else getSelectedIfNotEditing()

                    selected
                    |> Option.iter(fun (row,col) ->
                        match dir with
                        | KeyDirection.Down -> 
                            let row = (row + 1) % 10
                            cellActivator.current.TryFind(col, row) 
                            |> Option.iter (fun f -> f())
                        | KeyDirection.Left -> 
                            let col = 
                                match (col - 1) with
                                | i when i < 0 -> (cols + i) % 10
                                | i -> i % 10
                            cellActivator.current.TryFind(col, row) 
                            |> Option.iter (fun f -> f())
                        | KeyDirection.Right -> 
                            let col = (col + 1) % 10
                            cellActivator.current.TryFind(col, row) 
                            |> Option.iter (fun f -> f())
                        | KeyDirection.Up -> 
                            let row = 
                                match (row - 1) with
                                | i when i < 0 -> (rows + i) % 10
                                | i -> i % 10
                            cellActivator.current.TryFind(col, row) 
                            |> Option.iter (fun f -> f())
                    )
                }
                |> Promise.start
            )

        let enterOrLeaveEditor =
            Recoil.useCallbackRef(fun setter ->
                promise {
                    let! selected = setter.snapshot.getPromise(Selectors.selectedTracker)

                    selected
                    |> Option.iter(fun (row, col) ->
                        promise {
                            let! cellState = setter.snapshot.getPromise(Selectors.Cell.state(row, col))

                            if cellState = CellState.Editing then
                                moveDirection true KeyDirection.Down
                            else setter.set(Selectors.Cell.state(row, col), CellState.Editing)
                        }
                        |> Promise.start)
                }
            )

        let deleteCell =
            Recoil.useCallbackRef(fun setter ->
                promise {
                    let! selected = getSelectedIfNotEditing()

                    selected
                    |> Option.iter (fun (row, col) -> setter.set(Selectors.Cell.expr(row, col), ""))
                }
            )

        let keyboardHandler =
            React.useCallbackRef(fun (ev: Browser.Types.KeyboardEvent) ->
                promise {
                    EventListeners.KeyDirection.fromEv ev
                    |> Option.iter (fun (bypass, dir) -> moveDirection bypass dir)

                    match ev.key with
                    | EventListeners.Keys.Enter ->
                        do! enterOrLeaveEditor()
                    | EventListeners.Keys.Backspace when ev.key = EventListeners.Keys.Delete ->
                        do! deleteCell()
                    | EventListeners.Keys.z when ev.ctrlKey ->
                        timeTravel.backward 1
                    | EventListeners.Keys.y when ev.ctrlKey ->
                        timeTravel.forward 1
                    | _ -> ()
                }
                |> Promise.start
            )

        React.useKeyDownListener(keyboardHandler)

        Html.div [
            prop.children [
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
                            |> List.map (fun row -> rowComp {| index = row; cols = cols; activator = cellActivator |})
                        )
                    ]
                ]
            ]
        ])
