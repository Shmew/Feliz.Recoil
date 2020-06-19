# Feliz.Recoil - API Reference

## Components

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

    /// Enables logging for any atoms with a set persistence type.
    ///
    /// Similar to React.StrictMode, this will do nothing in production mode.
    ///
    /// This will be adjusted later, see: https://github.com/facebookexperimental/Recoil/issues/277
    log (value: bool) : IRootProperty

    /// A function that will be called when the root is first rendered, 
    /// which can set initial values for atoms.
    init (initializer: MutableSnapshot -> unit) : IRootProperty

    /// Allows you to hydrate atoms from the your local storage, those atoms 
    /// will then be observed and the local storage will be written to on any 
    /// state changes.
    localStorage: (initializer: Storage.Hydrator -> unit) : IRootProperty

    /// Allows you to hydrate atoms from the your session storage, those atoms 
    /// will then be observed and the session storage will be written to on any 
    /// state changes.
    sessionStorage: (initializer: Storage.Hydrator -> unit) : IRootProperty

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
        root.log true
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
