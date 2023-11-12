
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.JavaScript
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools
// open Fantomas.FakeHelpers
// open Fantomas.FormatConfig
// open Tools.Linting
open Tools.Web
open System
open System.IO

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Feliz.Recoil"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "Fable bindings in Feliz style for Facebook's experimental state management library recoil."

// Author(s) of the project
let author = "Cody Johnson"

// File system information
let solutionFile = "Feliz.Recoil.sln"

// Github repo
let repo = "https://github.com/Shmew/Feliz.Recoil"

let PROJECT_ROOT = __SOURCE_DIRECTORY__ @@ "../"

printfn "Project root is %s" PROJECT_ROOT

// Files to skip Fantomas formatting
let excludeFantomas =
    [ PROJECT_ROOT @@ "src/**/*.fs" ]

// Files that have bindings to other languages where name linting needs to be more relaxed.
let relaxedNameLinting = 
    [ PROJECT_ROOT @@ "src/**/**/*.fs"
      PROJECT_ROOT @@ "src/**/*.fs" ]

// Read additional information from the release notes document
let release = 
    let path = PROJECT_ROOT @@ "RELEASE_NOTES.md"
    ReleaseNotes.load path 

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|Shproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | f when f.EndsWith("shproj") -> Shproj
    | _ -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)
    
let srcGlob    = PROJECT_ROOT @@ "src/**/*.??proj"
let fsSrcGlob  = PROJECT_ROOT @@ "src/**/*.fs"
let fsTestGlob = PROJECT_ROOT @@ "tests/**/*.fs"
let bin        = PROJECT_ROOT @@ "bin"
let temp       = PROJECT_ROOT @@ "temp"
let objFolder  = PROJECT_ROOT @@ "obj"
let pub        = PROJECT_ROOT @@ "public"
let genGlob    = PROJECT_ROOT @@ "src/**/*.Generator.*.fsproj"
let libGlob    = PROJECT_ROOT @@ "src/**/*.fsproj"
let buildGlob           = PROJECT_ROOT @@ "build/**/*.fsproj"

let foldExcludeGlobs (g: IGlobbingPattern) (d: string) = g -- d
let foldIncludeGlobs (g: IGlobbingPattern) (d: string) = g ++ d

let fsSrcAndTest =
    !! fsSrcGlob
    ++ fsTestGlob
    -- (PROJECT_ROOT  @@ "src/**/obj/**")
    -- (PROJECT_ROOT  @@ "tests/**/obj/**")
    -- (PROJECT_ROOT  @@ "src/**/AssemblyInfo.*")
    -- (PROJECT_ROOT  @@ "src/**/**/AssemblyInfo.*")

let fsRelaxedNameLinting =
    let baseGlob s =
        !! s
        -- (PROJECT_ROOT  @@ "src/**/AssemblyInfo.*")
        -- (PROJECT_ROOT  @@ "src/**/obj/**")
        -- (PROJECT_ROOT  @@ "tests/**/obj/**")
    match relaxedNameLinting with
    | [h] when relaxedNameLinting.Length = 1 -> baseGlob h |> Some
    | h::t -> List.fold foldIncludeGlobs (baseGlob h) t |> Some
    | _ -> None

let setCmd f args =
    match Environment.isWindows with
    | true -> Command.RawCommand(f, Arguments.OfArgs args)
    | false -> Command.RawCommand("mono", Arguments.OfArgs (f::args))

let configuration() =
    FakeVar.getOrDefault "configuration" "Release"

let getEnvFromAllOrNone (s: string) =
    let envOpt (envVar: string) =
        if String.isNullOrEmpty envVar then None
        else Some(envVar)

    let procVar = Environment.GetEnvironmentVariable(s) |> envOpt
    let userVar = Environment.GetEnvironmentVariable(s, EnvironmentVariableTarget.User) |> envOpt
    let machVar = Environment.GetEnvironmentVariable(s, EnvironmentVariableTarget.Machine) |> envOpt

    match procVar,userVar,machVar with
    | Some(v), _, _
    | _, Some(v), _
    | _, _, Some(v)
        -> Some(v)
    | _ -> None

