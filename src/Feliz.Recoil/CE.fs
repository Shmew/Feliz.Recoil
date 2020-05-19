namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module ComputationExpressions =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<NoEquality;NoComparison>]
    type SelectorState<'T,'Mode, 'V> =
        { Key: string option
          Get: (SelectorGetter -> 'V) option
          Set: (SelectorMethods -> 'T -> unit) option
          Mode: 'Mode }

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    type SelectorHelper =
        static member Create () =
            { Key = None
              Get = None 
              Set = None
              Mode = unbox<ReadOnly>() }

        static member setKey (state, key: string) =
            { Key = Some key 
              Get = state.Get
              Set = state.Set
              Mode = state.Mode }

        static member setGet (state, get: SelectorGetter -> 'T) =
            { Key = state.Key
              Get = Some get
              Set = state.Set
              Mode = state.Mode }

        static member setSet (state, set: SelectorMethods -> 'T -> unit) =
            { Key = state.Key
              Get = state.Get
              Set = Some set
              Mode = unbox<ReadWrite>() }

    type SelectorBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            SelectorHelper.Create()

        [<CustomOperation("key")>]
        member _.Key (state, value: string) = 
            SelectorHelper.setKey(state, value)

        [<CustomOperation("get")>]
        member inline _.Get (state, (f: SelectorGetter -> 'U)) = 
            SelectorHelper.setGet(state, f)

        [<CustomOperation("set")>]
        member inline _.Set (state, (f: SelectorMethods -> 'T -> unit)) =
            SelectorHelper.setSet(state, f)
            
        member inline _.Run (selector: SelectorState<'T,ReadOnly,JS.Promise<'T>>) =
            match selector with
            | { Key = Some key; Get = Some getF } -> Recoil.selector(key, getF)
            | _ -> Recoil.selector(selector.Get.Value)
        
        member inline _.Run (selector: SelectorState<'T,ReadWrite,JS.Promise<'T>>) =
            match selector with
            | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
            | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)
        
        member inline _.Run (selector: SelectorState<'T,ReadOnly,Async<'T>>) =
            match selector with
            | { Key = Some key; Get = Some getF } -> Recoil.selector(key, getF)
            | _ -> Recoil.selector(selector.Get.Value)
        
        member inline _.Run (selector: SelectorState<'T,ReadWrite,Async<'T>>) =
            match selector with
            | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
            | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)
        
        member inline _.Run (selector: SelectorState<'T,ReadOnly,RecoilValue<'T,_>>) =
            match selector with
            | { Key = Some key; Get = Some getF } -> Recoil.selector(key, getF)
            | _ -> Recoil.selector(selector.Get.Value)
        
        member inline _.Run (selector: SelectorState<'T,ReadWrite,RecoilValue<'T,_>>) =
            match selector with
            | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
            | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)

    [<AutoOpen>]
    module SelectorBuilderMagic =
        type SelectorBuilder with
            member inline _.Run (selector: SelectorState<'T,ReadOnly,'T>) =
                match selector with
                | { Key = Some key; Get = Some getF } -> Recoil.selector(key, (unbox<SelectorGetter -> 'T> getF))
                | _ -> Recoil.selector((unbox<SelectorGetter -> 'T> selector.Get.Value))

            member inline _.Run (selector: SelectorState<'T,ReadWrite,'T>) =
                match selector with
                | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
                | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)

    let selector = SelectorBuilder()    

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<NoEquality;NoComparison>]
    type AtomState<'T> =
        { Key: string option
          Def: 'T option }

    type AtomBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            { Key = None
              Def = None }

        [<CustomOperation("key")>]
        member _.Key (state: AtomState<_>, value: string) = 
            { state with Key = Some value }
            
        [<CustomOperation("def")>]
        member _.Default (state: AtomState<_>, v: 'T) = 
            { state with Def = Some v }

        member inline _.Run (atom: AtomState<JS.Promise<'T>>) =
            if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Def.Value)
            else Recoil.atom(atom.Def.Value)

        member inline _.Run (atom: AtomState<Async<'T>>) =
            if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Def.Value)
            else Recoil.atom(atom.Def.Value)

        member inline _.Run (atom: AtomState<RecoilValue<'T,_>>) =
            if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Def.Value)
            else Recoil.atom(atom.Def.Value)

    [<AutoOpen>]
    module AtomBuilderMagic =
        type AtomBuilder with
            member inline _.Run<'T> (atom: AtomState<'T>) : RecoilValue<'T,ReadWrite> =
                if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Def.Value)
                else Recoil.atom(atom.Def.Value)

    let atom = AtomBuilder()
