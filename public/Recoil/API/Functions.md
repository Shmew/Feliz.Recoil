# Feliz.Recoil - API Reference

## Functions

### Recoil.atom

Creates a RecoilValue with the given default value.

**Keys mut be unique across the application!**

Signature:
```fs
(key: string, defaultValue: 'T, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: JS.Promise<'T>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: Async<'T>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) -> RecoilValue<'T,ReadWrite>
(key: string, defaultValue: RecoilValue<'T,_>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) -> RecoilValue<'T,ReadWrite>
```

Usage:
```fs
let myText = Recoil.atom("myAtomKey", "some text")
```

### Recoil.selector

Derives state and returns a RecoilValue via the provided get function.

**Keys mut be unique across the application!**

When a setter is not provided the selector is *ReadOnly*. If you try to 
use hooks like `Recoil.useState` you will get compiler errors.

Signature:
```fs
(key: string, get: SelectorGetter -> 'U, ?cacheImplementation: CacheImplementation<'U,'U>) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> JS.Promise<'U>, ?cacheImplementation: CacheImplementation<'U,JS.Promise<'U>>) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> Async<'U>, ?cacheImplementation: CacheImplementation<'U,Async<'U>>) -> RecoilValue<'U,ReadOnly>
(key: string, get: SelectorGetter -> RecoilValue<'U,_>, ?cacheImplementation: CacheImplementation<'U,RecoilValue<'U,_>>) -> RecoilValue<'U,ReadOnly>

(key: string, get: SelectorGetter -> 'U, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,'U>) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,JS.Promise<'U>>) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> Async<'U>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,Async<'U>>) -> RecoilValue<'U,ReadWrite>
(key: string, get: SelectorGetter -> RecoilValue<'U,_>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,RecoilValue<'U,_>>) -> RecoilValue<'U,ReadWrite>
```

Usage:
```fs
let textState = Recoil.atom("textState", "Hello world!")

let vowels = [ 'a'; 'e'; 'i'; 'o'; 'u' ]

let textStateTransform =
    Recoil.selector("textStateTransform", fun getter ->
        getter.get(textState)
        |> String.filter(fun v -> List.contains v vowels)
    )
```