// --------------------------------------------------------------------------------------
// Generate assembly info files with the right version & up-to-date information

let generateAssemblyInfo() =
    let getAssemblyInfoAttributes projectName =
        [ AssemblyInfo.Title (projectName)
          AssemblyInfo.Product project
          AssemblyInfo.Description summary
          AssemblyInfo.Version release.AssemblyVersion
          AssemblyInfo.FileVersion release.AssemblyVersion
          AssemblyInfo.Configuration <| configuration()
          AssemblyInfo.InternalsVisibleTo (sprintf "%s.Tests" projectName) ]

    let getProjectDetails (projectPath : string) =
        let projectName = Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    !! srcGlob
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, _, folderName, attributes) ->
        match projFileName with
        | Fsproj -> AssemblyInfoFile.createFSharp (folderName </> "AssemblyInfo.fs") attributes
        | Csproj -> AssemblyInfoFile.createCSharp ((folderName </> "Properties") </> "AssemblyInfo.cs") attributes
        | Vbproj -> AssemblyInfoFile.createVisualBasic ((folderName </> "My Project") </> "AssemblyInfo.vb") attributes
        | Shproj -> () )

// --------------------------------------------------------------------------------------
// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the
// src folder to support multiple project outputs

let copyBinaries() =
    !! libGlob
    -- genGlob
    -- (PROJECT_ROOT @@ "src/**/*.shproj")
    |> Seq.map (fun f -> ((Path.getDirectory f) @@ "bin" @@ configuration(), "bin" @@ (Path.GetFileNameWithoutExtension f)))
    |> Seq.iter (fun (fromDir, toDir) -> Shell.copyDir toDir fromDir (fun _ -> true))

// --------------------------------------------------------------------------------------
// Clean tasks

let clean() =
    let clean() =
        !! (PROJECT_ROOT  @@ "tests/**/bin")
        ++ (PROJECT_ROOT  @@ "tests/**/obj")
        ++ (PROJECT_ROOT  @@ "tools/bin")
        ++ (PROJECT_ROOT  @@ "tools/obj")
        ++ (PROJECT_ROOT  @@ "src/**/bin")
        ++ (PROJECT_ROOT  @@ "src/**/obj")
        |> Seq.toList
        |> List.append [bin; temp; objFolder]
        |> Shell.cleanDirs
    TaskRunner.runWithRetries clean 10

let cleanDocs() =
    let clean() =
        !! (pub @@ "*.md")
        ++ (pub @@ "*bundle.*")
        ++ (pub @@ "**/README.md")
        ++ (pub @@ "**/RELEASE_NOTES.md")
        ++ (pub @@ "index.html")
        |> List.ofSeq
        |> List.iter Shell.rm

    TaskRunner.runWithRetries clean 10

let copyDocFiles() =
    [ pub @@ "Recoil/README.md", PROJECT_ROOT @@ "README.md"
      pub @@ "Recoil/RELEASE_NOTES.md", PROJECT_ROOT @@ "RELEASE_NOTES.md"
      pub @@ "index.html", PROJECT_ROOT @@ "docs/index.html" ]
    |> List.iter (fun (target, source) -> Shell.copyFile target source)


let postBuildClean() =
    let clean() =
        !! srcGlob
        -- (PROJECT_ROOT @@ "src/**/*.shproj")
        |> Seq.map (
            (fun f -> (Path.getDirectory f) @@ "bin" @@ configuration()) 
            >> (fun f -> Directory.EnumerateDirectories(f) |> Seq.toList )
            >> (fun fL -> fL |> List.map (fun f -> Directory.EnumerateDirectories(f) |> Seq.toList)))
        |> (Seq.concat >> Seq.concat)
        |> Seq.iter Directory.delete
    TaskRunner.runWithRetries clean 10

let postPublishClean() = 
    let clean() =
        !! (PROJECT_ROOT @@ "src/**/bin" @@ configuration() @@ "/**/publish")
        |> Seq.iter Directory.delete
    TaskRunner.runWithRetries clean 10

// --------------------------------------------------------------------------------------
// Restore tasks

let restoreSolution () =
    solutionFile
    |> DotNet.restore id

