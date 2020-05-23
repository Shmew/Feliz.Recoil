namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module ComputationExpressions =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<RequireQualifiedAccess>]
    module SelectorState =
        type Empty = interface end

        type Key = | Value of string

        type ICacheImplementer<'T,'U,'V> =
            abstract member SetCache : CacheImplementation<'T,'U> -> 'V

        [<NoEquality;NoComparison>]
        type ReadOnly<'T,'U> = 
            { Key: string
              Get: (SelectorGetter -> 'U)
              Cache: CacheImplementation<'T,'U> option }

            interface ICacheImplementer<'T,'U,ReadOnly<'T,'U>> with
                member this.SetCache cache =
                    { Key = this.Key
                      Get = this.Get
                      Cache = Some cache }

        [<NoEquality;NoComparison>]
        type ReadWrite<'T,'U> =
            { Key: string
              Get: SelectorGetter -> 'U
              Set: SelectorMethods -> 'T -> unit
              Cache: CacheImplementation<'T,'U> option }

            interface ICacheImplementer<'T,'U,ReadWrite<'T,'U>> with
                member this.SetCache (cache: CacheImplementation<'T,'U>) =
                    { this with Cache = Some cache }

    type SelectorBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<SelectorState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: SelectorState.Empty, value: string) = 
            SelectorState.Key.Value value

        [<CustomOperation("get")>]
        member inline _.Get (SelectorState.Key.Value state, (f: SelectorGetter -> 'U)) : SelectorState.ReadOnly<'T,'U> = 
            { Key = state
              Get = f
              Cache = None }

        [<CustomOperation("set")>]
        member inline _.Set (state: SelectorState.ReadOnly<'T,'U>, (f: SelectorMethods -> 'T -> unit)) : SelectorState.ReadWrite<'T,'U> =
            { Key = state.Key
              Get = state.Get
              Set = f 
              Cache = state.Cache }

        [<CustomOperation("cache")>]
        member inline _.Cache (state: SelectorState.ICacheImplementer<'T,'U,_>, (cacheImplementation: CacheImplementation<'T,'U>)) = 
            state.SetCache(cacheImplementation)

        [<CustomOperation("no_cache")>]
        member inline _.NoCache (state: SelectorState.ICacheImplementer<'T,'U,_>) = 
            state.SetCache(NoCache())
            
        member inline _.Run (selector: SelectorState.ReadOnly<'T,JS.Promise<'T>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache
            )
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,JS.Promise<'T>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache
            )
        
        member inline _.Run (selector: SelectorState.ReadOnly<'T,Async<'T>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache
            )
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,Async<'T>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache
            )
        
        member inline _.Run (selector: SelectorState.ReadOnly<'T,RecoilValue<'T,'Mode>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache
            )
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,RecoilValue<'T,'Mode>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache
            )

    [<AutoOpen>]
    module SelectorBuilderMagic =
        type SelectorBuilder with
            member inline _.Run (selector: SelectorState.ReadOnly<'T,'T>) =
                Recoil.selector (
                    selector.Key, 
                    selector.Get, 
                    ?cacheImplementation = selector.Cache
                )

            member inline _.Run (selector: SelectorState.ReadWrite<'T,'T>) =
                Recoil.selector (
                    selector.Key, 
                    selector.Get, 
                    selector.Set, 
                    ?cacheImplementation = selector.Cache
                )

    let selector = SelectorBuilder()    

    //[<EditorBrowsable(EditorBrowsableState.Never)>]
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
