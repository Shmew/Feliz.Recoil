namespace Feliz.Recoil

open Fable.Core

module Async =
    let map (mapper: 'T -> 'U) (a: Async<'T>) =
        async {
            let! res = a
            return mapper res
        }

    let lift (value: 'T) = async { return value }

module Map =
    let toJS (map: Map<'Key,'Value>) =
        (JS.Constructors.Map.Create(), map)
        ||> Map.fold(fun jsMap k v -> jsMap.set(k,v))

module Promise =
    let ignore (prom: JS.Promise<_>) = Promise.map ignore prom