let restore() =
    TaskRunner.runWithRetries restoreSolution 5

let yarnInstall() = 
    let setParams (defaults:Yarn.YarnParams) =
        { defaults with
            Yarn.YarnParams.YarnFilePath = (PROJECT_ROOT @@ "packages/tooling/Yarnpkg.Yarn/content/bin/yarn.cmd")
        }
    Yarn.install setParams

// --------------------------------------------------------------------------------------
// Build tasks

let getBuildConfiguration () =
    let defaultVal = configuration()

    match Environment.environVarOrDefault "CONFIGURATION" defaultVal with
    | "Debug" -> DotNet.BuildConfiguration.Debug
    | "Release" -> DotNet.BuildConfiguration.Release
    | config -> DotNet.BuildConfiguration.Custom config

/// So we don't require always being on the latest MSBuild.StructuredLogger
let disableBinLog (p: MSBuild.CliArguments) = { p with DisableInternalBinLog = true }

let build() =
    let args = [ "--no-restore" ]

    DotNet.build
        (fun c -> {
            c with
                MSBuildParams = disableBinLog c.MSBuildParams
                Configuration = getBuildConfiguration ()
                Common =
                    c.Common
                    |> DotNet.Options.withAdditionalArgs args

        })
        solutionFile

// --------------------------------------------------------------------------------------
// Publish net core applications

let publishDotNet() = 
    let runPublish (project: string) (framework: string) =
        printfn "Publishing %s with framework %s" project framework
        DotNet.publish
            (fun publishOptions ->
                { publishOptions with
                    Framework = Some framework
                    Configuration = getBuildConfiguration()
                    Common = 
                        { publishOptions.Common with
                            Version = release.AssemblyVersion |> Some
                        }
                }
            ) project

    !! libGlob
    -- genGlob
    |> Seq.map
        ((fun f -> (((Path.getDirectory f) @@ "bin" @@ configuration()), f) )
        >>
        (fun f ->
            Directory.EnumerateDirectories(fst f) 
            |> Seq.filter (fun frFolder -> frFolder.Contains("net7"))
            |> Seq.map (fun frFolder -> DirectoryInfo(frFolder).Name), snd f))
    |> Seq.iter (fun (l,p) -> l |> Seq.iter (runPublish p))

// --------------------------------------------------------------------------------------
// Lint and format source code to ensure consistency

let format() = ()
    //  let config =
    //      { FormatConfig.Default with
    //          PageWidth = 120
    //          SpaceBeforeColon = false }
 
    //  fsSrcAndTest
    //  |> (fun src -> List.fold foldExcludeGlobs src excludeFantomas)
    //  |> List.ofSeq
    //  |> formatCode config
    //  |> Async.RunSynchronously
    //  |> printfn "Formatted files: %A"

let lint() = ()
    // fsSrcAndTest
    // -- (PROJECT_ROOT  @@ "src/**/AssemblyInfo.*")
    // |> (fun src -> List.fold foldExcludeGlobs src relaxedNameLinting)
    // |> (fun fGlob ->
    //     match fsRelaxedNameLinting with
    //     | Some(glob) ->
    //         [(false, fGlob); (true, glob)]
    //     | None -> [(false, fGlob)])
    // |> Seq.map (fun (b,glob) -> (b,glob |> List.ofSeq))
    // |> List.ofSeq
    // |> FSharpLinter.lintFiles

// --------------------------------------------------------------------------------------
// Run the unit test binaries

let runTests() = ()
    // !! ("tests/**/bin" @@ configuration() @@ "**" @@ "*Tests.exe")
    // |> Seq.iter (fun f ->
    //     CreateProcess.fromCommand(setCmd f [])
    //     |> CreateProcess.withTimeout (TimeSpan.MaxValue)
    //     |> CreateProcess.ensureExitCodeWithMessage "Tests failed."
    //     |> Proc.run
    //     |> ignore)

