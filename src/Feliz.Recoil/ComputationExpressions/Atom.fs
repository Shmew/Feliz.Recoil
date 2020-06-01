namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module AtomCE =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<RequireQualifiedAccess>]
    module AtomState =
        type Empty = interface end

        type Key = | Value of string

        [<NoEquality;NoComparison>]
        type ReadWrite<'T,'U,'V> = 
            { Key: string
              Def: 'T
              Persist: PersistenceSettings<'U,'V> option
              DangerouslyAllowMutability: bool option }
    
    type AtomBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<AtomState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: AtomState.Empty, value: string) = 
            AtomState.Key.Value value
            
        [<CustomOperation("def")>]
        member _.Default (AtomState.Key.Value state, v: 'T) : AtomState.ReadWrite<'T,_,_> = 
            { Key = state
              Def = v
              Persist = None
              DangerouslyAllowMutability = None }

        [<CustomOperation("log")>]
        member _.Log (state: AtomState.ReadWrite<'T,_,_>) : AtomState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Persist = 
                { Type = PersistenceType.Log
                  Backbutton = None
                  Validator = (fun _ -> None) }
                |> Some
              DangerouslyAllowMutability = state.DangerouslyAllowMutability }

        [<CustomOperation("persist")>]
        member _.Persist (state: AtomState.ReadWrite<'T,_,_>, settings: PersistenceSettings<'U,'V>) : AtomState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Persist = Some settings
              DangerouslyAllowMutability = state.DangerouslyAllowMutability }

        [<CustomOperation("dangerouslyAllowMutability")>]
        member _.DangerouslyAllowMutability (state: AtomState.ReadWrite<'T,'U,'V>, value: bool) : AtomState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Persist = state.Persist
              DangerouslyAllowMutability = Some value }

        member inline _.Run (atom: AtomState.ReadWrite<JS.Promise<'T>,'T,'V>) =
            Recoil.atom (
                atom.Key,
                atom.Def,
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomState.ReadWrite<Async<'T>,'T,'V>) =
            Recoil.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomState.ReadWrite<RecoilValue<'T,'Mode>,'T,'V>) : RecoilValue<'T,ReadWrite> =
            Recoil.atom (
                atom.Key, 
                atom.Def, 
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

    [<AutoOpen>]
    module AtomBuilderMagic =
        type AtomBuilder with
            member inline _.Run<'T,'V> (atom: AtomState.ReadWrite<'T,'T,'V>) =
                Recoil.atom (
                    atom.Key, 
                    atom.Def, 
                    ?persistence = atom.Persist, 
                    ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
                )

    let atom = AtomBuilder()
