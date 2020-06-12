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

### Recoil.atomFamily

Creates a RecoilValue with the default value based on the parameter given.

Signature:
```fs
(key: string, defaultValue: 'P -> 'T, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) 
    -> ('P -> RecoilValue<'T,ReadWrite>)

(key: string, defaultValue: JS.Promise<'T>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) 
    -> ('P -> RecoilValue<'T,ReadWrite>)

(key: string, defaultValue: 'P -> JS.Promise<'T>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) 
    -> ('P -> RecoilValue<'T,ReadWrite>)

(key: string, defaultValue: Async<'T>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) 
    -> ('P -> RecoilValue<'T,ReadWrite>)

(key: string, defaultValue: 'P -> Async<'T>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) 
    -> ('P -> RecoilValue<'T,ReadWrite>)

(key: string, defaultValue: RecoilValue<'T,_>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) 
    -> ('P -> RecoilValue<'T,ReadWrite>)

(key: string, defaultValue: 'P -> RecoilValue<'T,_>, ?persistence: PersistenceSettings<'T,'U>, ?dangerouslyAllowMutability: bool) 
    -> ('P -> RecoilValue<'T,ReadWrite>)

```

See the [atomFamily example] for usage.

### Recoil.noWait

Converts a `RecoilValue<'T,_>` into a `RecoilValue<Loadable<'T>,ReadOnly>`.

Prevents a selector from being blocked while trying to resolve a RecoilValue.

Signature:
```fs
(recoilValue: RecoilValue<'T,'Mode>) -> RecoilValue<Loadable<'T>,ReadOnly>
```

### Recoil.selector

Derives state and returns a RecoilValue via the provided get function.

**Keys mut be unique across the application!**

When a setter is not provided the selector is *ReadOnly*. If you try to 
use hooks like `Recoil.useState` you will get compiler errors.

Signature:
```fs
(key: string, get: SelectorGetter -> 'U, ?cacheImplementation: CacheImplementation<'U,Loadable<'U>>, ?dangerouslyAllowMutability: bool) 
    -> RecoilValue<'U,ReadOnly>

(key: string, get: SelectorGetter -> JS.Promise<'U>, ?cacheImplementation: CacheImplementation<'U,Loadable<'U>>, ?dangerouslyAllowMutability: bool) 
    -> RecoilValue<'U,ReadOnly>

(key: string, get: SelectorGetter -> Async<'U>, ?cacheImplementation: CacheImplementation<'U,Loadable<'U>>, ?dangerouslyAllowMutability: bool) 
    -> RecoilValue<'U,ReadOnly>

(key: string, get: SelectorGetter -> RecoilValue<'U,_>, ?cacheImplementation: CacheImplementation<'U,Loadable<'U>>, ?dangerouslyAllowMutability: bool) 
    -> RecoilValue<'U,ReadOnly>

(key: string, get: SelectorGetter -> 'U, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,Loadable<'U>>, ?dangerouslyAllowMutability: bool) 
    -> RecoilValue<'U,ReadWrite>

(key: string, get: SelectorGetter -> JS.Promise<'U>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,Loadable<'U>>, ?dangerouslyAllowMutability: bool) 
    -> RecoilValue<'U,ReadWrite>

(key: string, get: SelectorGetter -> Async<'U>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,Loadable<'U>>, ?dangerouslyAllowMutability: bool) 
    -> RecoilValue<'U,ReadWrite>

(key: string, get: SelectorGetter -> RecoilValue<'U,_>, set: SelectorMethods -> 'T -> unit, ?cacheImplementation: CacheImplementation<'U,Loadable<'U>>, ?dangerouslyAllowMutability: bool) 
    -> RecoilValue<'U,ReadWrite>

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

### Recoil.selectorFamily

Derives state and returns a RecoilValue via the provided get function.

Signature:
```fs
(key: string, 
 get: 'P -> SelectorGetter -> 'U, // 'U can be: Async, Promise, or RecoilValue as well. 
 ?cacheImplementation: unit -> CacheImplementation<'T,Loadable<'T>>, 
 ?paramCacheImplementation: unit -> CacheImplementation<RecoilValue<'T,ReadOnly>, 'P>, 
 ?dangerouslyAllowMutability: bool)
    -> 'P -> RecoilValue<'T,'Mode>

(key: string, 
 get: 'P -> SelectorGetter -> 'U, // 'U can be: Async, Promise, or RecoilValue as well. 
 set: 'P -> SelectorMethods -> 'T -> unit, 
 ?cacheImplementation: unit -> CacheImplementation<'T,Loadable<'T>>,
 ?paramCacheImplementation: unit -> CacheImplementation<RecoilValue<'T,ReadWrite>, 'P>,
 ?dangerouslyAllowMutability: bool)
    -> 'P -> RecoilValue<'T,'Mode>
```

See the [selectorFamily example] for usage.

### Recoil.waitForAll

Converts a collection of RecoilValues into a RecoilValue of a collection.

Requests all dependencies in parallel and waits for all to be
available before returning a value.

Signature:
```fs
(recoilValues: RecoilValue<'T,'Mode> []) -> RecoilValue<'T [], 'Mode>
(recoilValues: RecoilValue<'T,'Mode> list) -> RecoilValue<'T list, 'Mode>
(recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) RecoilValue<ResizeArray<'T>, 'Mode>
```

See the [concurrency example] for usage.

### Recoil.waitForAny

Converts a collection of RecoilValues into a RecoilValue of a collection of loadables.

Requests all dependencies in parallel and waits for at least
one to be available before returning results.

Signature:
```fs
(recoilValues: RecoilValue<'T,'Mode> []) -> RecoilValue<Loadable<'T> [], 'Mode>
(recoilValues: RecoilValue<'T,'Mode> list) -> RecoilValue<Loadable<'T> list, 'Mode>
(recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) RecoilValue<ResizeArray<Loadable<'T>>, 'Mode>
```

See the [concurrency example] for usage.

### Recoil.waitForNone

Converts a collection of RecoilValues into a RecoilValue of a collection.

Requests all dependencies in parallel and immediately returns
current results without waiting.

The difference between this and `waitForAny` is that the *value* of at least one loadable will
have resolved before the latter returns. This function will return the collection with (potentially)
no resolved loadables.

Signature:
```fs
(recoilValues: RecoilValue<'T,'Mode> []) -> RecoilValue<Loadable<'T> [], 'Mode>
(recoilValues: RecoilValue<'T,'Mode> list) -> RecoilValue<Loadable<'T> list, 'Mode>
(recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) RecoilValue<ResizeArray<Loadable<'T>>, 'Mode>
```

See the [concurrency example] for usage.

### RecoilValue Module

Contains standard helpers for `RecoilValue` such as:

* apply
* bind
* lift
* map
* unzip 
* zip

As well as modules for common collection types: Array, List, Seq, ResizeArray:
* traverse
* sequence

#### Operators

You can open `Feliz.Recoil.RecoilValue.Operators` if you want 
to use the usual suspects:

* Infix apply - `<*>`
* Infix map - `<!>`
* Infix flipped map - `<&>`
* Infix bind - `>>=`
* Infix bind (right to left) - `=<<`
* Left-to-right Kleisli composition - `>=>`
* Right-to-left Kleisli composition - `<=<`

[atomFamily example]:https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/AtomFamily
[concurrency example]:https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/Concurrency
[selectorFamily example]:https://shmew.github.io/Feliz.Recoil/#/Recoil/Examples/SelectorFamily
