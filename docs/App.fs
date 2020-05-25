module App

open Browser.Dom
open Css
open Elmish
open Feliz
open Feliz.Markdown
open Feliz.Recoil
open Fable.SimpleHttp
open Feliz.Router
open Fable.Core.JsInterop

type Highlight =
    static member inline highlight (properties: IReactProperty list) =
        Interop.reactApi.createElement(importDefault "react-highlight", createObj !!properties)

let currentPath = 
    atom { 
        key "currentPath"
        def (Router.currentUrl())
    }

let currentTab = 
    atom { 
        key "currentTab"
        def (Router.currentUrl())
    }

let currentPathSelector =
    selector {
        key "currentPathSelector"
        get (fun getter -> getter.get(currentPath))
        set (fun setter (segments: string list) ->
            setter.set(currentTab, segments)
            setter.set(currentPath, segments) 
        )
    }

let samples = 
    [ "recoil-basic", Samples.Basic.render()
      "recoil-mixandmatch", Samples.MixAndMatch.render()
      "recoil-bidirectionalselectors", Samples.BidirectionalSelectors.render()
      "recoil-reset", Samples.Reset.render()
      "recoil-async", Samples.Async.render()
      "recoil-callback", Samples.Callback.render()
      "recoil-loadable", Samples.Loadable.render()
      "recoil-previous", Samples.Previous.render()
      "recoil-computationexpressions", Samples.ComputationExpressions.render()
      "recoil-nesting", Samples.Nesting.render()
      "recoil-logger", Samples.Logger.render()
      "recoil-elmish", Samples.Elmish.render()
      "recoil-composition", Samples.Composition.render() ]
      //"recoil-atomfamily", Samples.AtomFamily.render() 

let githubPath (rawPath: string) =
    let parts = rawPath.Split('/')
    if parts.Length > 5
    then sprintf "http://www.github.com/%s/%s" parts.[3] parts.[4]
    else rawPath

/// Renders a code block from markdown using react-highlight.
/// Injects sample React components when the code block has language of the format <language>:<sample-name>
let codeBlockRenderer' = React.functionComponent(fun (input: {| codeProps: Markdown.ICodeProperties |}) ->
    if input.codeProps.language <> null && input.codeProps.language.Contains ":" then
        let languageParts = input.codeProps.language.Split(':')
        let sampleName = languageParts.[1]
        let sampleApp =
            samples
            |> List.tryFind (fun (name, _) -> name = sampleName)
            |> Option.map snd
            |> Option.defaultValue (Html.h1 [
                prop.style [ style.color.crimson ];
                prop.text (sprintf "Could not find sample app '%s'" sampleName)
            ])
        Html.div [
            sampleApp
            Highlight.highlight [
                prop.className "fsharp"
                prop.text(input.codeProps.value)
            ]
        ]
    else
        Highlight.highlight [
            prop.className "fsharp"
            prop.text(input.codeProps.value)
        ])

let codeBlockRenderer (codeProps: Markdown.ICodeProperties) = codeBlockRenderer' {| codeProps = codeProps |}

let readme = sprintf "https://raw.githubusercontent.com/%s/%s/master/README.md"
let contributing = sprintf "https://raw.githubusercontent.com/Zaid-Ajaj/Feliz/master/public/Feliz/Contributing.md"

let (|PathPrefix|) (segments: string list) (path: string list) =
    if path.Length > segments.Length then
        match List.splitAt segments.Length path with
        | start,end' when start = segments -> Some end'
        | _ -> None
    else None

let resolveContent (path: string list) =
    match path with
    | [ Urls.Recoil; Urls.Overview; ] -> [ "Recoil"; "README.md" ]
    | [ Urls.Recoil; Urls.Installation ] -> [ "Recoil"; "Installation.md" ]
    | [ Urls.Recoil; Urls.ReleaseNotes ] -> [ "Recoil"; "RELEASE_NOTES.md" ]
    | [ Urls.Recoil; Urls.Contributing ] -> [ contributing ]
    | PathPrefix [ Urls.Recoil; Urls.API ] (Some res) ->
        match res with
        | [ Urls.Types ] -> [ "Types.md" ]
        | [ Urls.Components ] -> [ "Components.md" ]
        | [ Urls.Functions ] -> [ "Functions.md" ]
        | [ Urls.Hooks ] -> [ "Hooks.md" ]
        | [ Urls.ComputationExpressions ] -> [ "ComputationExpressions.md" ]
        | [ Urls.Elmish ] -> [ "Elmish.md" ]
        | [ Urls.Bridge ] -> [ "Bridge.md" ]
        | _ -> []
        |> fun path -> [ Urls.Recoil; Urls.API ] @ path
    | PathPrefix [ Urls.Recoil; Urls.Examples ] (Some res) ->
        match res with
        | [ Urls.Basic ] -> [ "Basic.md" ]
        | [ Urls.MixAndMatch ] -> [ "MixAndMatch.md" ]
        | [ Urls.BidirectionalSelectors ] -> [ "BidirectionalSelectors.md" ]
        | [ Urls.Reset ] -> [ "Reset.md" ]
        | [ Urls.Async ] -> [ "Async.md" ]
        | [ Urls.Callback ] -> [ "Callback.md" ]
        | [ Urls.Loadable ] -> [ "Loadable.md" ]
        | [ Urls.Previous ] -> [ "Previous.md" ]
        | [ Urls.ComputationExpressions ] -> [ "ComputationExpressions.md" ]
        | [ Urls.Nesting ] -> [ "Nesting.md" ]
        | [ Urls.Logger ] -> [ "Logger.md" ]
        | [ Urls.Elmish ] -> [ "Elmish.md" ]
        | [ Urls.Composition ] -> [ "Composition.md" ]
        | [ Urls.Websockets ] -> [ "Websockets.md" ]
        // Utils - not implemented
        | [ Urls.AtomFamily ] -> [ "AtomFamily.md" ]
        | _ -> []
        |> fun path -> [ Urls.Recoil; Urls.Examples ] @ path
    | _ -> [ "Recoil"; "README.md" ]

