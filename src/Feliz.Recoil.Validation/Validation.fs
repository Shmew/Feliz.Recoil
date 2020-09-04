namespace Feliz.Recoil.Validation

open Fable.Core
open Feliz.Recoil
open System
open System.ComponentModel
open System.Text.RegularExpressions

[<RequireQualifiedAccess>]
type ValidationError<'Custom> =
    | Blank
    | Contains
    | Custom of 'Custom
    | Eq
    | Email
    | Float
    | Gt
    | Gte
    | HasNumbers
    | HasSpecialCharacters
    | Int
    | Length of int
    | Lt
    | Lte
    | Match
    | MaxLength
    | MinLength
    | None
    | NoNumbers
    | NoSpecialCharacters
    | Parse
    | Some
    | Url

    member this.DefaultText =
        match this with
        | Blank -> "Cannot be blank."
        | Contains -> "Missing required item."
        | Custom _ -> "Specify custom error message."
        | Eq -> "Did not equal requirement."
        | Email -> "Must be an email address."
        | Float -> "Must be a decimal."
        | Gt -> "Too small."
        | Gte -> "Too small."
        | HasNumbers -> "Must contain a number."
        | HasSpecialCharacters -> "Must contain a special character."
        | Int -> "Must be an integer."
        | Length i -> sprintf "Must be a %i long." i
        | Lt -> "Too large."
        | Lte -> "Too large."
        | Match -> "Must match."
        | MaxLength -> "Too long."
        | MinLength -> "Too short."
        | None -> "Cannot be empty."
        | NoNumbers -> "Cannot contain numbers."
        | NoSpecialCharacters -> "Cannot contain special characters."
        | Parse -> "Unable to parse input."
        | Some -> "Must be empty."
        | Url -> "Must be a URL."

[<RequireQualifiedAccess>]
module ValidationError =
    let defaultText (validationError: ValidationError<_>) = validationError.DefaultText

[<RequireQualifiedAccess>]
type ValidationState<'T,'Custom> =
    | Init
    | Invalid of ValidationError<'Custom>
    | Valid of 'T
    
    member this.isInit = match this with | Init -> true | _ -> false
    member this.isInvalid = match this with | Invalid _ -> true | _ -> false
    member this.isValid = match this with | Valid _ -> true | _ -> false

[<RequireQualifiedAccess>]
module ValidationState =
    let isInit (validationState: ValidationState<_,_>) = validationState.isInit
    let isInvalid (validationState: ValidationState<_,_>) = validationState.isInvalid
    let isValid (validationState: ValidationState<_,_>) = validationState.isValid

