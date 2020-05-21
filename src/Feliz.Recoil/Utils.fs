namespace Feliz.Recoil

open Fable.Core

module Map =
    let toJS (map: Map<'Key,'Value>) =
        (JS.Constructors.Map.Create(), map)
        ||> Map.fold(fun jsMap k v -> jsMap.set(k,v))