let contentPath =
    atom {
        key "contentPath"
        def (Router.currentUrl() |> resolveContent)
    }

let contentSelector =
    selector {
        key "contentSelector"
        get (fun getter -> getter.get(contentPath))
        set (fun setter (newValue: string list) ->
            setter.set(currentPathSelector, newValue)

            resolveContent newValue
            |> fun res -> setter.set(contentPath, res)
        )
    }

let currentMarkdownPath =
    selector {
        key "currentMarkdownPath"
        get (fun getter ->
            match getter.get(contentSelector) with
            | [ one: string ] when one.StartsWith "http" -> one
            | segments -> String.concat "/" segments
        )
    }

let markdownSelector =
    selector {
        key "markdownSelector"
        get (fun getter ->
            async {
                let! (statusCode, responseText) = Http.get (getter.get(currentMarkdownPath))
                if statusCode = 200 then return Ok responseText
                else return Error responseText
            }
        )
    }

let renderMarkdown = React.functionComponent(fun (input: {| content: string |}) ->
    let path = Recoil.useValue(currentMarkdownPath)

    Html.div [
        prop.className [ Bulma.Content; "scrollbar" ]
        prop.style [ 
            style.width (length.percent 100)
            style.padding (0,20)
        ]
        prop.children [
            if path.StartsWith "https://raw.githubusercontent.com" then
                Html.h2 [
                    Html.i [ prop.className [ FA.Fa; FA.FaGithub ] ]
                    Html.anchor [
                        prop.style [ style.marginLeft 10; style.color.lightGreen ]
                        prop.href (githubPath path)
                        prop.text "View on Github"
                    ]
                ]

            Markdown.markdown [
                markdown.source input.content
                markdown.escapeHtml false
                markdown.renderers [
                    markdown.renderers.code codeBlockRenderer
                ]
            ]
        ]
    ])

module MarkdownLoader =
    let render = React.memo(fun () ->
        let content = Recoil.useValue(markdownSelector)

        match content with
        | Ok content -> renderMarkdown {| content = content |}
        | Error error ->
            Html.h1 [
                prop.style [ style.color.crimson ]
                prop.text error
            ])

// A collapsable nested menu for the sidebar
// keeps internal state on whether the items should be visible or not based on the collapsed state
let nestedMenuList' = React.functionComponent(fun (input: {| name: string; basePath: string list; elems: (string list -> Fable.React.ReactElement) list |}) ->
    let tab,setTab = Recoil.useState(currentTab)

    let collapsed = 
        match tab with
        | [ ] -> false
        | _ -> 
            input.basePath 
            |> List.indexed 
            |> List.forall (fun (i, segment) -> 
                List.tryItem i tab
                |> Option.map ((=) segment) 
                |> Option.defaultValue false) 

    Html.li [
        Html.anchor [
            prop.className Bulma.IsUnselectable
            prop.onClick <| fun _ -> 
                match collapsed with
                | true -> setTab(input.basePath |> List.rev |> List.tail |> List.rev)
                | false -> setTab(input.basePath)
            prop.children [
                Html.i [
                    prop.style [ style.marginRight 10 ]
                    prop.className [
                        FA.Fa
                        if not collapsed then FA.FaAngleDown else FA.FaAngleUp
                    ]
                ]
                Html.span input.name
            ]
        ]

        Html.ul [
            prop.className Bulma.MenuList
            prop.style [ 
                if not collapsed then yield! [ style.display.none ] 
            ]
            prop.children (input.elems |> List.map (fun f -> f input.basePath))
        ]
    ])

// top level label
let menuLabel' = React.functionComponent (fun (input: {| content: string |}) ->
    Html.p [
        prop.className [ Bulma.MenuLabel; Bulma.IsUnselectable ]
        prop.text input.content
    ])

// top level menu
let menuList' = React.functionComponent(fun (input: {| items: Fable.React.ReactElement list |}) ->
    Html.ul [
        prop.className Bulma.MenuList
        prop.style [ style.width (length.percent 95) ]
        prop.children input.items
    ])

