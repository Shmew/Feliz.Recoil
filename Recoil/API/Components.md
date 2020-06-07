# Feliz.Recoil - API Reference

## Components

### Recoil.logger

Enables console debugging when in development.

Similar to `React.strictMode`, this will do nothing
in production.

This must be a descendant of a `Recoil.root` component.

If you have persistence settings already enabled, they will
already log, this below usage is for non-persisted atoms.

Usage:
```fs

let myAtom = 
    Recoil.atom (
        "myAtomKey", 
        1, 
        { Type = PersistenceType.Log
          Backbutton = None
          Validator = (fun _ -> None) }
    )

// or

let myOtherAtom =
    atom {
        key "myOtherAtomKey"
        def 1
        log
    }

...
Recoil.root [
    Recoil.logger()
    ...
]
```

### Recoil.root

Provides the context in which atoms have values. 

Must be an ancestor of any component that uses any Recoil hooks. 

Multiple roots may co-exist; atoms will have distinct values 
within each root. If they are nested, the innermost root will 
completely mask any outer roots.

Signature:
```fs
type RootInitializer =
    /// Sets the initial value of a single atom to the provided value.
    member set (recoilValue: RecoilValue<'T,ReadWrite>, currentValue: 'T) : unit
    /// Sets the initial value for any number of atoms whose keys are the
    /// keys in the provided map. 
    ///
    /// As with useSetUnvalidatedAtomValues, the validator for each atom will be 
    /// called when it is next read, and setting an atom without a configured 
    /// validator will result in an exception.
    ///
    /// Setting the logging option automatically adds the logger in the component.
    member setUnvalidatedAtomValues (atomValues: Map<string,'T>) : unit
    member setUnvalidatedAtomValues (atomValues: (string * 'T) list) : unit

(children: ReactElement list) -> ReactElement

(props: IRootProperty list) -> ReactElement

type root =
    children: (children: ReactElement list) : IRootProperty

    /// Enables logging for any atoms with a set persistence type.
    ///
    /// This will be adjusted later, see: https://github.com/facebookexperimental/Recoil/issues/277
    log : IRootProperty

    /// A function that will be called when the root is first rendered, 
    /// which can set initial values for atoms.
    init (initializer: RootInitializer -> unit) : IRootProperty

    /// Allows you to hydrate atoms from the your local storage, those atoms 
    /// will then be observed and the local storage will be written to on any 
    /// state changes.
    localStorage: (initializer: Storage.Hydrator -> unit) : IRootProperty

    /// Allows you to hydrate atoms from the your session storage, those atoms 
    /// will then be observed and the session storage will be written to on any 
    /// state changes.
    sessionStorage: (initializer: Storage.Hydrator -> unit) : IRootProperty
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
        root.log

        root.children [
            Html.div [
                ...
            ]
            ...
        ]
    ])
```
