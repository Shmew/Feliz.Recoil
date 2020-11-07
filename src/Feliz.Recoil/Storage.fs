namespace Feliz.Recoil

open Browser.WebStorage
open Fable.SimpleJson

[<RequireQualifiedAccess>]
module Storage =
    let [<Literal>] private RootKey = "__recoil__/"

    let inline private effect<'T> (storage: Browser.Types.Storage) (e: Effector<'T,ReadWrite>) =
        try
            storage.getItem(RootKey + e.node.key)
            |> Json.parseAs<'T>
            |> e.setSelf
        with _ -> ()

        e.onSet (fun newValue -> 
            if Recoil.isDefaultValue newValue then 
                storage.removeItem e.node.key
            else storage.setItem(RootKey + e.node.key, SimpleJson.stringify newValue)
        )

    /// Local storage AtomEffect
    let inline local (e: Effector<'T,ReadWrite>) = effect<'T> localStorage e
    
    /// Session storage AtomEffect
    let inline session (e: Effector<'T,ReadWrite>) = effect<'T> sessionStorage e
