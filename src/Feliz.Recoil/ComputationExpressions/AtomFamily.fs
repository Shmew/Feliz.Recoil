namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module AtomFamilyCE =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<RequireQualifiedAccess>]
    module AtomFamilyState =
        type Empty = interface end

        type Key = | Value of string

        [<NoEquality;NoComparison>]
        type ReadWrite<'T,'U,'V,'P> = 
            { Key: string
              Def: 'T
              Effects: ('P -> AtomEffect<'U,ReadWrite> list) option
              Persist: PersistenceSettings<'U,'V> option
              DangerouslyAllowMutability: bool option }
    
    type AtomFamilyBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<AtomFamilyState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: AtomFamilyState.Empty, value: string) = 
            AtomFamilyState.Key.Value value
            
        [<CustomOperation("def")>]
        member _.Default (AtomFamilyState.Key.Value state, v: 'T) : AtomFamilyState.ReadWrite<'T,_,_,_> = 
            { Key = state
              Def = v
              Effects = None
              Persist = None
              DangerouslyAllowMutability = None }
        
        [<CustomOperation("effect")>]
        member _.Effect (state: AtomFamilyState.ReadWrite<'T,'U,_,_>, f: 'P -> Effector<'U,ReadWrite> -> unit) : AtomFamilyState.ReadWrite<'T,'U,_,'P> = 
            { state with Effects = state.Effects |> Option.map (fun xsf -> fun p ->  (AtomEffect(f p))::(xsf p)) }
        member _.Effect (state: AtomFamilyState.ReadWrite<'T,'U,_,_>, f: 'P -> Effector<'U,ReadWrite> -> System.IDisposable) : AtomFamilyState.ReadWrite<'T,'U,_,'P> = 
            { state with Effects = state.Effects |> Option.map (fun xsf -> fun p ->  (AtomEffect(f p))::(xsf p)) }

        [<CustomOperation("local_storage")>]
        member inline this.LocalStorage (state: AtomFamilyState.ReadWrite<'T,_,_,_>) : AtomFamilyState.ReadWrite<'T,'U,'V,_> = 
            this.Effect(state, fun _ e -> Storage.local e)

        [<CustomOperation("session_storage")>]
        member inline this.SessionStorage (state: AtomFamilyState.ReadWrite<'T,_,_,_>) : AtomFamilyState.ReadWrite<'T,'U,'V,_> = 
            this.Effect(state, fun _ e -> Storage.session e)

        [<CustomOperation("log")>]
        member inline this.Log (state: AtomFamilyState.ReadWrite<'T,_,_,_>) : AtomFamilyState.ReadWrite<'T,'U,'V,_> = 
            #if DEBUG
            this.Effect(state, fun _ e -> Logger.effect e)
            #else
            state
            #endif

        [<CustomOperation("persist")>]
        member _.Persist (state: AtomFamilyState.ReadWrite<'T,_,_,_>, settings: PersistenceSettings<'U,'V>) : AtomFamilyState.ReadWrite<'T,'U,'V,_> = 
            { Key = state.Key
              Def = state.Def
              Effects = state.Effects
              Persist = Some settings
              DangerouslyAllowMutability = state.DangerouslyAllowMutability }

        [<CustomOperation("dangerouslyAllowMutability")>]
        member _.DangerouslyAllowMutability (state: AtomFamilyState.ReadWrite<'T,'U,'V,_>) : AtomFamilyState.ReadWrite<'T,'U,'V,_> = 
            { Key = state.Key
              Def = state.Def
              Effects = state.Effects
              Persist = state.Persist
              DangerouslyAllowMutability = Some true }

        member inline _.Run (atom: AtomFamilyState.ReadWrite<JS.Promise<'U>,'U,'V,_>) =
            Recoil.Family.atom (
                atom.Key,
                atom.Def,
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<'P -> JS.Promise<'U>,'U,'V,'P>) =
            Recoil.Family.atom (
                atom.Key,
                atom.Def,
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<Async<'U>,'U,'V,_>) =
            Recoil.Family.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<'P -> Async<'U>,'U,'V,'P>) =
            Recoil.Family.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<RecoilValue<'U,#ReadOnly>,'U,'V,_>) : 'P -> RecoilValue<'U,ReadWrite> =
            Recoil.Family.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<'P -> RecoilValue<'U,#ReadOnly>,'U,'V,'P>) : 'P -> RecoilValue<'U,ReadWrite> =
            Recoil.Family.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

    [<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
    module AtomFamilyBuilderMagic =
        type AtomFamilyBuilder with
            member inline _.Run<'U,'V,'P> (atom: AtomFamilyState.ReadWrite<'P -> 'U,'U,'V,'P>) : 'P -> RecoilValue<'U,ReadWrite> =
                Recoil.Family.atom (
                    atom.Key, 
                    atom.Def, 
                    ?persistence = atom.Persist, 
                    ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
                )

    let atomFamily = AtomFamilyBuilder()
