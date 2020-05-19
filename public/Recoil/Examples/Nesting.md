# Feliz.Recoil - Nesting Example

This example shows how you can construct more complex representations.

```fsharp:recoil-nesting
open Feliz
open Feliz.Recoil
open Zanaptak.TypedCssClasses

type Bulma = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.5/css/bulma.min.css", Naming.PascalCase>
type FA = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.13.0/css/fontawesome.min.css", Naming.PascalCase>

type State =
    { Age: int
      FirstName: string
      LastName: string
      FullName: string
      Job: string }

let age =
    atom {
        def 26
    }

let firstName =
    atom {
        def "Cody"
    }

let lastName =
    atom {
        def "Johnson"
    }

let fullName =
    selector {
        get (fun getter ->
            sprintf "%s %s" 
                (getter.get(firstName))
                (getter.get(lastName))
        )
    }

let job =
    atom {
        def "F# Developer"
    }

let state =
    selector {
        get (fun getter ->
            { Age = getter.get(age)
              FirstName = getter.get(firstName)
              LastName = getter.get(lastName)
              FullName = getter.get(fullName)
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
    let age,setAge = Recoil.useState(age)

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.number
            prop.step 1
            prop.onTextChange <| fun s ->
                try (int s |> setAge)
                with _ -> ()
            prop.defaultValue age
        ]
    ])

let firstNameComp = React.functionComponent(fun () ->
    let firstName,setFirstName = Recoil.useState(firstName)

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.maxLength 40
            prop.onTextChange setFirstName
            prop.defaultValue firstName
        ]
    ])

let lastNameComp = React.functionComponent(fun () ->
    let lastName,setLastName = Recoil.useState(lastName)

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.maxLength 40
            prop.onTextChange setLastName
            prop.defaultValue lastName
        ]
    ])

let jobComp = React.functionComponent(fun () ->
    let job,setJob = Recoil.useState(job)

    Html.div [
        Html.input [
            prop.style [ maxWidth ]
            prop.classes [ Bulma.Input ]
            prop.type'.text
            prop.maxLength 40
            prop.onTextChange setJob
            prop.defaultValue job
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
                prop.text (sprintf "Age: %i" (state.Age))
            ]
            Html.div [
                prop.text (sprintf "First Name: %s" state.FirstName)
            ]
            Html.div [
                prop.text (sprintf "Last Name: %s" state.LastName)
            ]
            Html.div [
                prop.text (sprintf "Full Name: %s" state.FullName)
            ]
            Html.div [
                prop.text (sprintf "Job: %s" state.Job)
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
