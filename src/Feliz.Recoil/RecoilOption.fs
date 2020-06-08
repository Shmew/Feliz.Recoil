namespace Feliz.Recoil.Option

open Fable.Core
open Feliz.Recoil
open System.ComponentModel

module RecoilOption =
    let lift (value: 'T) =
        RecoilValue.lift (Some value)

    let liftNone<'T> () : RecoilValue<'T option,ReadOnly> =
        RecoilValue.lift (None)

    let map (f: 'T -> 'U) (input: RecoilValue<'T option,'Mode>) =
        recoil {
            let! res = input

            return Option.map f res
        }

    let bind (f: 'T -> RecoilValue<'U option,ReadOnly>) (input: RecoilValue<'T option,'Mode>) =
        recoil {
            let! res = input
            
            let t =
                match res with
                | Some x -> f x
                | None -> recoil { return None }
            
            return! t
        }

    let apply (rrFun: RecoilValue<('T -> 'U) option,ReadOnly>) (input: RecoilValue<'T option,'Mode>) =
        rrFun |> bind (fun f -> input |> map f)

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

    let map2 (f: 'T -> 'U -> 'V) (a: RecoilValue<'T option,_>) (b: RecoilValue<'U option,_>) =
        f <!> a <*> b

    let map3 (f: 'A -> 'B -> 'C -> 'D) (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) (c: RecoilValue<'C option,_>) =
        f <!> a <*> b <*> c
    
    let map4 (f: 'A -> 'B -> 'C -> 'D -> 'E) (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) (c: RecoilValue<'C option,_>) (d: RecoilValue<'D option,_>) =
        f <!> a <*> b <*> c <*> d
    
    let map5 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G) (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) (c: RecoilValue<'C option,_>) (d: RecoilValue<'D option,_>) (e: RecoilValue<'E option,_>) =
        f <!> a <*> b <*> c <*> d <*> e
    
    let map6 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G -> 'H) (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) (c: RecoilValue<'C option,_>) (d: RecoilValue<'D option,_>) (e: RecoilValue<'E option,_>) (g: RecoilValue<'G option,_>) =
        f <!> a <*> b <*> c <*> d <*> e <*> g

    let unzip (a: RecoilValue<('A * 'B) option,_>) =
        fst <!> a, snd <!> a
    
    let unzip3 (a: RecoilValue<('A  * 'B * 'C) option,_>) =
        (fun (res,_,_) -> res) <!> a, (fun (_,res,_) -> res) <!> a, (fun (_,_,res) -> res) <!> a

    let unzip4 (a: RecoilValue<('A * 'B * 'C * 'D) option,_>) =
        (fun (res,_,_,_) -> res) <!> a,
        (fun (_,res,_,_) -> res) <!> a, 
        (fun (_,_,res,_) -> res) <!> a, 
        (fun (_,_,_,res) -> res) <!> a
    
    let unzip5 (a: RecoilValue<('A * 'B * 'C * 'D * 'E) option,_>) =
        (fun (res,_,_,_,_) -> res) <!> a, 
        (fun (_,res,_,_,_) -> res) <!> a, 
        (fun (_,_,res,_,_) -> res) <!> a, 
        (fun (_,_,_,res,_) -> res) <!> a, 
        (fun (_,_,_,_,res) -> res) <!> a
    
    let unzip6 (a: RecoilValue<('A * 'B * 'C * 'D * 'E * 'F) option,_>) =
        (fun (res,_,_,_,_,_) -> res) <!> a, 
        (fun (_,res,_,_,_,_) -> res) <!> a, 
        (fun (_,_,res,_,_,_) -> res) <!> a, 
        (fun (_,_,_,res,_,_) -> res) <!> a, 
        (fun (_,_,_,_,res,_) -> res) <!> a, 
        (fun (_,_,_,_,_,res) -> res) <!> a

    let zip (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) =
        map2(fun x y -> x, y) a b
    
    let zip3 (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) (c: RecoilValue<'C option,_>) =
        map3(fun x y z -> x, y, z) a b c
    
    let zip4 (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) (c: RecoilValue<'C option,_>) (d: RecoilValue<'D option,_>) =
        map4(fun w x y z -> w, x, y, z) a b c d
    
    let zip5 (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) (c: RecoilValue<'C option,_>) (d: RecoilValue<'D option,_>) (e: RecoilValue<'E option,_>) =
        map5(fun v w x y z -> v, w, x, y, z) a b c d e
    
    let zip6 (a: RecoilValue<'A option,_>) (b: RecoilValue<'B option,_>) (c: RecoilValue<'C option,_>)  (d: RecoilValue<'D option,_>) (e: RecoilValue<'E option,_>) (f: RecoilValue<'F option,_>) =
        map6(fun u v w x y z -> u, v, w, x, y, z) a b c d e f

    module Array =
        let traverse (f: 'T -> RecoilValue<'U option,_>) (recoilValues: RecoilValue<'T option,_> []) =
            lift [||]
            |> Array.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Array.append [|h|] t) x' xs
            ) recoilValues

        let sequence (recoilValues: RecoilValue<'T option,_> []) =
            traverse lift recoilValues

    module Async =
        let map (mapping: 'T -> Async<'U>) (recoilValue: RecoilValue<'T option,'Mode>) =
            let mapping a =
                recoil {
                    return
                        async { 
                            let! value = mapping a
                            return Some value
                        }
                } 

            bind mapping recoilValue

        let bind (binder: 'T -> Async<RecoilValue<'U option,'Mode1>>) (recoilValue: RecoilValue<'T option,'Mode2>) =
            let binder a =
                recoil {
                    return
                        async { 
                            let! value = binder a
                            return value
                        }
                }
                |> RecoilValue.flatten

            bind binder recoilValue

        let lift (value: Async<'T>) =
            RecoilValue.Async.lift(value)
            |> RecoilValue.map Ok

    module List =
        let traverse (f: 'T -> RecoilValue<'U option,_>) (recoilValues: RecoilValue<'T option,_> list) =
            lift []
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) recoilValues

        let sequence (recoilValues: RecoilValue<'T option,_> list) =
            traverse lift recoilValues

    module Result =
        let map f (recoilValue: RecoilValue<Result<'T,'Error> option,_>) =
            Result.map f <!> recoilValue

        let bind f (recoilValue: RecoilValue<Result<'T,'Error> option,_>) =
            Result.bind f <!> recoilValue

    module Promise =
        let map (mapping: 'T -> JS.Promise<'U>) (recoilValue: RecoilValue<'T option,'Mode>) =
            let mapping a =
                recoil {
                    return
                        promise { 
                            let! value = mapping a
                            return Some value
                        }
                } 

            bind mapping recoilValue

        let bind (binder: 'T -> JS.Promise<RecoilValue<'U option,'Mode1>>) (recoilValue: RecoilValue<'T option,'Mode2>) =
            let binder a =
                recoil {
                    return
                        promise { 
                            let! value = binder a
                            return value
                        }
                }
                |> RecoilValue.flatten

            bind binder recoilValue

        let lift (value: JS.Promise<'T>) =
            RecoilValue.Promise.lift(value)
            |> RecoilValue.map Ok

    module ResizeArray =
        let traverse f (recoilValues: ResizeArray<RecoilValue<'T option,_>>) =
            lift []
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) (List.ofSeq recoilValues)
            |> map ResizeArray
    
        let sequence (recoilValues: ResizeArray<RecoilValue<'T option,_>>) =
            traverse lift recoilValues

    module Seq =
        let traverse f (recoilValues: RecoilValue<'T option,_> seq) =
            lift Seq.empty
            |> Seq.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Seq.append (Seq.singleton(h)) t) x' xs
            ) recoilValues
    
        let sequence (recoilValues: RecoilValue<'T option,_> seq) =
            traverse lift recoilValues

[<AutoOpen>]
module RecoilOptionBuilder =
    open System

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let inline dispose (x: #IDisposable) = x.Dispose()

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let inline using (a, k) = 
        try k a
        finally dispose a

    type RecoilOptionBuilder internal () =
        member _.Bind (value: RecoilValue<_,_>, f) = value |> RecoilOption.bind f

        member _.Combine (value: RecoilValue<_,_>, f) = value |> RecoilOption.bind f

        member _.Delay f = f

        member _.Return (value: Async<'T>) = RecoilOption.Async.lift value
        member _.Return (value: JS.Promise<'T>) = RecoilOption.Promise.lift value

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
module RecoilOptionBuilderMagic =
    type RecoilOptionBuilder with
        member _.Return (value: 'T) = RecoilOption.lift value

    let recoilOption = RecoilOptionBuilder()
