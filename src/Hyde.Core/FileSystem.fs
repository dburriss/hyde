module FileSystem

open System.IO

type FSItem =
| D of DItem
| F of FItem
and DItem = DirectoryInfo * FSItem list
and FItem = FileInfo

let dirExists path = Directory.Exists(path)
let fileExists path = File.Exists(path)
let directoryName path = if(dirExists path) then Some(Path.GetDirectoryName(path)) else None
let fileContent path = if(fileExists path) then Some(File.ReadAllText(path)) else None
let fileName path = if(fileExists path) then Some(Path.GetFileName(path)) else None
let fileExt path = if(fileExists path) then Some(Path.GetExtension(path)) else None
let fileNameWithoutExt path = if(fileExists path) then Some(Path.GetFileNameWithoutExtension(path)) else None
let loadFile path = path |> fileContent
let writeFile path content = File.WriteAllText(path, content)
let private directory path = if(dirExists path) then Some(new DirectoryInfo(path)) else None

        
module FSItem =                        
    let private dirs filter (d:DirectoryInfo) = 
        d.GetDirectories() 
        |> Seq.filter filter
        |> List.ofSeq

    let private files filter (d:DirectoryInfo) = 
        d.GetFiles() 
        |> Seq.filter filter
        |> List.ofSeq

    let private subDirs (f:string->(DirectoryInfo->bool)->(FileInfo->bool)->FSItem) (dirFilter:DirectoryInfo->bool) (fileFilter:FileInfo->bool) (d:DirectoryInfo)  = 
        let ds = d |> dirs dirFilter 
        ds |> List.map (fun sd -> (f (sd.ToString()) dirFilter fileFilter))
    let rec init path dirFilter fileFilter = 
        let construct (di:DirectoryInfo) =
            let sDirs = di |> subDirs init dirFilter fileFilter
            let files = di |> files fileFilter |> List.map (fun f -> f |> FSItem.F)
            let items = List.append sDirs files
            DItem (di, items) |> FSItem.D

        if(fileExists path) then 
            printfn "FILE: %s" path
            (new FileInfo(path)) |> FSItem.F
        else 
            printfn "DIR: %s" path
            let dir = directory path
            match dir with
            | Some d -> d |> construct
            | None -> failwith (sprintf "%s does not exist" path)                
   
let discover firFilter fileFilter path = 
    FSItem.init path firFilter fileFilter
let isDirectory fi =    match fi with
                        | FSItem.D _ -> true
                        | FSItem.F _ -> false

let isFile fi =    match fi with
                        | FSItem.D _ -> false
                        | FSItem.F _ -> true                            

module Defaults =
    let directoryFilter (d:DirectoryInfo) = 
        d.Name.StartsWith("_") |> not
        && d.Name.StartsWith(".") |> not
        && not (["packages";"paket-files";"bin"] |> List.contains (d.Name.ToLower()))

    let fileFilter (d:FileInfo) = 
        d.Name.StartsWith("_") |> not
        && not (["build.fsx";"hyde.fsx"] |> List.exists (fun f -> f = d.Name.ToLower()))   
        && not (d.Name.ToLower().StartsWith("paket"))