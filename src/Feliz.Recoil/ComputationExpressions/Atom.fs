namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JS
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
              Effects: AtomEffect<'U,ReadWrite> list
              Persist: PersistenceSettings<'U,'V> option
              DangerouslyAllowMutability: bool option }
    
    type AtomBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<AtomState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: AtomState.Empty, value: string) = 
            AtomState.Key.Value value
            
        [<CustomOperation("def")>]
        member _.Default (AtomState.Key.Value state, v: JS.Promise<'U>) : AtomState.ReadWrite<JS.Promise<'U>,'U,_> = 
            { Key = state
              Def = v
              Effects = []
              Persist = None
              DangerouslyAllowMutability = None }
        member _.Default (AtomState.Key.Value state, v: Async<'U>) : AtomState.ReadWrite<Async<'U>,'U,_> = 
            { Key = state
              Def = v
              Effects = []
              Persist = None
              DangerouslyAllowMutability = None }
        member _.Default (AtomState.Key.Value state, v: RecoilValue<'U,#ReadOnly>) : AtomState.ReadWrite<RecoilValue<'U,#ReadOnly>,'U,_> = 
            { Key = state
              Def = v
              Effects = []
              Persist = None
              DangerouslyAllowMutability = None }

        [<CustomOperation("effect")>]
        member _.Effect (state: AtomState.ReadWrite<'T,'U,_>, f: Effector<'U,ReadWrite> -> unit) : AtomState.ReadWrite<'T,'U,_> = 
            { state with Effects = (AtomEffect<'U,ReadWrite> f)::state.Effects }
        member _.Effect (state: AtomState.ReadWrite<'T,'U,_>, f: Effector<'U,ReadWrite> -> System.IDisposable) : AtomState.ReadWrite<'T,'U,_> = 
            { state with Effects = (AtomEffect<'U,ReadWrite> f)::state.Effects }

        [<CustomOperation("local_storage")>]
        member inline this.LocalStorage (state: AtomState.ReadWrite<'T,_,_>) : AtomState.ReadWrite<'T,'U,'V> = 
            this.Effect(state, Storage.local)

        [<CustomOperation("session_storage")>]
        member inline this.SessionStorage (state: AtomState.ReadWrite<'T,_,_>) : AtomState.ReadWrite<'T,'U,'V> = 
            this.Effect(state, Storage.session)

        [<CustomOperation("log")>]
        member inline this.Log (state: AtomState.ReadWrite<'T,_,_>) : AtomState.ReadWrite<'T,'U,'V> = 
            #if DEBUG
            this.Effect(state, Logger.effect)
            #else
            state
            #endif

        [<CustomOperation("persist")>]
        member _.Persist (state: AtomState.ReadWrite<'T,_,_>, settings: PersistenceSettings<'U,'V>) : AtomState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Effects = state.Effects
              Persist = Some settings
              DangerouslyAllowMutability = state.DangerouslyAllowMutability }

        [<CustomOperation("dangerouslyAllowMutability")>]
        member _.DangerouslyAllowMutability (state: AtomState.ReadWrite<'T,'U,'V>) : AtomState.ReadWrite<'T,'U,'V> = 
            { Key = state.Key
              Def = state.Def
              Effects = state.Effects
              Persist = state.Persist
              DangerouslyAllowMutability = Some true }

        member inline _.Run (atom: AtomState.ReadWrite<JS.Promise<'U>,'U,'V>) =
            Recoil.atom (
                atom.Key,
                atom.Def,
                effects = atom.Effects,
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomState.ReadWrite<Async<'U>,'U,'V>) =
            Recoil.atom (
                atom.Key, 
                atom.Def, 
                effects = atom.Effects,
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

        member inline _.Run (atom: AtomState.ReadWrite<RecoilValue<'U,#ReadOnly>,'U,'V>) : RecoilValue<'U,ReadWrite> =
            Recoil.atom (
                atom.Key, 
                atom.Def, 
                effects = atom.Effects,
                ?persistence = atom.Persist, 
                ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
            )

    [<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
    module AtomBuilderMagic =
        type AtomBuilder with
            member inline _.Default (AtomState.Key.Value state, v: 'T) : AtomState.ReadWrite<'T,'T,_> = 
                { Key = state
                  Def = v
                  Effects = []
                  Persist = None
                  DangerouslyAllowMutability = None }

            member inline _.Run<'T,'V> (atom: AtomState.ReadWrite<'T,'T,'V>) =
                Recoil.atom (
                    atom.Key, 
                    atom.Def, 
                    effects = atom.Effects,
                    ?persistence = atom.Persist, 
                    ?dangerouslyAllowMutability = atom.DangerouslyAllowMutability
                )

    let atom = AtomBuilder()
