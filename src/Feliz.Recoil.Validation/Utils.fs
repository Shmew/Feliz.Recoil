namespace Feliz.Recoil.Validation

module internal Utils =
    [<RequireQualifiedAccess>]
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

    [<RequireQualifiedAccess>]
    module Result =
        let apply resF res =
            resF |> Result.bind(fun f -> res |> Result.map f)

        let map2 (f: 'A -> 'B -> 'C) res1 res2 =
            apply (Result.map f res1) res2

        let traverse f resList =
            Ok []
            |> List.foldBack (fun head tail ->
                let newHead = head |> Result.bind f
                map2 (fun h t -> h::t) newHead tail
            ) resList

        let sequence resList =
            traverse Ok resList
