namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open System.ComponentModel

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
module RecoilRoot =
    [<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
    module Types =
        type IRootProperty = interface end
        type ITimeTravelProperty = interface end

        type TimeTravelProps =
            abstract maxHistory: int option

        type RootProps =
            abstract children: ReactElement list
            abstract initializer: (MutableSnapshot -> unit) option
            abstract timeTravel: TimeTravelProps option
    
    [<Erase;RequireQualifiedAccess;EditorBrowsable(EditorBrowsableState.Never)>]
    module Interop =
        let inline mkRootAttr (key: string) (value: obj) = unbox<IRootProperty>(key, value)
        let inline mkTimeTravelAttr (key: string) (value: obj) = unbox<ITimeTravelProperty>(key, value)
    
    [<Erase>]
    type root =
        static member inline children (children: ReactElement list) = Interop.mkRootAttr  "children" children

        /// A function that will be called when the root is first rendered, 
        /// which can set initial values for atoms.
        static member inline init (initializer: MutableSnapshot -> unit) = Interop.mkRootAttr "initializer" initializer

        /// Enable time traveling via the useTimeTravel hook in children.
        static member inline timeTravel (value: bool) = Interop.mkRootAttr "timeTravel" (if value then Some (createObj !![]) else None)
        /// Enable time traveling via the useTimeTravel hook in children.
        static member inline timeTravel (properties: ITimeTravelProperty list) = Interop.mkRootAttr "timeTravel" (createObj !!properties)

    [<Erase>]
    type timeTravel =
        /// Sets the max history buffer.
        static member inline maxHistory (value: int) = Interop.mkTimeTravelAttr "maxHistory" value

    type Recoil with
        /// Provides the context in which atoms have values. 
        /// 
        /// Must be an ancestor of any component that uses any Recoil hooks. 
        /// 
        /// Multiple roots may co-exist; atoms will have distinct values 
        /// within each root. If they are nested, the innermost root will 
        /// completely mask any outer roots.
        #if FABLE_COMPILER
        static member inline root (children: ReactElement list) =
        #else
        static member root (children: ReactElement list) =
        #endif
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
        #if FABLE_COMPILER
        static member inline root (props: IRootProperty list) =
        #else
        static member root (props: IRootProperty list) =
        #endif
            let props = unbox<RootProps>(createObj !!props)

            Bindings.Recoil.RecoilRoot(createObj [
                "initializeState" ==> (fun o -> 
                    if props.initializer.IsSome then
                        props.initializer.Value o
                )
                "children" ==> (
                    match props.timeTravel with
                    | Some timeProps ->
                        TimeTravel.rootWrapper {| otherChildren = props.children; maxHistory = timeProps.maxHistory |}
                        |> Interop.reactApi.Children.toArray
                    | _ -> Interop.reactApi.Children.toArray(props.children)
                )
            ])