let menuItem' = React.functionComponent(fun (input: {| name: string; path: string list |}) ->
    let path = Recoil.useValue(currentTab)

    Html.li [
        Html.anchor [
            prop.className [
                if path = input.path then Bulma.IsActive
                if path = input.path then Bulma.HasBackgroundPrimary
            ]
            prop.text input.name
            prop.href (sprintf "#/%s" (String.concat "/" input.path))
        ]
    ])

let menuLabel (content: string) =
    menuLabel' {| content = content |}

let menuList (items: Fable.React.ReactElement list) =
    menuList' {| items = items |}

let nestedMenuItem (name: string) (extendedPath: string list) (basePath: string list) =
    let path = basePath @ extendedPath
    menuItem' 
        {| name = name
           path = path |}

let nestedMenuList (name: string) (basePath: string list) (items: (string list -> Fable.React.ReactElement) list) =
    nestedMenuList' 
        {| name = name
           basePath = basePath
           elems = items |}

let subNestedMenuList (name: string) (basePath: string list) (items: (string list -> Fable.React.ReactElement) list) (addedBasePath: string list) =
    nestedMenuList' 
        {| name = name
           basePath = (addedBasePath @ basePath)
           elems = items |}

let menuItem (name: string) (basePath: string list) =
    menuItem' 
        {| name = name
           path = basePath |}

let allItems = React.memo(fun () ->
    Html.div [
        prop.className "scrollbar"
        prop.children [
            menuList [
                menuItem "Overview" [ ]
                menuItem "Installation" [ Urls.Recoil; Urls.Installation ]
                menuItem "Release Notes" [ Urls.Recoil; Urls.ReleaseNotes ]
                menuItem "Contributing" [ Urls.Recoil; Urls.Contributing ]
                nestedMenuList "API Reference" [ Urls.Recoil; Urls.API ] [
                    nestedMenuItem "Types" [ Urls.Types ]
                    nestedMenuItem "Components" [ Urls.Components ]
                    nestedMenuItem "Functions" [ Urls.Functions ]
                    nestedMenuItem "Hooks" [ Urls.Hooks ]
                    nestedMenuItem "Computation Expressions" [ Urls.ComputationExpressions ]
                    nestedMenuItem "Elmish" [ Urls.Elmish ]
                    nestedMenuItem "Bridge" [ Urls.Bridge ]
                ]
                menuLabel "Examples"
                menuItem "Basic" [ Urls.Recoil; Urls.Examples; Urls.Basic ]
                menuItem "Mix and Match" [ Urls.Recoil; Urls.Examples; Urls.MixAndMatch ]
                menuItem "Bi-directional Selectors" [ Urls.Recoil; Urls.Examples; Urls.BidirectionalSelectors ]
                menuItem "Reset" [ Urls.Recoil; Urls.Examples; Urls.Reset ]
                menuItem "Asynchronous Selectors" [ Urls.Recoil; Urls.Examples; Urls.Async ]
                menuItem "Callbacks" [ Urls.Recoil; Urls.Examples; Urls.Callback ]
                menuItem "Loadables" [ Urls.Recoil; Urls.Examples; Urls.Loadable ]
                menuItem "Previous" [ Urls.Recoil; Urls.Examples; Urls.Previous ]
                menuItem "Computation Expressions" [ Urls.Recoil; Urls.Examples; Urls.ComputationExpressions ]
                menuItem "Nesting" [ Urls.Recoil; Urls.Examples; Urls.Nesting ]
                menuItem "Debug Logger" [ Urls.Recoil; Urls.Examples; Urls.Logger ]
                menuItem "Elmish" [ Urls.Recoil; Urls.Examples; Urls.Elmish ]
                menuItem "Composition" [ Urls.Recoil; Urls.Examples; Urls.Composition ]
                menuItem "With Websockets" [ Urls.Recoil; Urls.Examples; Urls.Websockets ]
                //menuItem "Atom Family" [ Urls.Recoil; Urls.Examples; Urls.AtomFamily ]
            ]
        ]
    ])

let sidebar = React.memo(fun () ->
    // the actual nav bar
    Html.aside [
        prop.className Bulma.Menu
        prop.style [
            style.width (length.perc 100)
        ]
        prop.children [ 
            menuLabel "Feliz.Recoil"
            allItems() 
        ]
    ])

let main = React.memo(fun () ->
    Html.div [
        prop.className [ Bulma.Tile; Bulma.IsAncestor ]
        prop.children [
            Html.div [
                prop.className [ Bulma.Tile; Bulma.Is2 ]
                prop.children [ sidebar() ]
            ]

            Html.div [
                prop.className Bulma.Tile
                prop.style [ style.paddingTop 30 ]
                prop.children [
                    React.suspense ([
                        MarkdownLoader.render() 
                    ], Html.none)
                ]
            ]
        ]
    ])

let render' = React.memo(fun () ->
    let setPath = Recoil.useSetState(contentSelector)

    let application =
        Html.div [
            prop.style [ 
                style.padding 30
            ]
            prop.children [ main() ]
        ]

    Router.router [
        Router.onUrlChanged setPath
        Router.application application
    ])

let appMain = React.memo(fun () ->
    Recoil.root [
        render'()
    ])

ReactDOM.render(appMain(), document.getElementById "root")
