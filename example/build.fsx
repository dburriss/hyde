#load "Hyde.fsx"

open Hyde

[<CLIMutable>]
type Config = {
    title:string
}

let defaults = {
    title = "A blog"
}

let tee f x =
    f x
    x

// load config
let loadConfig path =
    FS.loadFile "_config.yaml"
    |> fun c -> match c with
                | Some x -> Yaml.deserialize x
                | None -> defaults

// render template
loadConfig "_config.yaml"
|> Liquid.init (Liquid.Defaults.populateCtx "site")
|> Liquid.addString "test" "tval"
|> Liquid.renderContent Liquid.Defaults.errorHandler "<h1> {{ site.title }} </h1> <p> {{ test }} </p>"
|> printfn "%s"

//font matter
let ti = FS.loadFile "index.html"
            |> Option.map Template.parse
            |> Option.map (tee (printfn "Template item: %A"))

// FS
let fs = FS.discover FS.Defaults.directoryFilter FS.Defaults.fileFilter __SOURCE_DIRECTORY__
printfn "%A" fs

//layout
let layout = ti |> Option.map (fun t -> FrontMatter.layout t.FrontMatter) |> Option.flatten
