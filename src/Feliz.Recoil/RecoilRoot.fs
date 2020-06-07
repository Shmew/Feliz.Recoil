namespace Feliz.Recoil

open Browser.WebStorage
open Fable.Core.JsInterop
open Feliz
open System.ComponentModel

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never)>]
module RecoilRoot =
    [<AutoOpen;EditorBrowsable(EditorBrowsableState.Never)>]
    module Types =
        type IRootProperty = interface end

        type RootProps =
            abstract children: ReactElement list
            abstract logging: bool option
            abstract initializer: (RootInitializer -> unit) option
            abstract useLocalStorage: (Storage.Hydrator -> unit) option
            abstract useSessionStorage: (Storage.Hydrator -> unit) option
    
    [<RequireQualifiedAccess;EditorBrowsable(EditorBrowsableState.Never)>]
    module Interop =
        let mkRootAttr (key: string) (value: obj) = unbox<IRootProperty>(key, value)
    
    type root =
        static member inline children (children: ReactElement list) = Interop.mkRootAttr  "children" children

        /// Enables logging for any atoms with a set persistence type.
        ///
        /// This will be adjusted later, see: https://github.com/facebookexperimental/Recoil/issues/277
        static member inline log = Interop.mkRootAttr "logging" true

        /// A function that will be called when the root is first rendered, 
        /// which can set initial values for atoms.
        static member inline init (initializer: RootInitializer -> unit) = Interop.mkRootAttr "initializer" initializer

        /// Allows you to hydrate atoms from the your local storage, those atoms 
        /// will then be observed and the local storage will be written to on any 
        /// state changes.
        static member inline localStorage (initializer: Storage.Hydrator -> unit) = Interop.mkRootAttr "useLocalStorage" initializer

        /// Allows you to hydrate atoms from the your session storage, those atoms 
        /// will then be observed and the session storage will be written to on any 
        /// state changes.
        static member inline sessionStorage (initializer: Storage.Hydrator -> unit) = Interop.mkRootAttr "useSessionStorage" initializer

    type Recoil with
        /// Provides the context in which atoms have values. 
        /// 
        /// Must be an ancestor of any component that uses any Recoil hooks. 
        /// 
        /// Multiple roots may co-exist; atoms will have distinct values 
        /// within each root. If they are nested, the innermost root will 
        /// completely mask any outer roots.
        static member inline root (children: ReactElement list) =
            Bindings.Recoil.RecoilRoot(createObj [
                "children" ==> Interop.reactApi.Children.toArray(children)
            ])
        /// Provides the context in which atoms have values. 
        /// 
        /// Must be an ancestor of any component that uses any Recoil hooks. 
        /// 
        /// Multiple roots may co-exist; atoms will have distinct values 
        /// within each root. If they are nested, the innermost root will 
        /// completely mask any outer roots.
        static member inline root (props: IRootProperty list) =
            let props = unbox<RootProps>(createObj !!props)

            Bindings.Recoil.RecoilRoot(createObj [
                "initializeState" ==> (fun o -> 
                    if props.initializer.IsSome then
                        props.initializer.Value o
                    if props.useLocalStorage.IsSome then
                        Storage.Hydrator(o, localStorage) |> props.useLocalStorage.Value
                    if props.useSessionStorage.IsSome then
                        Storage.Hydrator(o, sessionStorage) |> props.useSessionStorage.Value
                )
                "children" ==> 
                    match props.logging, props.useLocalStorage.IsSome with
                    | Some true, true -> Interop.reactApi.Children.toArray([ Storage.observer(); Recoil.logger() ] @ props.children)
                    | Some true, false -> Interop.reactApi.Children.toArray(Recoil.logger()::props.children)
                    | None, true -> Interop.reactApi.Children.toArray(Storage.observer()::props.children)
                    | _ -> Interop.reactApi.Children.toArray(props.children)
            ])
