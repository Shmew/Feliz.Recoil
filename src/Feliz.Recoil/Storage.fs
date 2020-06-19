namespace Feliz.Recoil

open Browser.WebStorage
open Fable.SimpleJson
open Feliz
open System.ComponentModel

[<RequireQualifiedAccess>]
module Storage =
    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let [<Literal>] RootKey = "__recoil__/"

    type Hydrator [<EditorBrowsable(EditorBrowsableState.Never)>] (setter: MutableSnapshot, storage: Browser.Types.Storage) =
        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member _.setter = setter

        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member _.storage = storage

        member inline this.setAtom<'T> (atom: RecoilValue<'T,ReadWrite>) =
            async {
                try 
                    this.storage.getItem(RootKey + atom.key)
                    |> Json.parseAs<'T>
                    |> fun res -> this.setter.set(atom, res)
                with _ -> ()
            } |> Async.StartImmediate

    [<EditorBrowsable(EditorBrowsableState.Never)>]
    let observer = React.functionComponent(fun () ->
        Recoil.useTransactionObservation <| fun o ->
            async {
                o.modifiedAtoms
                |> Set.iter (fun (name, _) ->
                    o.atomInfo.TryFind(name)
                    |> Option.iter (fun atomInfo ->
                        match atomInfo.persistence.type' with
                        | Some PersistenceType.LocalStorage -> Some localStorage
                        | Some PersistenceType.SessionStorage -> Some sessionStorage
                        | _ -> None
                        |> Option.iter (fun storage ->
                            o.atomValues.TryFind(name)
                            |> Option.iter (fun value ->
                                storage.setItem(RootKey + name, SimpleJson.stringify(value))
                            )
                        )
                    )
                )
            }
            |> Async.StartImmediate

        Html.none)
