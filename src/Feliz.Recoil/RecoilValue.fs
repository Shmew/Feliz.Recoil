namespace Feliz.Recoil

open System.ComponentModel

[<EditorBrowsable(EditorBrowsableState.Never)>]
module xxHash =
    open Fable.Core
    open Fable.Core.JsInterop

    [<EmitConstructor>]
    type XXH =
        [<Emit("$0($1, $2).toString(16)")>]
        abstract invoke: string * int -> string
    
    let xxh : XXH = import "XXH" "./Hash/xxhash"

    let inline getHash (str: string) = xxh.invoke(str, 0)

    [<Emit("$0.toString()")>]
    let funcToString x : string = jsNative

    let inline hashFunc f = f |> funcToString |> getHash

module RecoilValue =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    type MapSelectorCache<'T,'U> (map: Map<string,RecoilValue<'U,ReadOnly>>) =
        interface CacheImplementation<RecoilValue<'U,ReadOnly>,'T -> 'U> with
            member _.get key = map.TryFind(xxHash.hashFunc key) 
            member _.set (key: 'T -> 'U) (value: RecoilValue<'U,ReadOnly>) = 
                MapSelectorCache<'T,'U>(map.Add(xxHash.hashFunc key, value)) 
                    :> CacheImplementation<RecoilValue<'U,ReadOnly>,'T -> 'U>

        static member toCacheImpl (cache: MapSelectorCache<'T,'U>) = cache :> CacheImplementation<RecoilValue<'U,ReadOnly>,'T -> 'U>

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    type BindSelectorCache<'T,'U,'Mode> (map: Map<string,RecoilValue<'U,ReadOnly>>) =
        interface CacheImplementation<RecoilValue<'U,ReadOnly>,'T -> RecoilValue<'U,'Mode>> with
            member _.get key = map.TryFind(xxHash.hashFunc key) 
            member _.set (key: 'T -> RecoilValue<'U,'Mode>) (value: RecoilValue<'U,ReadOnly>) = 
                BindSelectorCache<'T,'U,'Mode>(map.Add(xxHash.hashFunc key, value)) 
                    :> CacheImplementation<RecoilValue<'U,ReadOnly>,'T -> RecoilValue<'U,'Mode>>

        static member toCacheImpl (cache: BindSelectorCache<'T,'U,'Mode>) = cache :> CacheImplementation<RecoilValue<'U,ReadOnly>,'T -> RecoilValue<'U,'Mode>>

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let mapFamily (recoilValue: RecoilValue<'T,'Mode>) =
        selectorFamily {
            key (recoilValue.key + "/__map")
            get (fun (mapping: 'T -> 'U) getter -> getter.get(recoilValue) |> mapping)
            param_cache (fun () -> 
                MapSelectorCache<'T,'U>(Map.empty) 
                    :> CacheImplementation<RecoilValue<'U,ReadOnly>,'T -> 'U>)
        }

    let map (mapping: 'T -> 'U) (recoilValue: RecoilValue<'T,'Mode>) =
        mapFamily recoilValue mapping

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let bindFamily (recoilValue: RecoilValue<'T,'Mode1>) =
        selectorFamily {
            key (recoilValue.key + "/__bind")
            get (fun (binder: 'T -> RecoilValue<'U,'Mode2>) getter -> getter.get(recoilValue) |> binder)
            param_cache (fun () -> 
                BindSelectorCache<'T,'U,'Mode2>(Map.empty) 
                    :> CacheImplementation<RecoilValue<'U,ReadOnly>,'T -> RecoilValue<'U,_>>)
        }

    let bind (binder: 'T -> RecoilValue<'U,'Mode1>) (recoilValue: RecoilValue<'T,'Mode2>) =
        bindFamily recoilValue binder

    let apply (recoilFun: RecoilValue<'T -> 'U,'Mode1>) (recoilValue: RecoilValue<'T,'Mode2>) =
        recoilFun |> bind (fun f -> recoilValue |> map f)
        
    let inline lift (value: 'T) =
        Bindings.Recoil.constSelector(value)

    let flatten (value: RecoilValue<RecoilValue<'T,_>,_>) =
        value |> bind id

    module Operators =
        /// Infix apply.
        let (<*>) f m = apply f m
        
        /// Infix map.
        let (<!>) f m = map f m
        
        /// Infix bind.
        let (>>=) f m = bind f m
        
        /// Infix bind (right to left).
        let (=<<) m f = bind f m
    
        /// Left-to-right Kleisli composition
        let (>=>) f g = fun x -> f x >>= g
    
        /// Right-to-left Kleisli composition
        let (<=<) x = (fun f a b -> f b a) (>=>) x

    open Operators

    let map2 (f: 'A -> 'B -> 'C) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) =
        map f a <*> b
    
    let map3 (f: 'A -> 'B -> 'C -> 'D) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) =
        map f a <*> b <*> c
    
    let map4 (f: 'A -> 'B -> 'C -> 'D -> 'E) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) =
        map f a <*> b <*> c <*> d
    
    let map5 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) =
        map f a <*> b <*> c <*> d <*> e
    
    let map6 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G -> 'H) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) (g: RecoilValue<'G,_>) =
        map f a <*> b <*> c <*> d <*> e <*> g

    let unzip (a: RecoilValue<'A * 'B,_>) =
        a |> map fst, a |> map snd
    
    let unzip3 (a: RecoilValue<'A * 'B * 'C,_>) =
        a |> map (fun (res,_,_) -> res), a |> map (fun (_,res,_) -> res), a |> map (fun (_,_,res) -> res)

    let unzip4 (a: RecoilValue<'A * 'B * 'C * 'D,_>) =
        a |> map (fun (res,_,_,_) -> res),
        a |> map (fun (_,res,_,_) -> res), 
        a |> map (fun (_,_,res,_) -> res), 
        a |> map (fun (_,_,_,res) -> res)
    
    let unzip5 (a: RecoilValue<'A * 'B * 'C * 'D * 'E,_>) =
        a |> map (fun (res,_,_,_,_) -> res), 
        a |> map (fun (_,res,_,_,_) -> res), 
        a |> map (fun (_,_,res,_,_) -> res), 
        a |> map (fun (_,_,_,res,_) -> res), 
        a |> map (fun (_,_,_,_,res) -> res)
    
    let unzip6 (a: RecoilValue<'A * 'B * 'C * 'D * 'E * 'F,_>) =
        a |> map (fun (res,_,_,_,_,_) -> res), 
        a |> map (fun (_,res,_,_,_,_) -> res), 
        a |> map (fun (_,_,res,_,_,_) -> res), 
        a |> map (fun (_,_,_,res,_,_) -> res), 
        a |> map (fun (_,_,_,_,res,_) -> res), 
        a |> map (fun (_,_,_,_,_,res) -> res)

    let zip (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) =
        map2(fun x y -> x, y) a b
    
    let zip3 (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) =
        map3(fun x y z -> x, y, z) a b c
    
    let zip4 (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) =
        map4(fun w x y z -> w, x, y, z) a b c d
    
    let zip5 (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) =
        map5(fun v w x y z -> v, w, x, y, z) a b c d e
    
    let zip6 (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>)  (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) (f: RecoilValue<'F,_>) =
        map6(fun u v w x y z -> u, v, w, x, y, z) a b c d e f

    module Array =
        let traverse (f: 'T -> RecoilValue<'U,_>) (recoilValues: RecoilValue<'T,_> []) =
            lift [||]
            |> Array.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Array.append [|h|] t) x' xs
            ) recoilValues

        let sequence (recoilValues: RecoilValue<'T,_> []) =
            traverse (Bindings.Recoil.constSelector) recoilValues

    module List =
        let traverse (f: 'T -> RecoilValue<'U,_>) (recoilValues: RecoilValue<'T,_> list) =
            lift []
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) recoilValues

        let sequence (recoilValues: RecoilValue<'T,_> list) =
            traverse (Bindings.Recoil.constSelector) recoilValues

    module ResizeArray =
        let traverse f (recoilValues: ResizeArray<RecoilValue<'T,_>>) =
            lift []
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) (List.ofSeq recoilValues)
            |> map ResizeArray
    
        let sequence (recoilValues: ResizeArray<RecoilValue<'T,_>>) : RecoilValue<ResizeArray<'T>,ReadOnly> =
            traverse (Bindings.Recoil.constSelector) recoilValues

    module Seq =
        let traverse f (recoilValues: RecoilValue<'T,_> seq) =
            lift Seq.empty
            |> Seq.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Seq.append (Seq.singleton(h)) t) x' xs
            ) recoilValues
    
        let sequence (recoilValues: RecoilValue<'T,_> seq) =
            traverse (Bindings.Recoil.constSelector) recoilValues

[<AutoOpen>]
module RecoilValueBuilder =
    open System

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let inline dispose (x: #IDisposable) = x.Dispose()

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let inline using (a, k) = 
        try k a
        finally dispose a

    type RecoilValueBuilder internal () =
        member _.Bind (value: RecoilValue<_,_>, f) = value |> RecoilValue.bind f

        member _.Combine (value: RecoilValue<_,_>, f) = value |> RecoilValue.bind f

        member _.Delay f = f

        member _.Return value = Bindings.Recoil.constSelector value

        member _.ReturnFrom (value: RecoilValue<_,_>) = value

        member _.Run f = f()

        member this.TryFinally ((m: RecoilValue<_,_>), handler) =
            try this.ReturnFrom(m)
            finally handler()

        member this.TryWith ((m: RecoilValue<_,_>), handler) =
            try this.ReturnFrom(m)
            with e -> handler e

        member this.Using (value, k) = 
            this.TryFinally(k value, (fun () -> dispose value))

    let recoil = RecoilValueBuilder()
