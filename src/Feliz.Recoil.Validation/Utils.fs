namespace Feliz.Recoil.Validation

module internal Utils =
    module Async =
        let map (f: 'T -> 'U) (value: Async<'T>) =
            async {
                let! v = value
                return f v
            }

        let bind (f: 'T -> Async<'U>) (value: Async<'T>) =
            async { 
                let! res = map f value
                return! res
            }

        let lift (value: 'T) =
            async { return value }

    module RecoilResultOption =
        open Feliz.Recoil
        open Feliz.Recoil.Result

        type RecoilResultOption<'T,'Error,'Mode> = RecoilValue<Result<'T option,'Error>,'Mode>

        let map (f: 'T -> 'U) (input: RecoilResultOption<'T,'Error,'Mode>) =
            RecoilValue.Result.map(Option.map f) input

        let bind (f: 'T -> RecoilResultOption<'U,'Error,ReadOnly>) (input: RecoilResultOption<'T,'Error,'Mode>) =
            let binder opt =
                match opt with
                | Some x -> f x
                | None -> recoil { return (Ok None) }
        
            RecoilResult.bind binder input

        let apply (rroFun: RecoilResultOption<'T -> 'U,'Error,'Mode1>) (input: RecoilResultOption<'T,'Error,'Mode2>) =
            rroFun |> bind (fun f -> input |> map f)

        let map2 (f: 'T -> 'U -> 'V) (a: RecoilResultOption<'T,'Error,'Mode1>) (b: RecoilResultOption<'U,'Error,'Mode2>) =
            apply (map f a) b

    module RecoilResultResult =
        open Feliz.Recoil
        open Feliz.Recoil.Result

        let bind (f: 'T -> RecoilValue<Result<Result<'U,'Error>,'Error2>,_>) (input: RecoilValue<Result<Result<'T,'Error>,'Error2>,_>) =
            let binder res : RecoilValue<Result<Result<'U,'Error>,'Error2>,_>  =
                match res with
                | Ok x -> f x
                | Error e -> recoil { return (Ok (Error e)) }

            RecoilResult.bind binder input

    module RecoilResultSeq =
        open Feliz.Recoil
        open Feliz.Recoil.Result

        let bind (f: 'T -> RecoilValue<Result<seq<'T>,'Error>,_>) (input: RecoilValue<Result<seq<'T>,'Error>,_>) =
            let binder res =
                Seq.map f res
                |> RecoilResult.Seq.sequence
                |> RecoilResult.map Seq.concat

            RecoilResult.bind binder input
