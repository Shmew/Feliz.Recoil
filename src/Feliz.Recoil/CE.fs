namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module ComputationExpressions =
    //[<EditorBrowsable(EditorBrowsableState.Never)>]
    [<RequireQualifiedAccess>]
    module SelectorState =
        type Empty = interface end

        type Key = | Value of string

        type ICacheImplementer<'V,'T> =
            abstract member SetCache : CacheImplementation<'V> -> 'T

        [<NoEquality;NoComparison>]
        type ReadOnly<'V> = 
            { Key: string
              Get: (SelectorGetter -> 'V)
              Cache: CacheImplementation<'V> option }

            interface ICacheImplementer<'V,ReadOnly<'V>> with
                member this.SetCache (cache: CacheImplementation<'V>) =
                    { this with Cache = Some cache }

        [<NoEquality;NoComparison>]
        type ReadWrite<'T,'V> =
            { Key: string
              Get: SelectorGetter -> 'V
              Set: SelectorMethods -> 'T -> unit
              Cache: CacheImplementation<'V> option }

            interface ICacheImplementer<'V,ReadWrite<'T,'V>> with
                member this.SetCache (cache: CacheImplementation<'V>) =
                    { this with Cache = Some cache }

    type SelectorBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<SelectorState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: SelectorState.Empty, value: string) = 
            SelectorState.Key.Value value

        [<CustomOperation("get")>]
        member inline _.Get (SelectorState.Key.Value state, (f: SelectorGetter -> 'U)) : SelectorState.ReadOnly<'U> = 
            { Key = state
              Get = f
              Cache = None }

        [<CustomOperation("set")>]
        member inline _.Set (state: SelectorState.ReadOnly<'U>, (f: SelectorMethods -> 'T -> unit)) : SelectorState.ReadWrite<'T,'U> =
            { Key = state.Key
              Get = state.Get
              Set = f 
              Cache = state.Cache }

        [<CustomOperation("cache")>]
        member inline _.Cache (state: SelectorState.ICacheImplementer<'V,_>, (cacheImplementation: CacheImplementation<'V>)) = 
            state.SetCache(cacheImplementation)
            
        member inline _.Run (selector: SelectorState.ReadOnly<JS.Promise<'T>>) =
            Recoil.selector(selector.Key, selector.Get)
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,JS.Promise<'T>>) =
            Recoil.selector(selector.Key, selector.Get, selector.Set)
        
        member inline _.Run (selector: SelectorState.ReadOnly<Async<'T>>) =
            Recoil.selector(selector.Key, selector.Get)
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,Async<'T>>) =
            Recoil.selector(selector.Key, selector.Get, selector.Set)
        
        member inline _.Run (selector: SelectorState.ReadOnly<RecoilValue<'T,_>>) =
            Recoil.selector(selector.Key, selector.Get)
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,RecoilValue<'T,_>>) =
            Recoil.selector(selector.Key, selector.Get, selector.Set)

    [<AutoOpen>]
    module SelectorBuilderMagic =
        type SelectorBuilder with
            member inline _.Run (selector: SelectorState.ReadOnly<'T>) =
                Recoil.selector(selector.Key, selector.Get)

            member inline _.Run (selector: SelectorState.ReadWrite<'T,'T>) =
                Recoil.selector(selector.Key, selector.Get, selector.Set)

    let selector = SelectorBuilder()    

    //[<EditorBrowsable(EditorBrowsableState.Never)>]
    [<RequireQualifiedAccess>]
    module AtomState =
        type Empty = interface end

        type Key = | Value of string

        type ReadWrite<'T> = 
            { Key: string
              Def: 'T }

        [<NoEquality;NoComparison>]
        type ReadWriteWithPersistence<'T,'U> =
            { Key: string
              Def: 'T
              Persist: PersistenceSettings<'T,'U> }
    
    type AtomBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<AtomState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: AtomState.Empty, value: string) = 
            AtomState.Key.Value value
            
        [<CustomOperation("def")>]
        member _.Default (AtomState.Key.Value state, v: 'T) : AtomState.ReadWrite<'T> = 
            { Key = state
              Def = v }

        [<CustomOperation("log")>]
        member _.Log (state: AtomState.ReadWrite<'T>) : AtomState.ReadWriteWithPersistence<'T,'U> = 
            { Key = state.Key
              Def = state.Def
              Persist = 
                { Type = PersistenceType.Log
                  Backbutton = None
                  Validator = (fun _ -> None) } }

        [<CustomOperation("persist")>]
        member _.Persist (state: AtomState.ReadWrite<'T>, settings: PersistenceSettings<'T,'U>) : AtomState.ReadWriteWithPersistence<'T,'U> = 
            { Key = state.Key
              Def = state.Def
              Persist = settings }

        member inline _.Run (atom: AtomState.ReadWrite<JS.Promise<'T>>) =
            Recoil.atom(atom.Key, atom.Def)
        member inline _.Run (atom: AtomState.ReadWriteWithPersistence<JS.Promise<'T>,'U>) =
            Recoil.atom(atom.Key, atom.Def, atom.Persist)

        member inline _.Run (atom: AtomState.ReadWrite<Async<'T>>) =
            Recoil.atom(atom.Key, atom.Def)
        member inline _.Run (atom: AtomState.ReadWriteWithPersistence<Async<'T>,'U>) =
            Recoil.atom(atom.Key, atom.Def, atom.Persist)

        member inline _.Run (atom: AtomState.ReadWrite<RecoilValue<'T,_>>) =
            Recoil.atom(atom.Key, atom.Def)
        member inline _.Run (atom: AtomState.ReadWriteWithPersistence<RecoilValue<'T,_>,'U>) =
            Recoil.atom(atom.Key, atom.Def, atom.Persist)

    [<AutoOpen>]
    module AtomBuilderMagic =
        type AtomBuilder with
            member inline _.Run<'T> (atom: AtomState.ReadWrite<'T>) =
                Recoil.atom(atom.Key, atom.Def)
            member inline _.Run<'T,'U> (atom: AtomState.ReadWriteWithPersistence<'T,'U>) =
                Recoil.atom(atom.Key, atom.Def, atom.Persist)

    let atom = AtomBuilder()