[<EditorBrowsable(EditorBrowsableState.Never)>]
module Validation =
    open Utils

    module internal Regex =
        let email = Regex @"^(([^<>()\[\]\\.,;:\s@""]+(\.[^<>()\[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$"
        let url = Regex @"^((([A-Za-z]{3,9}:(?:\/\/)?)(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-_]*)?\??(?:[\-\+=&;%@\.\w_]*)#?(?:[\.\!\/\\\w]*))?)$"

    let internal (>>=) m f = Result.bind f m
    let internal (>=>) f1 f2 = f1 >> (Result.bind f2) 

    let validationFamily<'T,'Custom when 'Custom : equality and 'T : equality> (validators: RecoilValue<('T -> Result<'T,ValidationError<'Custom>>) list,ReadOnly>) =
        selectorFamily {
            key "__recoil__forms__/__validationFamily"
            set_only (fun (resultAtom: RecoilValue<ValidationState<'T,'Custom>,ReadWrite>) setter (newValue: 'T option) ->
                let validators = setter.get(validators)

                newValue
                |> Option.iter (fun newValue ->

                    let result =
                        if validators.Length > 0 then
                            validators 
                            |> List.fold (fun state f -> state >>= f) (Ok newValue)
                        else Ok newValue

                    match result, setter.get(resultAtom) with
                    | Ok res, ValidationState.Valid currentRes when res = currentRes -> ()
                    | Ok res, _ -> setter.set(resultAtom, ValidationState.Valid(res))
                    | Error e, ValidationState.Invalid currentE when e = currentE -> ()
                    | Error e, _ -> setter.set(resultAtom, ValidationState.Invalid e)
                ))
        }

    type GenericValidators =
        static member compare (input: 'T) (errorCase: ValidationError<_>) (comparer: 'T -> bool) =
            if comparer input then Ok input
            else Error errorCase

        static member tryCompare (input: 'T) (errorCase: ValidationError<_>) (comparer: 'T -> bool) =
            try 
                if comparer input then Ok input
                else Error errorCase
            with _ -> Error ValidationError<'Custom>.Parse
                
        static member contains (value: 'T) (input: #seq<'T>) =
            fun inner -> Seq.contains value inner
            |> GenericValidators.compare input (ValidationError<'Custom>.Contains)

        static member custom (validator: 'T -> Result<'T,ValidationError<'Custom>>) (input: 'T) =
            validator input

        static member eq (value: 'T) (input: 'T) =
            fun inner -> inner = value
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member forAllList (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) (input: 'T list) =
            let validate t =
                validators
                |> List.reduce (>=>)
                |> fun res -> res t
            
            match input with
            | [] -> []
            | [ h ] -> [ validate h ]
            | _ ->
                input
                |> List.map (fun t ->
                    validators
                    |> List.reduce (>=>)
                    |> fun res -> res t)
            |> Result.sequence

        static member forAllArray (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) (input: 'T []) =
            GenericValidators.forAllList validators (input |> List.ofArray)
            |> Result.map Array.ofList

        static member forAllResizeArray (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) (input: ResizeArray<'T>) =
            GenericValidators.forAllList validators (input |> List.ofSeq)
            |> Result.map ResizeArray

        static member forAllSeq (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) (input: seq<'T>) =
            GenericValidators.forAllList validators (input |> List.ofSeq)
            |> Result.map Seq.ofList

        static member gt (value: 'T) (input: 'T) =
            fun inner -> inner > value
            |> GenericValidators.compare input (ValidationError<'Custom>.Gt)

        static member gte (value: 'T) (input: 'T) =
            fun inner -> inner >= value
            |> GenericValidators.compare input (ValidationError<'Custom>.Gte)

        static member lengthOf (value: int) (input: #seq<'T>) =
            fun inner -> (Seq.length inner) = value
            |> GenericValidators.compare input (ValidationError<'Custom>.Length(value))

        static member lt (value: 'T) (input: 'T) =
            fun inner -> inner < value
            |> GenericValidators.compare input (ValidationError<'Custom>.Lt)

        static member lte (value: 'T) (input: 'T) =
            fun inner -> inner <= value
            |> GenericValidators.compare input (ValidationError<'Custom>.Lte)

        static member maxLengthOf (value: int) (input: #seq<'T>) =
            fun inner -> (Seq.length inner) <= value
            |> GenericValidators.compare input (ValidationError<'Custom>.MaxLength)

        static member minLengthOf (value: int) (input: #seq<'T>) =
            fun inner -> (Seq.length inner) >= value
            |> GenericValidators.compare input (ValidationError<'Custom>.MinLength)

    type OptionValidators =
        static member isSome (input: 'T option) =
            function | Some _ -> true | None -> false
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member isNone (input: 'T option) =
            function | Some _ -> false | None -> true
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member map (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) (input: 'T option) =
            match input with
            | Some t -> 
                validators
                |> List.reduce (>=>)
                |> fun res -> res t
                |> Result.map Some
            | None -> Ok None

    type ResultValidators =
        static member isOk (input: Result<'T,_>) =
            function | Ok _ -> true | Error _ -> false
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member isError (input: Result<'T,_>) =
            function | Ok _ -> false | Error _ -> true
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member map (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) (input: Result<'T,'Error>) =
            match input with
            | Ok t -> 
                validators
                |> List.reduce (>=>)
                |> fun res -> res t
                |> Result.map Ok
            | Error (err: 'Error) -> Ok (Error err) 

    type StringValidators =
        static member eqFloat (value: float) (input: string) =
            fun s -> (float s) = value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Eq)
        static member eqInt (value: int) (input: string) =
            fun s -> (float s) = (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Eq)
        static member eqStr (value: string) (input: string) =
            fun s -> s = value
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member gtFloat (value: float) (input: string) =
            fun s -> (float s) > value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Gt)
        static member gtInt (value: int) (input: string) =
            fun s -> (float s) > (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Gt)

        static member gteFloat (value: float) (input: string) =
            fun s -> (float s) >= value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Gte)
        static member gteInt (value: int) (input: string) =
            fun s -> (float s) >= (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Gte)

        static member hasSpecialCharacters (input: string) =
            fun (s: string) -> s.ToCharArray() |> Array.tryFind (fun c -> Char.IsLetterOrDigit c) |> Option.isSome
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.HasSpecialCharacters)

        static member hasNumbers (input: string) =
            fun (s: string) -> s.ToCharArray() |> Array.tryFind (fun c -> Char.IsNumber c) |> Option.isSome
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.HasNumbers)

        static member lengthOf (value: int) (input: string) =
            fun (s: string) -> s.Length = value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Length(value))

        static member ltFloat (value: float) (input: string) =
            fun s -> (float s) < value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Lt)
        static member ltInt (value: int) (input: string) =
            fun s -> (float s) < (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Lt)

        static member lteFloat (value: float) (input: string) =
            fun s -> (float s) <= value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Lte)
        static member lteInt (value: int) (input: string) =
            fun s -> (float s) <= (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Lte)
        
        static member matches (value: Regex) (input: string) =
            GenericValidators.tryCompare input (ValidationError<'Custom>.Match) value.IsMatch

        static member maxLengthOf (value: int) (input: string) =
            fun (s: string) -> s.Length <= value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.MaxLength)

        static member minLengthOf (value: int) (input: string) =
            fun (s: string) -> s.Length >= value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.MinLength)

        static member notBlank (input: string) =
            if String.IsNullOrEmpty input || String.IsNullOrWhiteSpace input then
                Error ValidationError<'Custom>.Blank
            else Ok input

        static member noNumbers (input: string) =
            fun (s: string) -> s.ToCharArray() |> Array.tryFind (fun c -> Char.IsNumber c) |> Option.isNone
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.NoNumbers)
            
        static member noSpecialCharacters (input: string) =
            fun (s: string) -> s.ToCharArray() |> Array.tryFind (fun c -> Char.IsLetterOrDigit c |> not) |> Option.isNone
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.NoSpecialCharacters)

        static member isEmail (input: string) =
            StringValidators.matches Regex.email input

        static member isFloat (input: string) =
            fun (s: string) -> 
                try 
                    float s |> ignore
                    true
                with _ -> false
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Float)

        static member isInt (input: string) =
            fun (s: string) -> 
                try 
                    float s |> ignore
                    true
                with _ -> false
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Int)
            
        static member isUrl (input: string) =
            StringValidators.matches Regex.url input

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never)>]
module RecoilExtensions =
    open Feliz

    type Recoil with
        /// Accepts an atom of a ValidationState and list of validators returning a setter function that will update
        /// the atom if the result changes.
        static member inline useValidation (recoilValue: RecoilValue<ValidationState<'T,'Custom>,ReadWrite>, validators: ('T -> Result<'T,ValidationError<'Custom>>) list) =
            let validator = 
                React.useMemo (
                    (fun () -> Validation.validationFamily(validators  |> RecoilValue.lift) recoilValue
                ), [| validators :> obj; recoilValue :> obj |])

            Recoil.useCallbackRef(fun setter (newValue: 'T) -> setter.set(validator, Some newValue))

[<Erase>]
type validate =
    static member inline custom (validator: 'T -> Result<'T,ValidationError<'Custom>>) = 
        Validation.GenericValidators.custom validator
        
    static member inline equals (value: 'T) = Validation.GenericValidators.eq value

    static member inline gt (value: 'T) = Validation.GenericValidators.gt value

    static member inline gte (value: 'T) = Validation.GenericValidators.gte value
        
    static member inline lt (value: 'T) = Validation.GenericValidators.lt value
        
    static member inline lte (value: 'T) = Validation.GenericValidators.lte value

    static member inline option (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) = 
        Validation.OptionValidators.map validators

    static member inline result (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) = 
        Validation.ResultValidators.map validators

[<RequireQualifiedAccess>]
module validate =
    open Microsoft.FSharp

    let isError = fun recoilValue -> Validation.ResultValidators.isError recoilValue

    let isNone = fun recoilValue -> Validation.OptionValidators.isNone recoilValue
    
    let isOk = fun recoilValue -> Validation.ResultValidators.isOk recoilValue
    
    let isSome = fun recoilValue -> Validation.OptionValidators.isSome recoilValue

    [<Erase;RequireQualifiedAccess>]
    type array =
        static member inline contains (value: 'T) = Validation.GenericValidators.contains value
        
        static member inline forAll (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) = 
            Validation.GenericValidators.forAllArray validators
        
        static member inline lengthOf (value: int) = Validation.GenericValidators.lengthOf value
        
        static member inline maxLengthOf (value: int) = Validation.GenericValidators.maxLengthOf value
        
        static member inline minLengthOf (value: int) = Validation.GenericValidators.minLengthOf value

    [<Erase;RequireQualifiedAccess>]
    type list =
        static member inline contains (value: 'T) = Validation.GenericValidators.contains value

        static member inline forAll (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) = 
            Validation.GenericValidators.forAllList validators

        static member inline lengthOf (value: int) = Validation.GenericValidators.lengthOf value

        static member inline maxLengthOf (value: int) = Validation.GenericValidators.maxLengthOf value

        static member inline minLengthOf (value: int) = Validation.GenericValidators.minLengthOf value

    [<Erase;RequireQualifiedAccess>]
    type resizeArray =
        static member inline contains (value: 'T) = Validation.GenericValidators.contains value

        static member inline forAll (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) = 
            Validation.GenericValidators.forAllResizeArray validators

        static member inline lengthOf (value: int) = Validation.GenericValidators.lengthOf value

        static member inline maxLengthOf (value: int) = Validation.GenericValidators.maxLengthOf value

        static member inline minLengthOf (value: int) = Validation.GenericValidators.minLengthOf value

    [<Erase;RequireQualifiedAccess>]
    type seq =
        static member inline contains (value: 'T) = Validation.GenericValidators.contains value

        static member inline forAll (validators: ('T -> Result<'T,ValidationError<'Custom>>) list) = 
            Validation.GenericValidators.forAllSeq validators

        static member inline lengthOf (value: int) = Validation.GenericValidators.lengthOf value

        static member inline maxLengthOf (value: int) = Validation.GenericValidators.maxLengthOf value

        static member inline minLengthOf (value: int) = Validation.GenericValidators.minLengthOf value

    [<Erase;RequireQualifiedAccess>]
    type string =
        static member inline equals (value: float) = Validation.StringValidators.eqFloat value
        static member inline equals (value: int) = Validation.StringValidators.eqInt value
        static member inline equals (value: Core.string) = Validation.StringValidators.eqStr value

        static member inline gt (value: float) = Validation.StringValidators.gtFloat value
        static member inline gt (value: int) = Validation.StringValidators.gtInt value
        
        static member inline gte (value: float) = Validation.StringValidators.gteFloat value
        static member inline gte (value: int) = Validation.StringValidators.gteInt value

        static member inline lengthOf (value: int) = Validation.StringValidators.lengthOf value
        
        static member inline lt (value: float) = Validation.StringValidators.ltFloat value
        static member inline lt (value: int) = Validation.StringValidators.ltInt value
                
        static member inline lte (value: float) = Validation.StringValidators.lteFloat value
        static member inline lte (value: int) = Validation.StringValidators.lteInt value
    
        static member inline matches (value: Regex) = Validation.StringValidators.matches value
        
        static member inline maxLengthOf (value: int) = Validation.StringValidators.maxLengthOf value
                
        static member inline minLengthOf (value: int) = Validation.StringValidators.minLengthOf value

    [<RequireQualifiedAccess>]
    module string =
        let hasNumbers = fun recoilValue -> Validation.StringValidators.hasNumbers recoilValue

        let hasSpecialCharacters = fun recoilValue -> Validation.StringValidators.hasSpecialCharacters recoilValue

        let isEmail = fun recoilValue -> Validation.StringValidators.isEmail recoilValue

        let isFloat = fun recoilValue -> Validation.StringValidators.isFloat recoilValue
    
        let isInt = fun recoilValue -> Validation.StringValidators.isInt recoilValue
    
        let isUrl = fun recoilValue -> Validation.StringValidators.isUrl recoilValue

        let notBlank = fun recoilValue -> Validation.StringValidators.notBlank recoilValue
                
        let noNumbers = fun recoilValue -> Validation.StringValidators.noNumbers recoilValue

        let noSpecialCharacters = fun recoilValue -> Validation.StringValidators.noSpecialCharacters recoilValue
