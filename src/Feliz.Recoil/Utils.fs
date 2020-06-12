namespace Feliz.Recoil

open Fable.Core

module Map =
    let toJS (map: Map<'Key,'Value>) =
        (JS.Constructors.Map.Create(), map)
        ||> Map.fold(fun jsMap k v -> jsMap.set(k,v))

module EqualityHelpers =
    open Fable.Core.JsInterop
    
    [<Emit("!!$0 && typeof $0.then === 'function'")>]
    let isPromise (x: obj) : bool = jsNative
    
    [<Emit("typeof $0 === 'function'")>]
    let isFunction (x: obj) : bool = jsNative

    [<Emit("typeof $0 === 'object' && !$0[Symbol.iterator]")>]
    let isNonEnumerableObject (x: obj) : bool = jsNative 

    [<Emit("typeof $0 === 'undefined'")>]
    let isUndefined (x: obj) : bool = jsNative

    [<Emit("typeof $0 === null")>]
    let isNull (x: obj) : bool = jsNative

    /// Normal structural F# comparison, but ignores top-level functions (e.g. Elmish dispatch).
    let equalsButFunctions (x: 'a) (y: 'a) =
        if obj.ReferenceEquals(x, y) then
            true
        elif isNonEnumerableObject x && not(isNull(box y)) then
            let keys = JS.Constructors.Object.keys x
            let length = keys.Count
            let mutable i = 0
            let mutable result = true
            while i < length && result do
                let key = keys.[i]
                i <- i + 1
                let xValue = x?(key)
                result <- isFunction xValue || xValue = y?(key)
            result
        else
            (box x) = (box y)

    /// Comparison similar to default React.memo, but ignores functions (e.g. Elmish dispatch).
    /// Performs a memberwise comparison where value types and strings are compared by value,
    /// and other types by reference.
    let memoEqualsButFunctions (x: 'a) (y: 'a) =
        if obj.ReferenceEquals(x, y) then
            true
        elif isNonEnumerableObject x && not(isNull(box y)) then
            let keys = JS.Constructors.Object.keys x
            let length = keys.Count
            let mutable i = 0
            let mutable result = true
            while i < length && result do
                let key = keys.[i]
                i <- i + 1
                let xValue = x?(key)
                result <- isFunction xValue || obj.ReferenceEquals(xValue, y?(key))
            result
        else
            false
