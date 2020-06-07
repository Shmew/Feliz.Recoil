namespace Feliz.Recoil.Result

open Fable.Core
open Feliz.Recoil
open System.ComponentModel

module RecoilResult =
    let lift (value: 'T) =
        RecoilValue.lift (Ok value)

    let liftError (value: 'Error) =
        RecoilValue.lift (Error value)

    let map (f: 'T -> 'U) (input: RecoilValue<Result<'T,'Error>,'Mode>) =
        recoil {
            let! res = input

            return Result.map f res
        }

    let mapError (f: 'Error -> 'U) (input: RecoilValue<Result<'T,'Error>,'Mode>) =
        recoil {
            let! res = input
            return Result.mapError f res
        }

    let bind (f: 'T -> RecoilValue<Result<'U,'Error>,ReadOnly>) (input: RecoilValue<Result<'T,'Error>,'Mode>) =
        recoil {
            let! res = input
            
            let t =
                match res with
                | Ok x -> f x
                | Error e -> recoil { return Error e }
            
            return! t
        }

    let apply (rrFun: RecoilValue<Result<'T -> 'U,'Error>,ReadOnly>) (input: RecoilValue<Result<'T,'Error>,'Mode>) =
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

    let map2 (f: 'T -> 'U -> 'V) (a: RecoilValue<Result<'T,'Error>,_>) (b: RecoilValue<Result<'U,'Error>,_>) =
        map f a <*> b

    let map3 (f: 'A -> 'B -> 'C -> 'D) (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) (c: RecoilValue<Result<'C,'Error>,_>) =
        map f a <*> b <*> c
    
    let map4 (f: 'A -> 'B -> 'C -> 'D -> 'E) (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) (c: RecoilValue<Result<'C,'Error>,_>) (d: RecoilValue<Result<'D,'Error>,_>) =
        map f a <*> b <*> c <*> d
    
    let map5 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G) (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) (c: RecoilValue<Result<'C,'Error>,_>) (d: RecoilValue<Result<'D,'Error>,_>) (e: RecoilValue<Result<'E,'Error>,_>) =
        map f a <*> b <*> c <*> d <*> e
    
    let map6 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G -> 'H) (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) (c: RecoilValue<Result<'C,'Error>,_>) (d: RecoilValue<Result<'D,'Error>,_>) (e: RecoilValue<Result<'E,'Error>,_>) (g: RecoilValue<Result<'G,'Error>,_>) =
        map f a <*> b <*> c <*> d <*> e <*> g

    let unzip (a: RecoilValue<Result<'A * 'B,'Error>,_>) =
        a |> map fst, a |> map snd
    
    let unzip3 (a: RecoilValue<Result<'A  * 'B * 'C,'Error>,_>) =
        a |> map (fun (res,_,_) -> res), a |> map (fun (_,res,_) -> res), a |> map (fun (_,_,res) -> res)

    let unzip4 (a: RecoilValue<Result<'A * 'B * 'C * 'D,'Error>,_>) =
        a |> map (fun (res,_,_,_) -> res),
        a |> map (fun (_,res,_,_) -> res), 
        a |> map (fun (_,_,res,_) -> res), 
        a |> map (fun (_,_,_,res) -> res)
    
    let unzip5 (a: RecoilValue<Result<'A * 'B * 'C * 'D * 'E,'Error>,_>) =
        a |> map (fun (res,_,_,_,_) -> res), 
        a |> map (fun (_,res,_,_,_) -> res), 
        a |> map (fun (_,_,res,_,_) -> res), 
        a |> map (fun (_,_,_,res,_) -> res), 
        a |> map (fun (_,_,_,_,res) -> res)
    
    let unzip6 (a: RecoilValue<Result<'A * 'B * 'C * 'D * 'E * 'F,'Error>,_>) =
        a |> map (fun (res,_,_,_,_,_) -> res), 
        a |> map (fun (_,res,_,_,_,_) -> res), 
        a |> map (fun (_,_,res,_,_,_) -> res), 
        a |> map (fun (_,_,_,res,_,_) -> res), 
        a |> map (fun (_,_,_,_,res,_) -> res), 
        a |> map (fun (_,_,_,_,_,res) -> res)

    let zip (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) =
        map2(fun x y -> x, y) a b
    
    let zip3 (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) (c: RecoilValue<Result<'C,'Error>,_>) =
        map3(fun x y z -> x, y, z) a b c
    
    let zip4 (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) (c: RecoilValue<Result<'C,'Error>,_>) (d: RecoilValue<Result<'D,'Error>,_>) =
        map4(fun w x y z -> w, x, y, z) a b c d
    
    let zip5 (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) (c: RecoilValue<Result<'C,'Error>,_>) (d: RecoilValue<Result<'D,'Error>,_>) (e: RecoilValue<Result<'E,'Error>,_>) =
        map5(fun v w x y z -> v, w, x, y, z) a b c d e
    
    let zip6 (a: RecoilValue<Result<'A,'Error>,_>) (b: RecoilValue<Result<'B,'Error>,_>) (c: RecoilValue<Result<'C,'Error>,_>)  (d: RecoilValue<Result<'D,'Error>,_>) (e: RecoilValue<Result<'E,'Error>,_>) (f: RecoilValue<Result<'F,'Error>,_>) =
        map6(fun u v w x y z -> u, v, w, x, y, z) a b c d e f

    module Array =
        let traverse (f: 'T -> RecoilValue<Result<'U,'Error>,_>) (recoilValues: RecoilValue<Result<'T,'Error>,_> []) =
            lift [||]
            |> Array.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Array.append [|h|] t) x' xs
            ) recoilValues

        let sequence (recoilValues: RecoilValue<Result<'T,'Error>,_> []) =
            traverse lift recoilValues

    module Async =
        let map (mapping: 'T -> Async<'U>) (recoilValue: RecoilValue<Result<'T,'Error>,'Mode>) =
            let mapping a =
                recoil {
                    return
                        async { 
                            let! value = mapping a
                            return Ok value
                        }
                } 

            bind mapping recoilValue

        let bind (binder: 'T -> Async<RecoilValue<Result<'U,'Error>,'Mode1>>) (recoilValue: RecoilValue<Result<'T,'Error>,'Mode2>) =
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
        let traverse (f: 'T -> RecoilValue<Result<'U,'Error>,_>) (recoilValues: RecoilValue<Result<'T,'Error>,_> list) =
            lift []
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) recoilValues

        let sequence (recoilValues: RecoilValue<Result<'T,'Error>,_> list) =
            traverse lift recoilValues

    module Option =
        let map f (recoilValue: RecoilValue<Result<'T option,'Error>,_>) =
            Option.map f <!> recoilValue

        let bind f (recoilValue: RecoilValue<Result<'T option,'Error>,_>) =
            Option.bind f <!> recoilValue

    module Promise =
        let map (mapping: 'T -> JS.Promise<'U>) (recoilValue: RecoilValue<Result<'T,'Error>,'Mode>) =
            let mapping a =
                recoil {
                    return
                        promise { 
                            let! value = mapping a
                            return Ok value
                        }
                } 

            bind mapping recoilValue

        let bind (binder: 'T -> JS.Promise<RecoilValue<Result<'U,'Error>,'Mode1>>) (recoilValue: RecoilValue<Result<'T,'Error>,'Mode2>) =
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
        let traverse f (recoilValues: ResizeArray<RecoilValue<Result<'T,'Error>,_>>) =
            lift []
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) (List.ofSeq recoilValues)
            |> map ResizeArray
    
        let sequence (recoilValues: ResizeArray<RecoilValue<Result<'T,'Error>,_>>) =
            traverse lift recoilValues

    module Seq =
        let traverse f (recoilValues: RecoilValue<Result<'T,'Error>,_> seq) =
            lift Seq.empty
            |> Seq.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Seq.append (Seq.singleton(h)) t) x' xs
            ) recoilValues
    
        let sequence (recoilValues: RecoilValue<Result<'T,'Error>,_> seq) =
            traverse lift recoilValues

[<AutoOpen>]
module RecoilResultBuilder =
    open System

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let inline dispose (x: #IDisposable) = x.Dispose()

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let inline using (a, k) = 
        try k a
        finally dispose a

    type RecoilResultBuilder internal () =
        member _.Bind (value: RecoilValue<_,_>, f) = value |> RecoilResult.bind f

        member _.Combine (value: RecoilValue<_,_>, f) = value |> RecoilResult.bind f

        member _.Delay f = f

        member _.Return (value: Async<'T>) = RecoilResult.Async.lift value
        member _.Return (value: JS.Promise<'T>) = RecoilResult.Promise.lift value

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
module RecoilResultBuilderMagic =
    type RecoilResultBuilder with
        member _.Return (value: 'T) = RecoilResult.lift value

    let recoilResult = RecoilResultBuilder()
