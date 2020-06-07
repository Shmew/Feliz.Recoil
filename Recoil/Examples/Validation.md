# Feliz.Recoil.Validation

This example shows how you can setup validation of form inputs.

```fsharp:recoil-validation
open Css
open Feliz
open Feliz.Recoil
open Feliz.Recoil.Validation
open Zanaptak.TypedCssClasses

type State =
    { Age: ValidationState<int,unit>
      FirstName: ValidationState<string,unit>
      LastName: ValidationState<string,unit>
      Job: ValidationState<string,unit> }

let age =
    atom {
        key "Validation/age"
        def ValidationState<int,unit>.Init
    }

let firstName =
    atom {
        key "Validation/firstName"
        def ValidationState<string,unit>.Init
    }

let lastName =
    atom {
        key "Validation/lastName"
        def ValidationState<string,unit>.Init
    }

let job =
    atom {
        key "Validation/job"
        def ValidationState<string,unit>.Init
    }

let state =
    selector {
        key "Validation/state"
        get (fun getter ->
            { Age = getter.get(age)
              FirstName = getter.get(firstName)
              LastName = getter.get(lastName)
              Job = getter.get(job) }
        )
    }

let maxWidth = style.maxWidth (length.em 30)

let label = React.functionComponent(fun (input: {| name: string |}) ->
    Html.div [
        prop.style [
            style.paddingBottom (length.em 1)
            style.paddingTop (length.em 1)
        ]
        prop.text input.name
    ])

let ageComp = React.functionComponent(fun () ->
    let setAge = Recoil.useValidation(age, [
        validate.gte 14
        validate.lt 99
    ])

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.number
            prop.step 1
            prop.onTextChange <| fun s ->
                try (int s |> setAge)
                with _ -> ()
            prop.defaultValue 0
        ]
    ])

let firstNameComp = React.functionComponent(fun () ->
    let setFirstName = Recoil.useValidation(firstName, [
        validate.string.notBlank
        validate.string.minLengthOf 3
        validate.string.maxLengthOf 15
        validate.string.noNumbers
        validate.string.noSpecialCharacters
    ])

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.maxLength 40
            prop.onTextChange setFirstName
            prop.defaultValue ""
        ]
    ])

let lastNameComp = React.functionComponent(fun () ->
    let setLastName = Recoil.useValidation(lastName, [
        validate.string.notBlank
        validate.string.minLengthOf 3
        validate.string.maxLengthOf 25
        validate.string.noNumbers
        validate.string.noSpecialCharacters
    ])

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.maxLength 40
            prop.onTextChange setLastName
            prop.defaultValue ""
        ]
    ])

let jobComp = React.functionComponent(fun () ->
    let setJob = Recoil.useValidation(job, [
        validate.string.notBlank
        validate.string.minLengthOf 5
        validate.string.maxLengthOf 40
        validate.string.noNumbers
    ])

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.maxLength 40
            prop.onTextChange setJob
            prop.defaultValue ""
        ]
    ])

let detailPanel = React.functionComponent(fun () ->
    let state = Recoil.useValue(state)

    Html.div [
        prop.style [
            style.marginTop (length.em 1)
            maxWidth
        ]
        prop.classes [ Bulma.Box ]
        prop.children [
            Html.div [
                if state.Age.isInvalid then
                    prop.style [ style.color.red ]

                match state.Age with
                | ValidationState.Init -> "Age: "
                | ValidationState.Invalid e -> e.DefaultText
                | ValidationState.Valid age ->
                    (sprintf "Age: %i" age)
                |> prop.text 
            ]
            Html.div [
                if state.FirstName.isInvalid then
                    prop.style [ style.color.red ]

                match state.FirstName with
                | ValidationState.Init -> "First Name: "
                | ValidationState.Invalid e -> e.DefaultText
                | ValidationState.Valid firstName ->
                    (sprintf "First Name: %s" firstName)
                |> prop.text
            ]
            Html.div [
                if state.LastName.isInvalid then
                    prop.style [ style.color.red ]

                match state.LastName with
                | ValidationState.Init -> "Last Name: "
                | ValidationState.Invalid e -> e.DefaultText
                | ValidationState.Valid lastName ->
                    (sprintf "Last Name: %s" lastName)
                |> prop.text
            ]
            Html.div [
                if state.Job.isInvalid then
                    prop.style [ style.color.red ]

                match state.Job with
                | ValidationState.Init -> "Job: "
                | ValidationState.Invalid e -> e.DefaultText
                | ValidationState.Valid job ->
                    (sprintf "Job: %s" job)
                |> prop.text
            ]
        ]
    ])

let render = React.functionComponent(fun () ->
    Recoil.root [
        Html.div [
            prop.classes [ Bulma.Columns ]

            prop.children [
                Html.div [
                    prop.classes [ Bulma.Column ]

                    prop.children [
                        label {| name = "Age:" |}
                        ageComp()
                        label {| name = "First Name:" |}
                        firstNameComp()
                        label {| name = "Last Name:" |}
                        lastNameComp()
                        label {| name = "Job:" |}
                        jobComp()
                    ]
                ]
                Html.div [
                    prop.classes [ Bulma.Column ]
                    
                    prop.children [ 
                        detailPanel() 
                    ]
                ]
            ]
            
        ]
    ])
```
