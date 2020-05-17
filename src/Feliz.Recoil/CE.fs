namespace Feliz.Recoil

open Fable.Core
open System.ComponentModel

[<AutoOpen>]
module ComputationExpressions =
    type Reader<'r,'t> = Reader of ('r->'t)

    [<RequireQualifiedAccess>]
    module Reader =
        let run (Reader x) = x : 'R->'T
        let map  (f: 'T-> _ ) (Reader m) = Reader (f << m) : Reader<'R,'U>
        let bind (f: 'T-> _ ) (Reader m) = Reader (fun r -> run (f (m r)) r) : Reader<'R,'U>
        let apply (Reader f) (Reader x) = Reader (fun a -> f a ((x: _->'T) a)) : Reader<'R,'U>
        let tryWith (Reader body) handler = Reader (fun s -> try body s with e -> (handler e) s) : Reader<'R,'T>

    type RecoilGetReader<'T> =
        static member lift x : Reader<(RecoilValue<'T,'Mode> -> 'T),'T> =
            Reader (fun getter -> getter(x))

        static member map f x : Reader<(RecoilValue<'T,'Mode> -> 'T),'T> =
            Reader.map (fun x -> f x) x

        static member retn (x: 'T) : Reader<(RecoilValue<'T,'Mode> -> 'T),'T> =
            Reader(fun _ -> x)
        static member retn (x: RecoilValue<'T,'Mode>) : Reader<(RecoilValue<'T,'Mode> -> 'T),'T> =
            Reader(fun getter -> getter(x))

        static member bind (f: 'T -> Reader<(RecoilValue<'T,'Mode> -> 'T),'T>) (x : Reader<(RecoilValue<'T,'Mode> -> 'T),'T>) : Reader<(RecoilValue<'T,'Mode> -> 'T),'T> = 
            Reader.bind (fun x -> f x) x

    type GetBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Bind (x: Reader<(RecoilValue<'T,_> -> 'T),'T>, f) = RecoilGetReader<'T>.bind f x
        member this.Bind (x: RecoilValue<'T,_>, f) =
            this.Bind(RecoilGetReader<'T>.retn x, f)
        
        member _.Return (x: 'T) = RecoilGetReader<'T>.retn x
        member _.Return (x: RecoilValue<'T,_>) = RecoilGetReader<'T>.retn x
        member _.ReturnFrom (x) : Reader<(RecoilValue<'T,_> -> 'T),'T> = x

        member _.Delay (f) = f

        member _.Run (f) = f()
    
        member this.TryWith (body, (handler: (exn -> Reader<(RecoilValue<'T,_> -> 'T),'T>))) =
            try this.ReturnFrom(body())
            with e -> this.ReturnFrom(handler e)
    
        member this.TryFinally (body, compensation) = 
            try this.ReturnFrom(body())
            finally compensation()
    
        member this.Using (disposable: #System.IDisposable, body) =
            let body' = fun () -> body disposable
            this.TryFinally(body', fun () ->
                match disposable with
                    | null -> ()
                    | disp -> disp.Dispose())
            
        member this.Combine (x: RecoilValue<'T,_>, f) = 
            this.Bind(x, f)

    type RecoilSetReader<'T,'Mode,'U> =
        static member lift x : Reader<SelectorMethods * 'T,'U> =
            Reader (fun (getter, _) -> getter.get(x))

        static member map f x : Reader<SelectorMethods * 'T,'T> =
            Reader.map (fun x -> f x) x

        static member retn (x: 'T) : Reader<SelectorMethods * 'T,'T> =
            Reader(fun _ -> x)
        static member retn (x: RecoilValue<'T,'Mode>) : Reader<SelectorMethods * 'T,'T> =
            Reader(fun (getter, _) -> getter.get(x))

        static member bind (f: 'T -> Reader<SelectorMethods * 'T,'U>) (x : Reader<SelectorMethods * 'T,'T>) : Reader<SelectorMethods * 'T,'U> = 
            Reader.bind (fun x -> f x) x

    type SetBuilder [<EditorBrowsable(EditorBrowsableState.Never)>] () =
        member _.Bind<'T,'Mode,'U> (x: Reader<SelectorMethods * 'T,'T>, f) = RecoilSetReader<'T,'Mode,'U>.bind f x
        member this.Bind<'T,'Mode,'U> (x: RecoilValue<'T,'Mode>, f) =
            this.Bind(RecoilSetReader<'T,'Mode,'U>.retn x, f)
        
        member _.Return<'T,'Mode,'U> (x: 'T) = RecoilSetReader<'T,'Mode,'U>.retn x
        member _.Return<'T,'Mode,'U> (x: RecoilValue<'T,'Mode>) = RecoilSetReader<'T,'Mode,'U>.retn x
        member _.ReturnFrom (x) : Reader<SelectorMethods * 'T,'T> = x

        member _.Zero () = Reader(fun _ -> ())

        member _.Delay (f) = f

        member _.Run (f) = f()
    
        member this.TryWith (body, (handler: (exn -> Reader<SelectorMethods * 'T,'T>))) =
            try this.ReturnFrom(body())
            with e -> this.ReturnFrom(handler e)
    
        member this.TryFinally (body, compensation) = 
            try this.ReturnFrom(body())
            finally compensation()
    
        member this.Using (disposable: #System.IDisposable, body) =
            let body' = fun () -> body disposable
            this.TryFinally(body', fun () ->
                match disposable with
                    | null -> ()
                    | disp -> disp.Dispose())
            
        member this.Combine<'T,'Mode> (x: RecoilValue<'T,'Mode>, f) = 
            this.Bind(x, f)

    [<NoEquality;NoComparison>]
    type SelectorState<'T,'Mode,'U,'V> =
        { Key: string option
          Get: ((RecoilValue<'T,'V> -> 'T) -> 'U) option
          Set: (SelectorMethods -> 'T -> unit) option }

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
            
        [<CustomOperation("getter")>]
        member _.Getter (state: SelectorState<_,_,_,_>, (f: Reader<(RecoilValue<'T,_> -> 'T),'U>)) = 
            { state with Get = Some (fun getter -> getter |> (Reader.run f)) }

        [<CustomOperation("set")>]
        member _.Set (state: SelectorState<_,ReadOnly,_,_>, f)  =
            { state with Set = Some f }

        [<CustomOperation("setter")>]
        member _.Setter (state: SelectorState<_,_,_,_>, (f: Reader<SelectorMethods * 'T,unit>)) = 
            { state with Set = Some (fun getter prevValue -> (getter,prevValue) |> (Reader.run f)) }

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

    type RecoilValue<'T,'Mode> with
        static member get (recoilValue: RecoilValue<'T,'Mode>) =
            Reader (fun (get: RecoilValue<'T,'Mode> -> 'T) -> get(recoilValue))

        static member inline reset (recoilValue: RecoilValue<'T,ReadWrite>) =
            Reader (fun (resetter: SelectorMethods) -> resetter.reset(recoilValue))

        static member set (recoilValue: RecoilValue<'T,ReadWrite>, newValue: 'T) = 
            Reader (fun (setter: SelectorMethods) -> setter.set(recoilValue, newValue))

    let selector = SelectorBuilder()    

    [<NoEquality;NoComparison>]
    type AtomState<'T> =
        { Key: string option
          Get: 'T option }

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
            if atom.Key.IsSome then Recoil.atom<'T>(atom.Key.Value, atom.Get.Value)
            else Recoil.atom(atom.Get.Value)

    let atom = AtomBuilder()

    type Recoil with
        static member inline get = GetBuilder()
        static member inline set = SetBuilder()
        //static member inline create<'T> (atom: AtomState<'T>) =
        //    if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Get.Value)
        //    else Recoil.atom(atom.Get.Value)
        //static member inline create<'T> (atom: AtomState<JS.Promise<'T>>) =
        //    if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Get.Value)
        //    else Recoil.atom(atom.Get.Value)
        //static member inline create<'T> (atom: AtomState<Async<'T>>) =
        //    if atom.Key.IsSome then Recoil.atom(atom.Key.Value, atom.Get.Value)
        //    else Recoil.atom(atom.Get.Value)
        //static member inline create<'T> (atom: AtomState<RecoilValue<'T,_>>) =
        //    if atom.Key.IsSome then Recoil.atom<'T>(atom.Key.Value, atom.Get.Value)
        //    else Recoil.atom(atom.Get.Value)
        //static member inline create<'T,'Mode,'U> (selector: SelectorState<'T,ReadOnly,'U>) =
        //    match selector with
        //    | { Key = Some key; Get = Some getF } -> Recoil.selector(key, getF)
        //    | _ -> Recoil.selector(selector.Get.Value)
        //static member inline create<'T,'Mode,'U> (selector: SelectorState<'T,ReadWrite,'U>) =
        //    match selector with
        //    | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
        //    | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)
        //static member inline create<'T,'Mode,'U> (selector: SelectorState<JS.Promise<'T>,'Mode,'U>) =
        //    match selector with
        //    | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
        //    | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)
        //static member inline create<'T,'Mode,'U> (selector: SelectorState<Async<'T>,'Mode,'U>) =
        //    match selector with
        //    | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
        //    | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)
        //static member inline create<'T,'Mode,'U> (selector: SelectorState<RecoilValue<'T,'Mode>,'Mode,'U>) =
        //    match selector with
        //    | { Key = Some key; Get = Some getF; Set = Some setF } -> Recoil.selector(key, getF, setF)
        //    | _ -> Recoil.selector(selector.Get.Value, selector.Set.Value)