// --------------------------------------------------------------------------------------
// Generate Paket load scripts
let loadScripts() =
    let frameworks =
        PROJECT_ROOT @@ "bin"
        |> Directory.EnumerateDirectories
        |> Seq.map (fun d ->
            Directory.EnumerateDirectories d
            |> Seq.map (fun f -> DirectoryInfo(f).Name)
            |> List.ofSeq)
        |> List.ofSeq
        |> List.reduce List.append
        |> List.distinct
        |> List.reduce (fun acc elem -> sprintf "%s --framework %s" elem acc)
        |> function
        | e when e.Length > 0 ->
            Some (sprintf "--framework %s" e)
        | _ -> None

    let arguments =
        [Some("generate-load-scripts"); frameworks]
        |> List.choose id
        |> List.reduce (fun acc elem -> sprintf "%s %s" acc elem)

    arguments
    |> CreateProcess.fromRawCommandLine ((PROJECT_ROOT @@ ".paket") @@ "paket.exe")
    |> CreateProcess.withTimeout (TimeSpan.MaxValue)
    |> CreateProcess.ensureExitCodeWithMessage "Failed to generate paket load scripts."
    |> Proc.run
    |> ignore

// --------------------------------------------------------------------------------------
// Update package.json version & name    

let packageJson() =
    let setValues (current: Json.JsonPackage) =
        { current with
            Name = Str.toKebabCase project |> Some
            Version = release.NugetVersion |> Some
            Description = summary |> Some
            Homepage = repo |> Some
            Repository = 
                { Json.RepositoryValue.Type = "git" |> Some
                  Json.RepositoryValue.Url = repo |> Some
                  Json.RepositoryValue.Directory = None }
                |> Some
            Bugs = 
                { Json.BugsValue.Url = 
                    @"https://github.com/Shmew/Feliz.Recoil/issues/new/choose" |> Some } |> Some
            License = "MIT" |> Some
            Author = author |> Some
            Private = true |> Some }
    
    Json.setJsonPkg setValues

// --------------------------------------------------------------------------------------
// Documentation targets

let killProcess() =
    Process.killAllByName "node.exe"
    Process.killAllByName "Node.js"

