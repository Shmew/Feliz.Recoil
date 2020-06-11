namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<EditorBrowsable(EditorBrowsableState.Never)>]
module Families =
    let mapFamily (recoilValue: RecoilValue<'T,'Mode>) =
        selectorFamily {
            key (recoilValue.key + "/__map")
            get (fun (mapping: RecoilValue<'T -> 'U,ReadOnly>) getter -> getter.get(recoilValue) |> getter.get(mapping))
        }

    let bindFamily (recoilValue: RecoilValue<'T,'Mode1>) =
        selectorFamily {
            key (recoilValue.key + "/__bind")
            get (fun (binder: RecoilValue<'T -> RecoilValue<'U,'Mode2>,ReadOnly>) getter -> getter.get(recoilValue) |> getter.get(binder))
        }

    module Async =
        let mapFamily (recoilValue: RecoilValue<'T,'Mode>) =
            selectorFamily {
                key (recoilValue.key + "/__map_async")
                get (fun (mapping: RecoilValue<'T -> Async<'U>,ReadOnly>) getter -> 
                    async { return! getter.get(recoilValue) |> getter.get(mapping) })
            }

        let bindFamily (recoilValue: RecoilValue<'T,'Mode1>) =
            selectorFamily {
                key (recoilValue.key + "/__bind_async")
                get (fun (binder: RecoilValue<'T -> Async<RecoilValue<'U,'Mode2>>,ReadOnly>) getter -> 
                    async {
                        return! getter.get(recoilValue) |> getter.get(binder) 
                    })
            }

    module Promise =
        let mapFamily (recoilValue: RecoilValue<'T,'Mode>) =
            selectorFamily {
                key (recoilValue.key + "/__map_promise")
                get (fun (mapping: RecoilValue<'T -> JS.Promise<'U>,ReadOnly>) getter -> 
                    promise { return! getter.get(recoilValue) |> getter.get(mapping) })
            }

        let bindFamily (recoilValue: RecoilValue<'T,'Mode1>) =
            selectorFamily {
                key (recoilValue.key + "/__bind_promise")
                get (fun (binder: RecoilValue<'T -> JS.Promise<RecoilValue<'U,'Mode2>>,ReadOnly>) getter -> 
                    promise {
                        let! res = getter.get(recoilValue) |> getter.get(binder) 
                        return res |> bindFamily |> id
                    })
            }

module RecoilValue =
    open Families

    let inline lift (value: 'T) = Bindings.Recoil.constSelector value

    let map (mapping: 'T -> 'U) (recoilValue: RecoilValue<'T,'Mode>) =
        lift mapping
        |> mapFamily recoilValue

    let bind (binder: 'T -> RecoilValue<'U,'Mode1>) (recoilValue: RecoilValue<'T,'Mode2>) =
        lift binder
        |> bindFamily recoilValue

    let apply (recoilFun: RecoilValue<'T -> 'U,'Mode1>) (recoilValue: RecoilValue<'T,'Mode2>) =
        recoilFun |> bind (fun f -> recoilValue |> map f)

    let applyM (recoilFun: RecoilValue<RecoilValue<'T,_> -> RecoilValue<'U,_>,_>) (recoilValue: RecoilValue<'T,_>) =
        recoilFun |> bind (fun f -> f recoilValue)

    let flatten (value: RecoilValue<RecoilValue<'T,_>,_>) =
        value |> bind id

    module Operators =
        /// Infix apply.
        let (<*>) f m = apply f m
        
        /// Infix map.
        let (<!>) f m = map f m
        
        /// Infix flipped map.
        let (<&>) m f = map f m

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
        f <!> a <*> b
    
    let map3 (f: 'A -> 'B -> 'C -> 'D) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) =
        f <!> a <*> b <*> c
    
    let map4 (f: 'A -> 'B -> 'C -> 'D -> 'E) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) =
        f <!> a <*> b <*> c <*> d
    
    let map5 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) =
        f <!> a <*> b <*> c <*> d <*> e
    
    let map6 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G -> 'H) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) (g: RecoilValue<'G,_>) =
        f <!> a <*> b <*> c <*> d <*> e <*> g

    let unzip (a: RecoilValue<'A * 'B,_>) =
        fst <!> a, snd <!> a
    
    let unzip3 (a: RecoilValue<'A * 'B * 'C,_>) =
        (fun (res,_,_) -> res) <!> a, (fun (_,res,_) -> res) <!> a, (fun (_,_,res) -> res) <!> a

    let unzip4 (a: RecoilValue<'A * 'B * 'C * 'D,_>) =
        (fun (res,_,_,_) -> res) <!> a,
        (fun (_,res,_,_) -> res) <!> a, 
        (fun (_,_,res,_) -> res) <!> a, 
        (fun (_,_,_,res) -> res) <!> a
    
    let unzip5 (a: RecoilValue<'A * 'B * 'C * 'D * 'E,_>) =
        (fun (res,_,_,_,_) -> res) <!> a, 
        (fun (_,res,_,_,_) -> res) <!> a, 
        (fun (_,_,res,_,_) -> res) <!> a, 
        (fun (_,_,_,res,_) -> res) <!> a, 
        (fun (_,_,_,_,res) -> res) <!> a
    
    let unzip6 (a: RecoilValue<'A * 'B * 'C * 'D * 'E * 'F,_>) =
        (fun (res,_,_,_,_,_) -> res) <!> a, 
        (fun (_,res,_,_,_,_) -> res) <!> a, 
        (fun (_,_,res,_,_,_) -> res) <!> a, 
        (fun (_,_,_,res,_,_) -> res) <!> a, 
        (fun (_,_,_,_,res,_) -> res) <!> a, 
        (fun (_,_,_,_,_,res) -> res) <!> a

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
            traverse lift recoilValues

    module Async =
        let map (mapping: 'T -> Async<'U>) (recoilValue: RecoilValue<'T,'Mode>) =
            lift (fun a -> async { return! mapping a })
            |> Async.mapFamily recoilValue

        let bind (binder: 'T -> Async<RecoilValue<'U,'Mode1>>) (recoilValue: RecoilValue<'T,'Mode2>) =
            lift (fun a -> async { return! binder a })
            |> Async.bindFamily recoilValue

        let lift (value: Async<'T>) =
            lift (fun a -> async { return! a })
            |> Async.mapFamily (Bindings.Recoil.constSelector value)

    module List =
        let traverse (f: 'T -> RecoilValue<'U,_>) (recoilValues: RecoilValue<'T,_> list) =
            lift []
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) recoilValues

        let sequence (recoilValues: RecoilValue<'T,_> list) =
            traverse lift recoilValues

    module Option =
        let map f (recoilValue: RecoilValue<'T option,_>) =
            Option.map f <!> recoilValue

        let bind f (recoilValue: RecoilValue<'T option,_>) =
            Option.bind f <!> recoilValue

    module Promise =
        let map (mapping: 'T -> JS.Promise<'U>) (recoilValue: RecoilValue<'T,'Mode>) =
            lift (fun a -> promise { return! mapping a })
            |> Promise.mapFamily recoilValue

        let bind (binder: 'T -> JS.Promise<RecoilValue<'U,'Mode1>>) (recoilValue: RecoilValue<'T,'Mode2>) =
            lift (fun a -> promise { return! binder a })
            |> Promise.bindFamily recoilValue

        let lift (value: JS.Promise<'T>) =
            lift (fun a -> promise { return! a })
            |> Promise.mapFamily (lift value) 

    let inline readOnly (value: RecoilValue<'T,ReadWrite>) = unbox<RecoilValue<'T,ReadOnly>> value

    module ResizeArray =
        let traverse f (recoilValues: ResizeArray<RecoilValue<'T,_>>) =
            lift []
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) (List.ofSeq recoilValues)
            |> map ResizeArray
    
        let sequence (recoilValues: ResizeArray<RecoilValue<'T,_>>) =
            traverse lift recoilValues

    module Result =
        let map f (recoilValue: RecoilValue<Result<'T,'Error>,_>) =
            Result.map f <!> recoilValue

        let bind f (recoilValue: RecoilValue<Result<'T,'Error>,_>) =
            Result.bind f <!> recoilValue

    module Seq =
        let traverse f (recoilValues: RecoilValue<'T,_> seq) =
            lift Seq.empty
            |> Seq.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Seq.append (Seq.singleton(h)) t) x' xs
            ) recoilValues
    
        let sequence (recoilValues: RecoilValue<'T,_> seq) =
            traverse lift recoilValues


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

        member _.Return (value: Async<'T>) = RecoilValue.Async.lift value
        member _.Return (value: JS.Promise<'T>) = RecoilValue.Promise.lift value

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

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never)>]
module RecoilValueBuilderMagic =
    type RecoilValueBuilder with
        member _.Return (value: 'T) = Bindings.Recoil.constSelector value

    let recoil = RecoilValueBuilder()
