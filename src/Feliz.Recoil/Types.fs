namespace Feliz.Recoil

open Fable.Core
open Fable.Core.JsInterop
open System.ComponentModel

[<EditorBrowsable(EditorBrowsableState.Never)>]
[<StringEnum(CaseRules.None);RequireQualifiedAccess>]
type RecoilValueTag =
    | Writeable
    | Readonly

[<Erase>]
type RecoilValue<'T> =
    [<Emit("$0.key")>]
    member _.key : string = jsNative
    [<Emit("$0.tag"); EditorBrowsable(EditorBrowsableState.Never)>]
    member _.tag : RecoilValueTag = jsNative

type DefaultValue = interface end

[<Erase>]
type SelectorMethods<'T> =
    [<Emit("$0.get($1)")>]
    member _.get (recoilValue: RecoilValue<'T>) : 'Return = jsNative
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T>, newValue: 'T) : unit = jsNative
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T>, newValue: DefaultValue) : unit = jsNative
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T>, newValue: 'T -> 'T) : unit = jsNative
    [<Emit("$0.set($1, $2)")>]
    member _.set (recoilValue: RecoilValue<'T>, newValue: 'T -> DefaultValue) : unit = jsNative
    [<Emit("$0.set($1)")>]
    member _.reset (recoilValue: RecoilValue<'T>) : unit = jsNative

[<StringEnum;RequireQualifiedAccess>]
type LoadableState =
    | HasValue
    | HasError
    | Loading

[<EditorBrowsable(EditorBrowsableState.Never)>]
type ResolvedLoadablePromiseInfo<'T> =
    abstract value: 'T

[<Erase>]
type Loadable<'T> =
    member inline this.asyncMaybe () : Async<'T> option =
        this.promiseMaybe() |> Option.map Async.AwaitPromise

    member inline this.asyncOrThrow () : Async<'T> =
        this.promiseOrThrow() |> Async.AwaitPromise

    [<Emit("$0.errorMaybe()")>]
    member _.errorMaybe () : exn option = jsNative

    [<Emit("$0.errorOrThrow()")>]
    member _.errorOrThrow () : exn = jsNative

    [<Emit("$0.getValue()")>]
    member _.getValue () : 'T = jsNative

    [<Emit("$0.map($1)")>]
    member _.map (mapping: 'T -> JS.Promise<'U>) : Loadable<'U> = jsNative
    member inline this.map (mapping: 'T -> Async<'U>) =
        let test = fun o -> o |> mapping |> Async.StartAsPromise
        this.map(test)
    
    [<Emit("$0.promiseMaybe()")>]
    member _.promiseMaybe () : JS.Promise<'T> option = jsNative

    [<Emit("$0.promiseOrThrow()")>]
    member _.promiseOrThrow () : JS.Promise<'T> = jsNative

    [<Emit("$0.state")>]
    member _.state : LoadableState = jsNative

    [<Emit("$0.contents");EditorBrowsable(EditorBrowsableState.Never)>]
    member _.contents : U3<'T,exn,JS.Promise<ResolvedLoadablePromiseInfo<'T>>> = jsNative

    member inline this.toAsync () : Async<'T> =
        this.toPromise'() 
        |> Promise.map(fun o -> o?value)
        |> Async.AwaitPromise

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<Emit("$0.toPromise()")>]
    member _.toPromise' () : JS.Promise<obj> = jsNative

    member inline this.toPromise () : JS.Promise<'T> =
        this.toPromise'()
        |> Promise.map(fun o -> o?value)

    [<Emit("$0.valueMaybe()")>]
    member _.valueMaybe () : 'T option = jsNative

    [<Emit("$0.valueOrThrow()")>]
    member _.valueOrThrow () : 'T = jsNative

[<AutoOpen;Erase>]
module LoadableMagic =
    type Loadable<'T> with
        [<Emit("$0.map($1)")>]
        member _.map (mapping: 'T -> 'U) : Loadable<'U> = jsNative

