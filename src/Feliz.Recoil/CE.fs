namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module ComputationExpressions =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<NoEquality;NoComparison>]
    type SelectorState<'T,'Mode,'U,'V> =
        { Key: string option
          Get: ((RecoilValue<'T,'V> -> 'T) -> 'U) option
          Set: (SelectorMethods -> 'T -> unit) option }

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    [<NoEquality;NoComparison>]
    type AtomState<'T> =
        { Key: string option
          Get: 'T option }

    type SelectorBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) : SelectorState<_,ReadOnly,_,_> =
            { Key = None
              Get = None
              Set = None }

        [<CustomOperation("key")>]
        member _.Key (state: SelectorState<_,_,_,_>, value: string) = 
            { state with Key = Some value }

        [<CustomOperation("get")>]
        member _.Get (state: SelectorState<_,_,_,_>, (f: ((RecoilValue<'T,_> -> 'T) -> 'U))) = 
            { state with Get = Some f }
            
        [<CustomOperation("set")>]
        member _.Set (state: SelectorState<_,ReadOnly,_,_>, f)  =
            { state with Set = Some f }

        member inline _.Run (selector: SelectorState<_,ReadOnly,JS.Promise<'U>,_>) =
            match selector with
            | { Key = Some key; Get = Some getF } -> Recoil.selector(key, getF)
            | _ -> Recoil.selector(selector.Get.Value)
        
        member inline _.Run (selector: SelectorState<_,ReadWrite,JS.Promise<'U>,_>) =
            match selector with
            | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
            | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)
        
        member inline _.Run (selector: SelectorState<_,ReadOnly,Async<'U>,_>) =
            match selector with
            | { Key = Some key; Get = Some getF } -> Recoil.selector(key, getF)
            | _ -> Recoil.selector(selector.Get.Value)
        
        member inline _.Run (selector: SelectorState<_,ReadWrite,Async<'U>,_>) =
            match selector with
            | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
            | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)
        
        member inline _.Run (selector: SelectorState<_,ReadOnly,RecoilValue<'U,_>,_>) =
            match selector with
            | { Key = Some key; Get = Some getF } -> Recoil.selector(key, getF)
            | _ -> Recoil.selector(selector.Get.Value)
        
        member inline _.Run (selector: SelectorState<_,ReadWrite,RecoilValue<'U,_>,_>) =
            match selector with
            | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
            | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)

    [<AutoOpen>]
    module SelectorBuilderMagic =
        type SelectorBuilder with
            member inline _.Run (selector: SelectorState<_,ReadOnly,'U,_>) =
                match selector with
                | { Key = Some key; Get = Some getF } -> Recoil.selector(key, getF)
                | _ -> Recoil.selector(selector.Get.Value)

            member inline _.Run (selector: SelectorState<_,ReadWrite,'U,_>) =
                match selector with
                | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
                | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)

    let selector = SelectorBuilder()    

    type AtomBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Yield (_) =
            { Key = None
              Get = None }

        [<CustomOperation("key")>]
        member _.Key (state: AtomState<_>, value: string) = 
            { state with Key = Some value }
            
        [<CustomOperation("def")>]
        member _.Default (state: AtomState<_>, v: 'T) = 
            { state with Get = Some v }

        member inline _.Run<'T> (atom: AtomState<'T>) =
            if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Get.Value)
            else Recoil.atom(atom.Get.Value)

        member inline _.Run<'T> (atom: AtomState<JS.Promise<'T>>) =
            if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Get.Value)
            else Recoil.atom(atom.Get.Value)

        member inline _.Run<'T> (atom: AtomState<Async<'T>>) =
            if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Get.Value)
            else Recoil.atom(atom.Get.Value)

        member inline _.Run<'T> (atom: AtomState<RecoilValue<'T,_>>) =
            if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Get.Value)
            else Recoil.atom(atom.Get.Value)

    let atom = AtomBuilder()
