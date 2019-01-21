namespace Frankenstein

module Site =
    open System.IO
    
    let discover path : SiteFiles = 
        let sitePath = Path.Combine(path, "_site") |> box
        {
            Site = [("site",sitePath)] |> Map.ofList
            Includes = Map.empty
            Layouts = Map.empty
            Pages = []
            Posts = []
            Assets = []
        }

    let collect (context:SiteFiles) = context

    let group context = context

    let render context = context

    let generate context = 
        let path = (context.Site.["site"] |> string)
        FS.createDirStructure path
        let index = Path.Combine(path,"index.html")
        FS.writeFile index ""
        ignore