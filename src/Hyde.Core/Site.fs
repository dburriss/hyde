namespace Frankenstein

module Site =
    open System.IO
    open FS

    type Collection = {
        name: string
        output: bool
    }

    // See https://jekyllrb.com/docs/configuration/default/
    type DefaultConfig = {

        // where things are
        source: string
        destination: string
        collections_dir: string
        plugins_dir: string
        layouts_dir: string
        data_dir: string
        includes_dir: string
        collections: Collection list

        // reading
        includes: string list
        excludes: string list
    }

    let defaultConfigRecord = {

        source = "."
        destination = "_site"
        collections_dir = "."
        plugins_dir = "_plugins"
        layouts_dir = "_layouts"
        data_dir = "_data"
        includes_dir = "_includes"
        collections = [ { name = "posts"; output = true} ]
        includes = []
        excludes = ["packages";"paket-files";"bin"]
    }
    
    let private merge (a : Map<'a, 'b>) (b : Map<'a, 'b>) (f : 'a -> 'b * 'b -> 'b) =
        Map.fold (fun s k v ->
            match Map.tryFind k s with
            | Some v' -> Map.add k (f k (v, v')) s
            | None -> Map.add k v s) a b

    let private join defaultConfig config = merge defaultConfig config (fun k (dv,cv) -> cv)
    
    let defaultConfiguration path : Configuration =
        let defaults = defaultConfigRecord |> Reflection.objToMap
        let configFile = Path.Combine(path, "_config.yaml")
        let config = FS.loadFile configFile |> Option.map Yaml.toMap |> Option.defaultWith (fun () -> Map.empty)
        join defaults config
    
    /// <summary>Discovers all files in given path</summary>
    let discover path : Discovery = 
        let config = defaultConfiguration path
        let includes = config |> Config.includes
        let excludes = config |> Config.excludes
        let df = FS.Defaults.directoryFilter includes excludes
        let ff = FS.Defaults.fileFilter includes excludes
        let fsitem = FSItem.init df ff path
        {
            Root = fsitem
            Config = config
        }
    
    /// <summary>Groups files together by type</summary>
    let group (discovery : Discovery) : SiteFiles =
        
        let path = discovery.Root |> FSItem.path        
        {
            Root = path
            Config = discovery.Config
            Includes = Map.empty
            Layouts = Map.empty
            Collections = Map.empty
            Pages = []
            Assets = []
        }

    /// <summary>Collects the inputs together</summary>
    let collect (siteFiles : SiteFiles) : SiteSource = 
        {
            Config = siteFiles.Config
            Includes = Map.empty
            Layouts = Map.empty
            Collections = Map.empty
            Pages = []
            Assets = []
        }

    /// <summary>Render the Pages and Collections</summary>
    let render (context:SiteSource) = context

    /// <summary>Save rendered content and copy assets</summary>
    let generate (context : SiteSource) = 
        //let output = context.Config |> Config.destination
        //let sitePath = Path.Combine(context.Root, output)
        //FS.createDirStructure sitePath
        //let index = Path.Combine(sitePath,"index.html")
        //FS.writeFile index "title"
        ignore()