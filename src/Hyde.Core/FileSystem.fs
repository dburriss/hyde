module FS

open System.IO

type FSItem =
| D of DItem
| F of FItem
and DItem = DirectoryInfo * FSItem list
and FItem = FileInfo

let dirExists path = Directory.Exists(path)
let createDir path = Directory.CreateDirectory path |> ignore
let createDirStructure (path:string) = 
    let paths = path.Split(Path.PathSeparator)
    let mutable dir = ""
    for d in paths do 
        dir <- Path.Combine(dir, d)
        createDir dir

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

    let private subDirs (f:(DirectoryInfo->bool)->(FileInfo->bool)->string->FSItem) (dirFilter:DirectoryInfo->bool) (fileFilter:FileInfo->bool) (d:DirectoryInfo)  = 
        let ds = d |> dirs dirFilter 
        ds |> List.map (fun sd -> (f dirFilter fileFilter (sd.ToString()) ))
    let rec init dirFilter fileFilter path = 
        let construct (di:DirectoryInfo) =
            let sDirs = di |> subDirs init dirFilter fileFilter
            let files = di |> files fileFilter |> List.map (fun f -> f |> FSItem.F)
            let items = List.append sDirs files
            DItem (di, items) |> FSItem.D

        if(fileExists path) then 
            (new FileInfo(path)) |> FSItem.F
        else 
            let dir = directory path
            match dir with
            | Some d -> d |> construct
            | None -> failwith (sprintf "%s does not exist" path)
            
    let isEmpty fsItem =
        match fsItem with
        | D (_, fs) -> fs |> List.isEmpty
        | F fi -> fi.Length = 0L 

    let hasContent fsItem = fsItem |> isEmpty |> not

    let path fsitem =
        match fsitem with
        | D (di,_) -> di |> string
        | F fi -> fi |> string

    let name fsitem =
        match fsitem with
        | D (di,_) -> di.Name
        | F fi -> fi.Name

    let startsWith value fsitem = fsitem |> (name >> String.startsWith value) 

    let collect mapping fsitems = fsitems |> List.collect mapping

    let rec private collectMatches predicate matched fsitem =
        let r = if(predicate fsitem) then matched @ [fsitem] else matched
        match fsitem with
        | D (_,fs) -> r @ (fs |> List.collect (fun i -> collectMatches predicate r i) )
        | _ -> r


    let matches predicate fsitem = collectMatches predicate [] fsitem
    let contains predicate fsitem = (matches predicate fsitem) |> List.isEmpty

    let isDirectory fi =    match fi with
                            | FSItem.D _ -> true
                            | FSItem.F _ -> false

    let isFile fi =    match fi with
                            | FSItem.D _ -> false
                            | FSItem.F _ -> true        
                            
let discover firFilter fileFilter path = 
    FSItem.init firFilter fileFilter path


module Defaults =

    
    let private doesNotStartWithUnderscore (name:string) : bool = (name.StartsWith("_") |> not)
    let private doesNotStartWithPeriod (name:string) : bool = (name.StartsWith(".") |> not)
    let private including includes name = List.contains name includes
    let private excluding excludes name = (including excludes name) |> not

    let directoryFilter includes excludes (d:DirectoryInfo) = 
        true
        //including includes d.Name
        //|| 
        //(
        //    (d.Name |> doesNotStartWithPeriod)
        //    && (d.Name |> doesNotStartWithUnderscore)
        //    && (d.Name |> excluding excludes)
        //)

    let fileFilter includes excludes (d:FileInfo) = 
        true
        //including includes d.Name
        //|| 
        //(
        //    (d.Name |> doesNotStartWithPeriod)
        //    && (d.Name |> doesNotStartWithUnderscore)
        //    && (d.Name |> excluding excludes)
        //)
