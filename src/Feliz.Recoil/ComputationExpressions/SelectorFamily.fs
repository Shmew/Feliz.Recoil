namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module SelectorFamilyCE =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<RequireQualifiedAccess>]
    module SelectorFamilyState =
        type Empty = interface end

        type Key = | Value of string

        type MutabilityImplementer<'V> =
            abstract member SetMutability : unit -> 'V

        type ICacheImplementer<'T,'U,'P,'Mode,'V> =
            abstract member SetCache : (unit -> CacheImplementation<'T,Loadable<'T>>) -> 'V
            abstract member SetParamCache<'Mode> : (unit -> CacheImplementation<RecoilValue<'T,'Mode>,'P>) -> 'V

        [<NoEquality;NoComparison>]
        type ReadOnly<'T,'U,'P> = 
            { Key: string
              Get: 'P -> SelectorGetter -> 'U
              Cache: (unit -> CacheImplementation<'T,Loadable<'T>>) option
              ParamCache: (unit -> CacheImplementation<RecoilValue<'T,ReadOnly>,'P>) option
              DangerouslyAllowMutability: bool option }

            interface ICacheImplementer<'T,'U,'P,ReadOnly,ReadOnly<'T,'U,'P>> with
                member this.SetCache cache =
                    { Key = this.Key
                      Get = this.Get
                      Cache = Some cache
                      ParamCache = this.ParamCache
                      DangerouslyAllowMutability = this.DangerouslyAllowMutability }

                member this.SetParamCache cache =
                    { Key = this.Key
                      Get = this.Get
                      Cache = this.Cache
                      ParamCache = Some cache
                      DangerouslyAllowMutability = this.DangerouslyAllowMutability }

            interface MutabilityImplementer<ReadOnly<'T,'U,'P>> with
                member this.SetMutability () =
                    { Key = this.Key
                      Get = this.Get
                      Cache = this.Cache
                      ParamCache = this.ParamCache
                      DangerouslyAllowMutability = Some true }

        [<NoEquality;NoComparison>]
        type ReadWrite<'T,'U,'P> =
            { Key: string
              Get: 'P -> SelectorGetter -> 'U
              Set: 'P -> SelectorMethods -> 'T -> unit
              Cache: (unit -> CacheImplementation<'T,Loadable<'T>>) option
              ParamCache: (unit -> CacheImplementation<RecoilValue<'T,ReadWrite>,'P>) option
              DangerouslyAllowMutability: bool option }

            interface ICacheImplementer<'T,'U,'P,ReadWrite,ReadWrite<'T,'U,'P>> with
                member this.SetCache (cache: unit -> CacheImplementation<'T,Loadable<'T>>) =
                    { this with Cache = Some cache }

                member this.SetParamCache cache =
                    { this with ParamCache = Some cache }

            interface MutabilityImplementer<ReadWrite<'T,'U,'P>> with
                member this.SetMutability () =
                    { this with DangerouslyAllowMutability = Some true }

        [<NoEquality;NoComparison>]
        type WriteOnly<'T,'P> =
            { Key: string
              Set: 'P -> SelectorMethods -> 'T -> unit
              DangerouslyAllowMutability: bool option }

            interface MutabilityImplementer<WriteOnly<'T,'P>> with
                member this.SetMutability () =
                    { this with DangerouslyAllowMutability = Some true }

    type SelectorFamilyBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            unbox<SelectorFamilyState.Empty>()

        [<CustomOperation("key")>]
        member _.Key (state: SelectorFamilyState.Empty, value: string) = 
            SelectorFamilyState.Key.Value value

        [<CustomOperation("get")>]
        member inline _.Get (SelectorFamilyState.Key.Value state, (f: 'P -> SelectorGetter -> 'U)) : SelectorFamilyState.ReadOnly<'T,'U,'P> = 
            { Key = state
              Get = f
              Cache = None
              ParamCache = None
              DangerouslyAllowMutability = None }

        [<CustomOperation("set")>]
        member inline _.Set (state: SelectorFamilyState.ReadOnly<'T,'U,'P>, (f: 'P -> SelectorMethods -> 'T -> unit)) : SelectorFamilyState.ReadWrite<'T,'U,'P> =
            { Key = state.Key
              Get = state.Get
              Set = f 
              Cache = state.Cache
              ParamCache = state.ParamCache |> Option.map (unbox<unit -> CacheImplementation<RecoilValue<'T,_>,'P>>)
              DangerouslyAllowMutability = None }

        [<CustomOperation("set_only")>]
        member inline _.SetOnly (SelectorFamilyState.Key.Value state, f: 'P -> SelectorMethods -> 'T -> unit) : SelectorFamilyState.WriteOnly<'T,'P> =
            { Key = state
              Set = f
              DangerouslyAllowMutability = None }

        [<CustomOperation("cache")>]
        member inline _.Cache (state: SelectorFamilyState.ICacheImplementer<'T,_,_,_,_>, (cacheImplementation: unit -> CacheImplementation<'T,Loadable<'T>>)) = 
            state.SetCache(cacheImplementation)

        [<CustomOperation("param_cache")>]
        member inline _.ParamCache (state: SelectorFamilyState.ICacheImplementer<'T,_,'P,_,_>, (cacheImplementation: unit -> CacheImplementation<RecoilValue<'T,_>,'P>)) = 
            state.SetParamCache(cacheImplementation)

        //[<CustomOperation("no_cache")>]
        //member inline _.NoCache (state: SelectorFamilyState.IOptionImplementer<_,_,_,_,_>) = 
        //    state.SetCache(fun () -> NoCache() :> CacheImplementation<'T,Loadable<'T>>)

        //[<CustomOperation("no_param_cache")>]
        //member inline _.NoParamCache (state: SelectorFamilyState.IOptionImplementer<_,_,_,_,_>) = 
        //    state.SetParamCache(fun () -> NoCache() :> CacheImplementation<_,_>)

        [<CustomOperation("dangerouslyAllowMutability")>]
        member inline _.DangerouslyAllowMutability (state: SelectorFamilyState.MutabilityImplementer<_>) = 
            state.SetMutability()

        member inline _.Run (selector: SelectorFamilyState.ReadOnly<'T,JS.Promise<'T>,'P>) =
            Recoil.Family.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache,
                ?paramCacheImplementation = selector.ParamCache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorFamilyState.ReadWrite<'T,JS.Promise<'T>,'P>) =
            Recoil.Family.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache,
                ?paramCacheImplementation = selector.ParamCache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorFamilyState.ReadOnly<'T,Async<'T>,'P>) =
            Recoil.Family.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache,
                ?paramCacheImplementation = selector.ParamCache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorFamilyState.ReadWrite<'T,Async<'T>,'P>) =
            Recoil.Family.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache,
                ?paramCacheImplementation = selector.ParamCache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorFamilyState.ReadOnly<'T,RecoilValue<'T,'Mode>,'P>) =
            Recoil.Family.selector (
                selector.Key, 
                selector.Get, 
                ?cacheImplementation = selector.Cache,
                ?paramCacheImplementation = selector.ParamCache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )
        
        member inline _.Run (selector: SelectorFamilyState.ReadWrite<'T,RecoilValue<'T,'Mode>,'P>) =
            Recoil.Family.selector (
                selector.Key, 
                selector.Get, 
                selector.Set, 
                ?cacheImplementation = selector.Cache,
                ?paramCacheImplementation = selector.ParamCache,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )

        member inline _.Run (selector: SelectorFamilyState.WriteOnly<'T,'P>) =
            Recoil.Family.selectorWriteOnly (
                selector.Key,
                selector.Set,
                ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
            )

    [<AutoOpen;EditorBrowsable(EditorBrowsableState.Never);Erase>]
    module SelectorFamilyBuilderMagic =
        type SelectorFamilyBuilder with
            member inline _.Run (selector: SelectorFamilyState.ReadOnly<'T,'T,'P>) =
                Recoil.Family.selector (
                    selector.Key, 
                    selector.Get, 
                    ?cacheImplementation = selector.Cache,
                    ?paramCacheImplementation = selector.ParamCache,
                    ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
                )

            member inline _.Run (selector: SelectorFamilyState.ReadWrite<'T,'T,'P>) =
                Recoil.Family.selector (
                    selector.Key, 
                    selector.Get, 
                    selector.Set, 
                    ?cacheImplementation = selector.Cache,
                    ?paramCacheImplementation = selector.ParamCache,
                    ?dangerouslyAllowMutability = selector.DangerouslyAllowMutability
                )

    let selectorFamily = SelectorFamilyBuilder()
