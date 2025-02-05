#if FAKE
#r "paket: groupref Build //"
#endif


#load ".fake/build.fsx/intellisense.fsx"




open Fake.Core
open Fake.DotNet
open Fake.Core.TargetOperators
open System

let [<Literal>] sln = "Itminus.Tags.sln";


let PROJECTS = [|
    "Itminus.Tags"
|]

let cli = """
usage: prog [options]

options:
-v <version> Version
-s <nugetsource> NugetSource
-k <apikey> ApiKey
"""
let parser = Docopt(cli)


module PackHelper =
    let makeNupkg (packageVersion: string) (proj: string) =
        let packId = $"{proj}";
        let mapPackOpt(opt: DotNet.PackOptions) = 
            let mspara =  opt.MSBuildParams 
            let props = mspara.Properties
            let newmsparam = { mspara with  Properties = ("PackageVersion",packageVersion)::props }

            {  opt with 
                Configuration = DotNet.BuildConfiguration.Release   
                MSBuildParams = newmsparam
            }

        DotNet.pack mapPackOpt  proj 
        let nupkgFilePath = $"{proj}/bin/Release/{packId}.{packageVersion}.nupkg"
        if System.IO.File.Exists nupkgFilePath |> not then
            failwith $"目标nupkg文件不存在：{nupkgFilePath}"
        nupkgFilePath

    let pushNupkg (nupkgFilePath:string) (source: string) (key: string) = 
        if System.IO.File.Exists nupkgFilePath |> not then
            failwith $"目标nupkg文件不存在：{nupkgFilePath}"
        let mapNugetPush (opt: DotNet.NuGetPushOptions) =
            let pushParam = opt.PushParams
            let newParam = { 
                    pushParam with  
                        Source = Some source
                        ApiKey = Some key
                    }
            { opt with  PushParams = newParam }
        DotNet.nugetPush mapNugetPush nupkgFilePath


Target.create "build" (fun p ->
    DotNet.build id sln
    DotNet.test id sln
)

Target.create "push" (fun p ->

    let args =p.Context.Arguments
    let parsedArguments = parser.Parse(args)

    // 从环境变量或者命令行参数中获取参数
    let getConfigFromCmdLineArgOrEnvVar cmdlineFlag envVarName  =
        DocoptResult.tryGetArgument cmdlineFlag parsedArguments
        |> Option.orElseWith (fun () -> 
            let envValue = Environment.environVar envVarName
            if String.isNotNullOrEmpty( envValue ) then
                Some envValue
            else 
                None
        )

    // Source
    let nugetSourceOpt = getConfigFromCmdLineArgOrEnvVar "-s" "NugetSource"
    // Key
    let nugetKeyOpt= getConfigFromCmdLineArgOrEnvVar "-k" "NugetApiKey"
    // PackageVersion
    let packageVersionOpt = DocoptResult.tryGetArgument "-v" parsedArguments

    match packageVersionOpt, nugetSourceOpt, nugetKeyOpt with
    | None, _, _ -> failwith "未指定版本号： -v <version>"
    | Some _ , None, _ -> failwith "未指定Nuget源： -s <source> 或者环境变量 NugetSource"
    | Some _, Some _ , None -> failwith "未指定Nuget源： -k <source> 或者环境变量 NugetApiKey"
    | Some packageVersion, Some source, Some key->
        let nupkgs =PROJECTS |> Array.map(fun proj -> PackHelper.makeNupkg packageVersion proj )
        nupkgs |> Array.iter (fun nupkg ->
            PackHelper.pushNupkg nupkg source key
        )
)


"build"
    ==> "push"

Target.runOrDefaultWithArguments "build"