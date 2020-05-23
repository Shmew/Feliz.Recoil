namespace Feliz.Recoil

open System.ComponentModel

module RecoilValue =
    let inline map (mapping: 'T -> 'U) (recoilValue: RecoilValue<'T,'Mode>) =
        selector {
            key (recoilValue.key + "/map")
            get (fun getter -> getter.get(recoilValue) |> mapping)
        }

    let inline bind (binder: 'T -> RecoilValue<'U,_>) (recoilValue: RecoilValue<'T,'Mode>) =
        selector {
            key (recoilValue.key + "/bind")
            get (fun getter -> getter.get(recoilValue) |> binder)
        }

    let inline apply (recoilFun: RecoilValue<'T -> 'U,'Mode1>) (recoilValue: RecoilValue<'T,'Mode2>) =
        recoilFun |> bind (fun f -> recoilValue |> map f)
        
    module Operators =
        /// Infix apply.
        let inline (<*>) f m = apply f m
        
        /// Infix map.
        let inline (<!>) f m = map f m
        
        /// Infix bind.
        let inline (>>=) f m = bind f m
        
        /// Infix bind (right to left).
        let inline (=<<) m f = bind f m
    
        /// Left-to-right Kleisli composition
        let inline (>=>) f g = fun x -> f x >>= g
    
        /// Right-to-left Kleisli composition
        let inline (<=<) x = (fun f a b -> f b a) (>=>) x

    open Operators

    let inline map2 (f: 'A -> 'B -> 'C) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) =
        map f a <*> b
    
    let inline map3 (f: 'A -> 'B -> 'C -> 'D) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) =
        map f a <*> b <*> c
    
    let inline map4 (f: 'A -> 'B -> 'C -> 'D -> 'E) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) =
        map f a <*> b <*> c <*> d
    
    let inline map5 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) =
        map f a <*> b <*> c <*> d <*> e
    
    let inline map6 (f: 'A -> 'B -> 'C -> 'D -> 'E -> 'G -> 'H) (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) (g: RecoilValue<'G,_>) =
        map f a <*> b <*> c <*> d <*> e <*> g

    let inline unzip (a: RecoilValue<'A * 'B,_>) =
        a |> map fst, a |> map snd
    
    let inline unzip3 (a: RecoilValue<'A * 'B * 'C,_>) =
        a |> map (fun (res,_,_) -> res), a |> map (fun (_,res,_) -> res), a |> map (fun (_,_,res) -> res)

    let inline unzip4 (a: RecoilValue<'A * 'B * 'C * 'D,_>) =
        a |> map (fun (res,_,_,_) -> res),
        a |> map (fun (_,res,_,_) -> res), 
        a |> map (fun (_,_,res,_) -> res), 
        a |> map (fun (_,_,_,res) -> res)
    
    let inline unzip5 (a: RecoilValue<'A * 'B * 'C * 'D * 'E,_>) =
        a |> map (fun (res,_,_,_,_) -> res), 
        a |> map (fun (_,res,_,_,_) -> res), 
        a |> map (fun (_,_,res,_,_) -> res), 
        a |> map (fun (_,_,_,res,_) -> res), 
        a |> map (fun (_,_,_,_,res) -> res)
    
    let inline unzip6 (a: RecoilValue<'A * 'B * 'C * 'D * 'E * 'F,_>) =
        a |> map (fun (res,_,_,_,_,_) -> res), 
        a |> map (fun (_,res,_,_,_,_) -> res), 
        a |> map (fun (_,_,res,_,_,_) -> res), 
        a |> map (fun (_,_,_,res,_,_) -> res), 
        a |> map (fun (_,_,_,_,res,_) -> res), 
        a |> map (fun (_,_,_,_,_,res) -> res)

    let inline zip (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) =
        map2(fun x y -> x, y) a b
    
    let inline zip3 (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) =
        map3(fun x y z -> x, y, z) a b c
    
    let inline zip4 (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) =
        map4(fun w x y z -> w, x, y, z) a b c d
    
    let inline zip5 (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>) (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) =
        map5(fun v w x y z -> v, w, x, y, z) a b c d e
    
    let inline zip6 (a: RecoilValue<'A,_>) (b: RecoilValue<'B,_>) (c: RecoilValue<'C,_>)  (d: RecoilValue<'D,_>) (e: RecoilValue<'E,_>) (f: RecoilValue<'F,_>) =
        map6(fun u v w x y z -> u, v, w, x, y, z) a b c d e f

    module Array =
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        let empty<'T> =
            selector {
                key "__empty_array__"
                get (fun _ -> [||] : 'T [])
            }

        let inline traverse (f: 'T -> RecoilValue<'U,_>) (recoilValues: RecoilValue<'T,_> []) =
            empty<'U>
            |> Array.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Array.append [|h|] t) x' xs
            ) recoilValues

        let inline sequence (recoilValues: RecoilValue<'T,_> []) =
            traverse (bind id) recoilValues

    module List =
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        let empty<'T> =
            selector {
                key "__empty_list__"
                get (fun _ -> [] : 'T list)
            }

        let inline traverse (f: 'T -> RecoilValue<'U,_>) (recoilValues: RecoilValue<'T,_> list) =
            empty<'U>
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) recoilValues

        let inline sequence (recoilValues: RecoilValue<'T,_> list) =
            traverse (bind id) recoilValues

    module ResizeArray =
        let inline traverse f (recoilValues: ResizeArray<RecoilValue<'T,_>>) =
            List.empty<'U>
            |> List.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> h::t) x' xs
            ) (List.ofSeq recoilValues)
            |> map ResizeArray
    
        let inline sequence (recoilValues: ResizeArray<RecoilValue<'T,_>>) =
            traverse (bind id) recoilValues

    module Seq =
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        let empty<'T> =
            selector {
                key "__empty_list__"
                get (fun _ -> Seq.empty<'T>)
            }

        let inline traverse f (recoilValues: RecoilValue<'T,_> seq) =
            empty<'U>
            |> Seq.foldBack (fun x xs ->
                let x' = x |> bind f
                map2 (fun h t -> Seq.append (Seq.singleton(h)) t) x' xs
            ) recoilValues
    
        let inline sequence (recoilValues: RecoilValue<'T,_> seq) =
            traverse (bind id) recoilValues

[<AutoOpen>]
module RecoilValueBuilder =
    open System

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let inline dispose (x: #IDisposable) = x.Dispose()

    type RecoilValueBuilder internal () =
        member _.Bind (value: RecoilValue<_,_>, f) = value |> RecoilValue.bind f

        member _.Combine (value: RecoilValue<_,_>, f) = value |> RecoilValue.bind f

        member _.Delay f = f

        member _.Return (value: RecoilValue<'T,'Mode>) = value

        member _.ReturnFrom (value: RecoilValue<_,_>) = 
            atom {
                key (value.key + "/__computation_expression__")
                def value
            }

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
