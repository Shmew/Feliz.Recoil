namespace Feliz.Recoil.Validation

open Fable.Core
open Feliz.Recoil
open Feliz.Recoil.Result
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
    
type Validator<'T,'Custom> = RecoilValue<Result<'T,ValidationError<'Custom>>,ReadOnly> -> RecoilValue<Result<'T,ValidationError<'Custom>>,ReadOnly>

[<EditorBrowsable(EditorBrowsableState.Never)>]
module Validation =
    open Utils

    module internal Regex =
        let email = Regex @"^(([^<>()\[\]\\.,;:\s@""]+(\.[^<>()\[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$"
        let url = Regex @"^((([A-Za-z]{3,9}:(?:\/\/)?)(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-_]*)?\??(?:[\-\+=&;%@\.\w_]*)#?(?:[\.\!\/\\\w]*))?)$"

    let internal (>>=!) f m = RecoilValue.Result.bind f m

    let internal validationFamily<'T,'Custom when 'Custom : equality and 'T : equality> (validators: Validator<'T,'Custom> list) =
        selectorFamily {
            key "__recoil__forms__/__validationFamily"
            get (fun (resultAtom: RecoilValue<ValidationState<'T,'Custom>,ReadWrite>) getter -> 
                getter.get(resultAtom) |> ignore
                None)
            set (fun (resultAtom: RecoilValue<ValidationState<'T,'Custom>,ReadWrite>) setter (newValue: 'T option) ->
                newValue
                |> Option.iter (fun newValue ->
                    let newRecoilValue = RecoilResult.lift newValue

                    let result =
                        if validators.Length > 0 then
                            validators 
                            |> List.fold (fun state f -> f state) newRecoilValue
                            |> setter.get
                        else Ok newValue

                    match result, setter.get(resultAtom) with
                    | Ok res, ValidationState.Valid currentRes when res = currentRes -> ()
                    | Ok res, _ -> setter.set(resultAtom, ValidationState.Valid(res))
                    | Error e, ValidationState.Invalid currentE when e = currentE -> ()
                    | Error e, _ -> setter.set(resultAtom, ValidationState.Invalid e)
                ))
        }

    type GenericValidators =
        static member compare (input: RecoilValue<Result<'T,ValidationError<_>>,_>) (errorCase: ValidationError<_>) (comparer: 'T -> bool) =
            (fun s ->
                if comparer s then Ok s
                else Error errorCase)
            >>=! input

        static member tryCompare (input: RecoilValue<Result<'T,ValidationError<_>>,_>) (errorCase: ValidationError<_>) (comparer: 'T -> bool) =
            (fun s ->
                try 
                    if comparer s then Ok s
                    else Error errorCase
                with _ -> Error ValidationError<'Custom>.Parse) 
            >>=! input
                
        static member contains (value: 'T) (input: RecoilValue<Result<#seq<'T>,ValidationError<_>>,_>) =
            fun inner -> Seq.contains value inner
            |> GenericValidators.compare input (ValidationError<'Custom>.Contains)

        static member custom (validator: 'T -> Result<'T,ValidationError<'Custom>>) (input: RecoilValue<Result<'T,ValidationError<_>>,_>) =
            validator >>=! input

        static member eq (value: 'T) (input: RecoilValue<Result<'T,ValidationError<_>>,_>) =
            fun inner -> inner = value
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member forAllArray (validators: Validator<'T,'Custom> list) (input: RecoilValue<Result<'T [],ValidationError<_>>,_>) =
            input
            |> RecoilResult.bind (
                Array.map (fun state ->
                    validators
                    |> Seq.fold (fun state f -> 
                        state |> f
                    ) (RecoilValue.lift(Ok state)))
                >> RecoilResult.Array.sequence
            )

        static member forAllList (validators: Validator<'T,'Custom> list) (input: RecoilValue<Result<'T list,ValidationError<_>>,_>) =
            input
            |> RecoilResult.bind (
                List.map (fun state ->
                    validators
                    |> List.fold (fun state f -> 
                        state |> f
                    ) (RecoilValue.lift(Ok state)))
                >> RecoilResult.List.sequence
            )

        static member forAllResizeArray (validators: Validator<'T,'Custom> list) (input: RecoilValue<Result<ResizeArray<'T>,ValidationError<_>>,_>) =
            input
            |> RecoilResult.bind (
                Seq.map (fun state ->
                    validators
                    |> Seq.fold (fun state f -> 
                        state |> f
                    ) (RecoilValue.lift(Ok state)))
                >> ResizeArray
                >> RecoilResult.ResizeArray.sequence
            )

        static member forAllSeq (validators: Validator<'T,'Custom> list) (input: RecoilValue<Result<seq<'T>,ValidationError<_>>,_>) =
            input
            |> RecoilResult.bind (
                Seq.map (fun state ->
                    validators
                    |> Seq.fold (fun state f -> 
                        state |> f
                    ) (RecoilValue.lift(Ok state)))
                >> RecoilResult.Seq.sequence
            )

        static member gt (value: 'T) (input: RecoilValue<Result<'T,ValidationError<_>>,_>) =
            fun inner -> inner > value
            |> GenericValidators.compare input (ValidationError<'Custom>.Gt)

        static member gte (value: 'T) (input: RecoilValue<Result<'T,ValidationError<_>>,_>) =
            fun inner -> inner >= value
            |> GenericValidators.compare input (ValidationError<'Custom>.Gte)

        static member lengthOf (value: int) (input: RecoilValue<Result<#seq<'T>,ValidationError<_>>,_>) =
            fun inner -> (Seq.length inner) = value
            |> GenericValidators.compare input (ValidationError<'Custom>.Length(value))

        static member lt (value: 'T) (input: RecoilValue<Result<'T,ValidationError<_>>,_>) =
            fun inner -> inner < value
            |> GenericValidators.compare input (ValidationError<'Custom>.Lt)

        static member lte (value: 'T) (input: RecoilValue<Result<'T,ValidationError<_>>,_>) =
            fun inner -> inner <= value
            |> GenericValidators.compare input (ValidationError<'Custom>.Lte)

        static member maxLengthOf (value: int) (input: RecoilValue<Result<#seq<'T>,ValidationError<_>>,_>) =
            fun inner -> (Seq.length inner) <= value
            |> GenericValidators.compare input (ValidationError<'Custom>.MaxLength)

        static member minLengthOf (value: int) (input: RecoilValue<Result<#seq<'T>,ValidationError<_>>,_>) =
            fun inner -> (Seq.length inner) >= value
            |> GenericValidators.compare input (ValidationError<'Custom>.MinLength)

    type OptionValidators =
        static member isSome (input: RecoilValue<Result<'T option,ValidationError<_>>,_>) =
            function | Some _ -> true | None -> false
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member isNone (input: RecoilValue<Result<'T option,ValidationError<_>>,_>) =
            function | Some -> false | None -> true
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member map (validators: Validator<'T,'Custom> list) (input: RecoilValue<Result<'T option,ValidationError<_>>,_>) =
            let folder (inner: RecoilValue<Result<'T,ValidationError<_>>,_>) =
                validators
                |> List.fold (fun state f -> f state) inner

            RecoilResultOption.bind(fun t -> 
                recoil { return Ok t }
                |> folder
                |> RecoilResult.map Some
            ) input

    type ResultValidators =
        static member isOk (input: RecoilValue<Result<Result<'T,_>,ValidationError<_>>,_>) =
            function | Ok _ -> true | Error _ -> false
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member isError (input: RecoilValue<Result<Result<'T,_>,ValidationError<_>>,_>) =
            function | Ok _ -> false | Error _ -> true
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member map (validators: Validator<'T,'Custom> list) (input: RecoilValue<Result<Result<'T,'Error>,ValidationError<_>>,_>) =
            let folder (inner: RecoilValue<Result<'T,ValidationError<_>>,_>) =
                validators
                |> List.fold (fun state f -> f state) inner
                
            RecoilResultResult.bind (fun t -> 
                recoil { return Ok t }
                |> folder
                |> RecoilResult.map Ok
            ) input

    type StringValidators =
        static member eqFloat (value: float) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) = value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Eq)
        static member eqInt (value: int) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) = (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Eq)
        static member eqStr (value: string) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> s = value
            |> GenericValidators.compare input (ValidationError<'Custom>.Eq)

        static member gtFloat (value: float) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) > value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Gt)
        static member gtInt (value: int) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) > (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Gt)

        static member gteFloat (value: float) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) >= value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Gte)
        static member gteInt (value: int) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) >= (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Gte)

        static member hasSpecialCharacters (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> s.ToCharArray() |> Array.tryFind (fun c -> Char.IsLetterOrDigit c) |> Option.isSome
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.HasSpecialCharacters)

        static member hasNumbers (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> s.ToCharArray() |> Array.tryFind (fun c -> Char.IsNumber c) |> Option.isSome
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.HasNumbers)

        static member lengthOf (value: int) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> s.Length = value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Length(value))

        static member ltFloat (value: float) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) < value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Lt)
        static member ltInt (value: int) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) < (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Lt)

        static member lteFloat (value: float) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) <= value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Lte)
        static member lteInt (value: int) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun s -> (float s) <= (float value)
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Lte)
        
        static member matches (value: Regex) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            GenericValidators.tryCompare input (ValidationError<'Custom>.Match) value.IsMatch

        static member maxLengthOf (value: int) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> s.Length <= value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.MaxLength)

        static member minLengthOf (value: int) (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> s.Length >= value
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.MinLength)

        static member notBlank (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            (fun s ->
                if String.IsNullOrEmpty s || String.IsNullOrWhiteSpace s then
                    Error ValidationError<'Custom>.Blank
                else Ok s)
            >>=! input

        static member noNumbers (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> s.ToCharArray() |> Array.tryFind (fun c -> Char.IsNumber c) |> Option.isNone
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.NoNumbers)
            
        static member noSpecialCharacters (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> s.ToCharArray() |> Array.tryFind (fun c -> Char.IsLetterOrDigit c |> not) |> Option.isNone
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.NoSpecialCharacters)

        static member isEmail (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            StringValidators.matches Regex.email input

        static member isFloat (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> 
                try 
                    float s |> ignore
                    true
                with _ -> false
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Float)

        static member isInt (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            fun (s: string) -> 
                try 
                    float s |> ignore
                    true
                with _ -> false
            |> GenericValidators.tryCompare input (ValidationError<'Custom>.Int)
            
        static member isUrl (input: RecoilValue<Result<string,ValidationError<_>>,_>) =
            StringValidators.matches Regex.url input

[<AutoOpen;EditorBrowsable(EditorBrowsableState.Never)>]
module RecoilExtensions =
    type Recoil with
        /// Accepts an atom of a ValidationState and list of validators returning a setter function that will update
        /// the atom if the result changes.
        static member useValidation (recoilValue: RecoilValue<ValidationState<'T,'Custom>,ReadWrite>, validators: Validator<'T,'Custom> list) =
            let validator = Validation.validationFamily(validators) recoilValue

            Recoil.useCallbackRef(fun setter (newValue: 'T) -> setter.set(validator, Some newValue))

[<Erase>]
type validate =
    static member inline custom (validator: 'T -> Result<'T,ValidationError<'Custom>>) : Validator<'T,'Custom> = Validation.GenericValidators.custom validator
        
    static member inline equals (value: 'T) : Validator<'T,'Custom> = Validation.GenericValidators.eq value

    static member inline gt (value: 'T) : Validator<'T,'Custom> = Validation.GenericValidators.gt value

    static member inline gte (value: 'T) : Validator<'T,'Custom> = Validation.GenericValidators.gte value
        
    static member inline lt (value: 'T) : Validator<'T,'Custom> = Validation.GenericValidators.lt value
        
    static member inline lte (value: 'T) : Validator<'T,'Custom> = Validation.GenericValidators.lte value

    static member inline option (validators: Validator<'T,'Custom> list) : Validator<'T option,'Custom> = Validation.OptionValidators.map validators

    static member inline result (validators: Validator<'T,'Custom> list) : Validator<Result<'T,'Error>,'Custom> = Validation.ResultValidators.map validators

[<RequireQualifiedAccess>]
module validate =
    open Microsoft.FSharp

    let isError : Validator<Result<'T,'Error>,'Custom> = fun recoilValue -> Validation.ResultValidators.isError recoilValue

    let isNone : Validator<'T option,'Custom> = fun recoilValue -> Validation.OptionValidators.isNone recoilValue
    
    let isOk : Validator<Result<'T,'Error>,'Custom> = fun recoilValue -> Validation.ResultValidators.isOk recoilValue
    
    let isSome : Validator<'T option,'Custom> = fun recoilValue -> Validation.OptionValidators.isSome recoilValue

    [<Erase;RequireQualifiedAccess>]
    type array =
        static member inline contains (value: 'T) : Validator<'T [],'Custom> = Validation.GenericValidators.contains value
        
        static member inline forAll (validators: Validator<'T,'Custom> list) : Validator<'T [],'Custom> = Validation.GenericValidators.forAllArray validators
        
        static member inline lengthOf (value: int) : Validator<'T [],'Custom> = Validation.GenericValidators.lengthOf value
        
        static member inline maxLengthOf (value: int) : Validator<'T [],'Custom> = Validation.GenericValidators.maxLengthOf value
        
        static member inline minLengthOf (value: int) : Validator<'T [],'Custom> = Validation.GenericValidators.minLengthOf value

    [<Erase;RequireQualifiedAccess>]
    type list =
        static member inline contains (value: 'T) : Validator<'T list,'Custom> = Validation.GenericValidators.contains value

        static member inline forAll (validators: Validator<'T,'Custom> list) : Validator<'T list,'Custom> = Validation.GenericValidators.forAllList validators

        static member inline lengthOf (value: int) : Validator<'T list,'Custom> = Validation.GenericValidators.lengthOf value

        static member inline maxLengthOf (value: int) : Validator<'T list,'Custom> = Validation.GenericValidators.maxLengthOf value

        static member inline minLengthOf (value: int) : Validator<'T list,'Custom> = Validation.GenericValidators.minLengthOf value

    [<Erase;RequireQualifiedAccess>]
    type resizeArray =
        static member inline contains (value: 'T) : Validator<ResizeArray<'T>,'Custom> = Validation.GenericValidators.contains value

        static member inline forAll (validators: Validator<'T,'Custom> list) : Validator<ResizeArray<'T>,'Custom> = Validation.GenericValidators.forAllResizeArray validators

        static member inline lengthOf (value: int) : Validator<ResizeArray<'T>,'Custom> = Validation.GenericValidators.lengthOf value

        static member inline maxLengthOf (value: int) : Validator<ResizeArray<'T>,'Custom> = Validation.GenericValidators.maxLengthOf value

        static member inline minLengthOf (value: int) : Validator<ResizeArray<'T>,'Custom> = Validation.GenericValidators.minLengthOf value

    [<Erase;RequireQualifiedAccess>]
    type seq =
        static member inline contains (value: 'T) : Validator<seq<'T>,'Custom> = Validation.GenericValidators.contains value

        static member inline forAll (validators: Validator<'T,'Custom> list) : Validator<seq<'T>,'Custom> = Validation.GenericValidators.forAllSeq validators

        static member inline lengthOf (value: int) : Validator<seq<'T>,'Custom> = Validation.GenericValidators.lengthOf value

        static member inline maxLengthOf (value: int) : Validator<seq<'T>,'Custom> = Validation.GenericValidators.maxLengthOf value

        static member inline minLengthOf (value: int) : Validator<seq<'T>,'Custom> = Validation.GenericValidators.minLengthOf value

    [<Erase;RequireQualifiedAccess>]
    type string =
        static member inline equals (value: float) : Validator<Core.string,'Custom> = Validation.StringValidators.eqFloat value
        static member inline equals (value: int) : Validator<Core.string,'Custom> = Validation.StringValidators.eqInt value
        static member inline equals (value: Core.string) : Validator<Core.string,'Custom> = Validation.StringValidators.eqStr value

        static member inline gt (value: float) : Validator<Core.string,'Custom> = Validation.StringValidators.gtFloat value
        static member inline gt (value: int) : Validator<Core.string,'Custom> = Validation.StringValidators.gtInt value
        
        static member inline gte (value: float) : Validator<Core.string,'Custom> = Validation.StringValidators.gteFloat value
        static member inline gte (value: int) : Validator<Core.string,'Custom> = Validation.StringValidators.gteInt value

        static member inline lengthOf (value: int) : Validator<Core.string,'Custom> = Validation.StringValidators.lengthOf value
        
        static member inline lt (value: float) : Validator<Core.string,'Custom> = Validation.StringValidators.ltFloat value
        static member inline lt (value: int) : Validator<Core.string,'Custom> = Validation.StringValidators.ltInt value
                
        static member inline lte (value: float) : Validator<Core.string,'Custom> = Validation.StringValidators.lteFloat value
        static member inline lte (value: int) : Validator<Core.string,'Custom> = Validation.StringValidators.lteInt value
    
        static member inline matches (value: Regex) : Validator<Core.string,'Custom> = Validation.StringValidators.matches value
        
        static member inline maxLengthOf (value: int) : Validator<Core.string,'Custom> = Validation.StringValidators.maxLengthOf value
                
        static member inline minLengthOf (value: int) : Validator<Core.string,'Custom> = Validation.StringValidators.minLengthOf value

    [<RequireQualifiedAccess>]
    module string =
        let hasNumbers : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.hasNumbers recoilValue

        let hasSpecialCharacters : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.hasSpecialCharacters recoilValue

        let isEmail : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.isEmail recoilValue

        let isFloat : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.isFloat recoilValue
    
        let isInt : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.isInt recoilValue
    
        let isUrl : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.isUrl recoilValue

        let notBlank : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.notBlank recoilValue
                
        let noNumbers : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.noNumbers recoilValue

        let noSpecialCharacters : Validator<Core.string,'Custom> = fun recoilValue -> Validation.StringValidators.noSpecialCharacters recoilValue
