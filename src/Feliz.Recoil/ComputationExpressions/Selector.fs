namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module SelectorCE =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<RequireQualifiedAccess>]
    module SelectorState =
        type Empty = interface end

        type Key = | Value of string

        type MutabilityImplementer<'V> =
            abstract member SetMutability : unit -> 'V

        type ICacheImplementer<'T,'U,'V> =
            abstract member SetCache : CacheImplementation<'T,Loadable<'T>> -> 'V

        [<NoEquality;NoComparison>]
        type ReadOnly<'T,'U> = 
            { Key: string
              Get: (SelectorGetter -> 'U)
              Cache: CacheImplementation<'T,Loadable<'T>> option
              DangerouslyAllowMutability: bool option }

            interface ICacheImplementer<'T,Loadable<'T>,ReadOnly<'T,'U>> with
                member this.SetCache cache =
                    { Key = this.Key
                      Get = this.Get
                      Cache = Some cache
                      DangerouslyAllowMutability = this.DangerouslyAllowMutability }

            interface MutabilityImplementer<ReadOnly<'T,'U>> with
                member this.SetMutability () =
                    { Key = this.Key
                      Get = this.Get
                      Cache = this.Cache
                      DangerouslyAllowMutability = Some true }

        [<NoEquality;NoComparison>]
        type ReadWrite<'T,'U> =
            { Key: string
              Get: SelectorGetter -> 'U
              Set: SelectorMethods -> 'T -> unit
              Cache: CacheImplementation<'T,Loadable<'T>> option
              DangerouslyAllowMutability: bool option }

            interface ICacheImplementer<'T,Loadable<'T>,ReadWrite<'T,'U>> with
                member this.SetCache (cache: CacheImplementation<'T,Loadable<'T>>) =
                    { this with Cache = Some cache }

            interface MutabilityImplementer<ReadWrite<'T,'U>> with
                member this.SetMutability () =
                    { this with DangerouslyAllowMutability = Some true }

        [<NoEquality;NoComparison>]
        type WriteOnly<'T> =
            { Key: string
              Set: SelectorMethods -> 'T -> unit
              DangerouslyAllowMutability: bool option }

            interface MutabilityImplementer<WriteOnly<'T>> with
                member this.SetMutability () =
                    { this with DangerouslyAllowMutability = Some true }

    type SelectorBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<SelectorState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: SelectorState.Empty, value: string) = 
            SelectorState.Key.Value value

        [<CustomOperation("get")>]
        member inline _.Get (SelectorState.Key.Value state, f: SelectorGetter -> 'U) : SelectorState.ReadOnly<'T,'U> = 
            { Key = state
              Get = f
              Cache = None
              DangerouslyAllowMutability = None }

        [<CustomOperation("set")>]
        member inline _.Set (state: SelectorState.ReadOnly<'T,'U>, f: SelectorMethods -> 'T -> unit) : SelectorState.ReadWrite<'T,'U> =
            { Key = state.Key
              Get = state.Get
              Set = f 
              Cache = state.Cache
              DangerouslyAllowMutability = None }

        [<CustomOperation("set_only")>]
        member inline _.SetOnly (SelectorState.Key.Value state, f: SelectorMethods -> 'T -> unit) : SelectorState.WriteOnly<'T> =
            { Key = state
              Set = f
              DangerouslyAllowMutability = None }

        [<CustomOperation("cache")>]
        member inline _.Cache (state: SelectorState.ICacheImplementer<'T,Loadable<'T>,_>, cacheImplementation: CacheImplementation<'T,Loadable<'T>>) = 
            state.SetCache(cacheImplementation)

        [<CustomOperation("no_cache")>]
        member inline _.NoCache (state: SelectorState.ICacheImplementer<'T,'U,_>) = 
            state.SetCache(Cache.Bypass())

        [<CustomOperation("dangerouslyAllowMutability")>]
        member inline _.DangerouslyAllowMutability (state: SelectorState.MutabilityImplementer<_>) = 
            state.SetMutability()

        member inline _.Run (selector: SelectorState.ReadOnly<'T,JS.Promise<'T>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,JS.Promise<'T>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorState.ReadOnly<'T,Async<'T>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,Async<'T>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorState.ReadOnly<'T,RecoilValue<'T,'Mode>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorState.ReadWrite<'T,RecoilValue<'T,'Mode>>) =
            Recoil.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )

        member inline _.Run (selector: SelectorState.WriteOnly<'T>) =
            Recoil.selectorWriteOnly (
                selector.Key,
                selector.Set
            )

    [<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
    module SelectorBuilderMagic =
        type SelectorBuilder with
            member inline _.Run (selector: SelectorState.ReadOnly<'T,'T>) =
                Recoil.selector (
                    selector.Key, 
                    selector.Get, 
                    ?cacheImplementation = selector.Cache,
                    ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
                )

            member inline _.Run (selector: SelectorState.ReadWrite<'T,'T>) =
                Recoil.selector (
                    selector.Key, 
                    selector.Get, 
                    selector.Set, 
                    ?cacheImplementation = selector.Cache,
                    ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
                )

    let selector = SelectorBuilder()
