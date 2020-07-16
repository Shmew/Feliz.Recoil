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
        type ReadWrite<'T,'U,'V> = 
            { Key: string
              Def: 'T
              Persist: PersistenceSettings<'U,'V> option
              DangerouslyAllowMutability: bool option }
    
    type AtomFamilyBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<AtomFamilyState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: AtomFamilyState.Empty, value: string) = 
            AtomFamilyState.Key.Value value
            
        [<CustomOperation("def")>]
        member _.Default (AtomFamilyState.Key.Value state, v: 'T) : AtomFamilyState.ReadWrite<'T,_,_> = 
            { Key = state
              Def = v
              Persist = None
              DangerouslyAllowMutability = None }

        [<CustomOperation("local_storage")>]
        member _.LocalStorage (state: AtomFamilyState.ReadWrite<'T,_,_>) : AtomFamilyState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Persist = 
                { Type = PersistenceType.LocalStorage
                  Backbutton = None
                  Validator = (fun _ -> None) }
                |> Some
              DangerouslyAllowMutability = state.DangerouslyAllowMutability }

        [<CustomOperation("session_storage")>]
        member _.SessionStorage (state: AtomFamilyState.ReadWrite<'T,_,_>) : AtomFamilyState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Persist = 
                { Type = PersistenceType.SessionStorage
                  Backbutton = None
                  Validator = (fun _ -> None) }
                |> Some
              DangerouslyAllowMutability = state.DangerouslyAllowMutability }

        [<CustomOperation("log")>]
        member _.Log (state: AtomFamilyState.ReadWrite<'T,_,_>) : AtomFamilyState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Persist = 
                { Type = PersistenceType.Log
                  Backbutton = None
                  Validator = (fun _ -> None) }
                |> Some
              DangerouslyAllowMutability = state.DangerouslyAllowMutability }

        [<CustomOperation("persist")>]
        member _.Persist (state: AtomFamilyState.ReadWrite<'T,_,_>, settings: PersistenceSettings<'U,'V>) : AtomFamilyState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Persist = Some settings
              DangerouslyAllowMutability = state.DangerouslyAllowMutability }

        [<CustomOperation("dangerouslyAllowMutability")>]
        member _.DangerouslyAllowMutability (state: AtomFamilyState.ReadWrite<'T,'U,'V>) : AtomFamilyState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Persist = state.Persist
              DangerouslyAllowMutability = Some true }

        member inline _.Run (atom: AtomFamilyState.ReadWrite<JS.Promise<'T>,'T,'V>) =
            Recoil.Family.atom (
                atom.Key,
                atom.Def,
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<'P -> JS.Promise<'T>,'T,'V>) =
            Recoil.Family.atom (
                atom.Key,
                atom.Def,
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<Async<'T>,'T,'V>) =
            Recoil.Family.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<'P -> Async<'T>,'T,'V>) =
            Recoil.Family.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<RecoilValue<'T,#ReadOnly>,'T,'V>) : 'P -> RecoilValue<'T,ReadWrite> =
            Recoil.Family.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomFamilyState.ReadWrite<'P -> RecoilValue<'T,#ReadOnly>,'T,'V>) : 'P -> RecoilValue<'T,ReadWrite> =
            Recoil.Family.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

    [<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
    module AtomFamilyBuilderMagic =
        type AtomFamilyBuilder with
            member inline _.Run<'T,'V,'P> (atom: AtomFamilyState.ReadWrite<'P -> 'T,'T,'V>) : 'P -> RecoilValue<'T,ReadWrite> =
                Recoil.Family.atom (
                    atom.Key, 
                    atom.Def, 
                    ?persistence = atom.Persist, 
                    ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
                )

    let atomFamily = AtomFamilyBuilder()
