[<RequireQualifiedAccess>]
module Samples.AtomFamily

open Css
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

let atomIds = 
    atom {
        key "AtomFamily/atomIds" 
        def ([] : int list)
    }

let latestAtom =
    selector {
        key "AtomFamily/latestAtom"
        get (fun getter ->
            getter.get(atomIds)
            |> List.tryHead)
    }

let textFamilySelector =
    let textFamily =
        atomFamily {
            key "AtomFamily/textFamily"
            def (fun i -> sprintf "Hello world #%i" i)
        }

    (* 
        Can also be written as:
    
        Recoil.Family.atom("textFamily", fun i -> sprintf "Hello world #%i" i)
    *)

    selector {
        key "AtomFamily/newAtomSelector"
        get (fun getter -> getter.get(atomIds) |> List.rev)
        set (fun setter newAtoms ->
            setter.get(atomIds)
            |> List.append newAtoms 
            |> fun res -> setter.set(atomIds, res)

            newAtoms
            |> List.iter(fun i -> setter.set(textFamily(i), Recoil.defaultValue))
        )
    }

let maxWidth = style.maxWidth (length.em 30)

let atomList = React.functionComponent(fun () ->
    let atomIds = Recoil.useValue(textFamilySelector)
    
    Html.orderedList (
        atomIds
        |> List.map (fun i ->
            Html.li [
                prop.key i
                prop.text (sprintf "Atom #%i" i)
            ]
        )
    ))

let addAtom = React.functionComponent(fun () ->
    let latestAtom = Recoil.useValue(latestAtom)
    let addNewAtom = Recoil.useSetState(textFamilySelector)

    Html.button [
        prop.classes [ 
            Bulma.Button
            Bulma.HasBackgroundPrimary
            Bulma.HasTextWhite 
        ]
        prop.text "Create New"
        prop.onClick <| fun _ ->
            match latestAtom with
            | Some i -> i + 1
            | None -> 0
            |> List.singleton
            |> addNewAtom
    ])

[<ReactComponent>]
let Render() =
    Recoil.root [
        addAtom()
        atomList()
    ]