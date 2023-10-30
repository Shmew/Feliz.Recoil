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

[<Erase;RequireQualifiedAccess>]
module JSe =
    /// Tests to see if the prototype property of a constructor appears 
    /// anywhere in the prototype chain of an object.
    ///
    /// This should only be used when working with external code (like bindings).
    [<Emit("$1 instanceof $0")>]
    let instanceOf (ctor: obj) (value: obj) : bool = jsNative

    [<Erase>]
    type is =
        [<Emit("typeof $0 === 'function'")>]
        static member function' (value: obj) : bool = jsNative

        /// Checks if the input is both an object and has a Symbol.iterator.
        [<Emit("typeof $0 === 'object' && !$0[Symbol.iterator]")>]
        static member nonEnumerableObject (value: obj) : bool = jsNative

[<AutoOpen>]
module NonErasedExtensions =
    open Fable.Core.JsInterop

    type JSe.is with
        /// Normal structural F# comparison, but ignores top-level functions (e.g. Elmish dispatch).
        static member equalsButFunctions (x: 'a) (y: 'a) =
            if obj.ReferenceEquals(x, y) then
                true
            elif JSe.is.nonEnumerableObject x && not(isNull(box y)) then
                let keys = JS.Constructors.Object.keys x
                let length = keys.Count
                let mutable i = 0
                let mutable result = true
                while i < length && result do
                    let key = keys.[i]
                    i <- i + 1
                    let xValue = x?(key)
                    result <- JSe.is.function' xValue || xValue = y?(key)
                result
            else
                (box x) = (box y)