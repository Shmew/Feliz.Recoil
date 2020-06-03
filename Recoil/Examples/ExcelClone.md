# Feliz.Recoil - Excel Clone

This example shows how performant the library can be,
all cells are controlled components, with no debouncing or
throttling.

This example supports basic operators like + - / * and basic
referencing such as `=A1+B1`.

```fsharp:recoil-excelclone
open Feliz
open Feliz.Recoil
open Feliz.Recoil.DataSheet

// The code is in the project repo, it's too large for this page.

let render = React.functionComponent(fun () ->
    Recoil.root [
        Recoil.logger()
        DataSheet.dataSheet()
    ])
```
