# Feliz.Recoil - API Reference

## Components

### Recoil.contextBridge

A component which you can use instead of Recoil.root in your nested React root to share the same 
consistent Recoil store state. 

As with any state sharing across React roots, changes may not be perfectly synchronized in all cases.

Signature:
```fs
(children: ReactElement list) -> ReactElement
```

### Recoil.root

Provides the context in which atoms have values. 

Must be an ancestor of any component that uses any Recoil hooks. 

Multiple roots may co-exist; atoms will have distinct values 
within each root. If they are nested, the innermost root will 
completely mask any outer roots.

Signature:
```fs
(children: ReactElement list) -> ReactElement

(props: IRootProperty list) -> ReactElement

type root =
    children: (children: ReactElement list) : IRootProperty

    /// A function that will be called when the root is first rendered, 
    /// which can set initial values for atoms.
    init (initializer: MutableSnapshot -> unit) : IRootProperty

    /// Enable time traveling via the useTimeTravel hook in children.
    timeTravel: (value: bool) : IRootProperty
    timeTravel (properties: ITimeTravelProperty list) : IRootProperty

type timeTravel =
    /// Sets the max history buffer.
    maxHistory: (value: int) : ITimeTravelProperty
```

Usage:
```fs
let myComp = React.functionComponent(fun () ->
    Recoil.root [
        Html.div [
            ...
        ]
        ...
    ])

let myComp = React.functionComponent(fun () ->
    Recoil.root [
        root.timeTravel [
            timeTravel.maxHistory 10
        ]

        root.children [
            Html.div [
                ...
            ]
            ...
        ]
    ])
```