let start() =
    let buildApp = async { Yarn.exec "start" id }
    let launchBrowser =
        let url = "http://localhost:8080"
        async {
            do! Async.Sleep 15000
            try
                if Environment.isLinux then
                    Shell.Exec(
                        sprintf "URL=\"%s\"; xdg-open $URL ||\
                            sensible-browser $URL || x-www-browser $URL || gnome-open $URL" url)
                    |> ignore
                else Shell.Exec("open", args = url) |> ignore
            with _ -> failwith "Opening browser failed."
        }

    Target.activateFinal "KillProcess"

    [ buildApp; launchBrowser ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

let demoRaw() =
    Yarn.exec "compile-demo-raw" id

let publishPages() = 
    Yarn.exec "publish-docs" id

// --------------------------------------------------------------------------------------
// Build and release NuGet targets

let nuget() =
    Paket.pack(fun p ->
        { p with
            OutputPath = bin
            Version = release.NugetVersion
            ReleaseNotes = Fake.Core.String.toLines release.Notes
            ProjectUrl = repo
            MinimumFromLockFile = true
            IncludeReferencedProjects = true })

let nugetPublish() =
    Paket.push(fun p ->
        { p with
            ApiKey = 
                match getEnvFromAllOrNone "NUGET_KEY" with
                | Some key -> key
                | None -> failwith "The NuGet API key must be set in a NUGET_KEY environment variable"
            WorkingDir = bin })

// --------------------------------------------------------------------------------------
// Release Scripts

let doGitPush msg =
    Git.Staging.stageAll ""
    Git.Commit.exec "" msg
    Git.Branches.push ""

let gitPush p = 
    p.Context.Arguments
    |> List.choose (fun s ->
        match s.StartsWith("--Msg=") with
        | true -> Some(s.Substring 6)
        | false -> None)
    |> List.tryHead
    |> function
    | Some(s) -> s
    | None -> (sprintf "Bump version to %s" release.NugetVersion)
    |> doGitPush

let gitTag() =
    Git.Branches.tag "" release.NugetVersion
    Git.Branches.pushTag "" "origin" release.NugetVersion

let publishDocs() =
    doGitPush "Publishing docs"

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build -t <Target>' to override

let initTargets() =
    // Set default
    FakeVar.set "configuration" "Release"

    // --------------------------------------------------------------------------------------
    // Set configuration mode based on target

    Target.create "ConfigDebug" <| fun _ ->
        FakeVar.set "configuration" "Debug"

    Target.create "ConfigRelease" <| fun _ ->
        FakeVar.set "configuration" "Release"

    Target.create "AssemblyInfo"  (fun _ -> generateAssemblyInfo())
    Target.create "CopyBinaries" (fun _ -> copyBinaries())
    Target.create "Clean" (fun _ -> clean())
    Target.create "CleanDocs" (fun _ -> cleanDocs())
    Target.create "CopyDocFiles" (fun _ -> copyDocFiles())
    Target.create "PrepDocs" (fun _ -> ())
    Target.create "PostBuildClean" (fun _ -> postBuildClean())
    Target.create "PostPublishClean" (fun _ -> postPublishClean())
    Target.create "Restore" (fun _ -> restore())
    Target.create "YarnInstall" (fun _ -> yarnInstall())
    Target.create "Build" (fun _ -> build())
    Target.create "PublishDotNet" (fun _ -> publishDotNet())
    Target.create "Format" (fun _ -> format())
    Target.create "Lint" (fun _ -> lint())
    Target.create "RunTests" (fun _ -> runTests())
    Target.create "LoadScripts" (fun _ -> loadScripts())
    Target.create "PackageJson" (fun _ -> packageJson())
    Target.createFinal "KillProcess" (fun _ -> killProcess())
    Target.create "Start" (fun _ -> start())
    Target.create "DemoRaw" (fun _ -> demoRaw())
    Target.create "PublishPages" (fun _ -> publishPages())
    Target.create "NuGet" (fun _ -> nuget())
    Target.create "NuGetPublish" (fun _ -> nugetPublish())
    Target.create "GitPush" gitPush
    Target.create "GitTag" (fun _ -> gitTag())
    Target.create "PublishDocs" (fun _ -> publishDocs())

    Target.create "All" ignore
    Target.create "Dev" ignore
    Target.create "Release" ignore
    Target.create "Publish" ignore

    "Clean"
    ==> "AssemblyInfo"
    ==> "Restore"
    ==> "PackageJson"
    ==> "YarnInstall"
    ==> "Build"
    ==> "PostBuildClean" 
    ==> "CopyBinaries"
    |> ignore

    "Build" ==> "RunTests" |> ignore

    "Build"
    ==> "PostBuildClean"
    ==> "PublishDotNet"
    ==> "PostPublishClean"
    ==> "CopyBinaries"
    |> ignore

    "Restore" ==> "Lint" |> ignore
    "Restore" ==> "Format" |> ignore

    "Format"
    ?=> "Lint"
    ?=> "Build"
    ?=> "RunTests"
    ?=> "CleanDocs"
    |> ignore

    "Restore" ==> "LoadScripts" |> ignore

    "All"
    ==> "GitPush"
    ?=> "GitTag"
    |> ignore

    "All" <== ["Lint"; "RunTests"; "CopyBinaries" ]

    "CleanDocs"
    ==> "CopyDocFiles"
    ==> "PrepDocs"
    |> ignore

    "All"
    ==> "NuGet"
    ==> "NuGetPublish"
    |> ignore

    "PrepDocs" 
    ==> "PublishPages"
    ==> "PublishDocs"
    |> ignore

    "All" 
    ==> "PrepDocs"
    ==> "DemoRaw"
    |> ignore

    "All" 
    ==> "PrepDocs"
    ==> "Start"
    |> ignore

    "All" ==> "PublishPages" |> ignore

    "ConfigDebug" ?=> "Clean" |> ignore
    "ConfigRelease" ?=> "Clean" |> ignore

    "Dev" <== ["All"; "ConfigDebug"; "Start"]

    "Release" <== ["All"; "NuGet"; "ConfigRelease"]

    "Publish" <== ["Release"; "ConfigRelease"; "NuGetPublish"; "PublishDocs"; "GitTag"; "GitPush" ]


[<EntryPoint>]
let main argv =
    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    initTargets()
    Target.runOrDefaultWithArguments "Dev"

    0