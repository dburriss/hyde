namespace Frankenstein

module Config =

    let destination config = config |> Map.tryFind "destination" |> Option.map string |> Option.defaultValue "_site"

    let asStringList m = 
        m 
        |> Map.toList 
        |> List.map (fun (_,v) -> v) 
        |> List.map string

    let includes (config:Configuration) = 
        config 
        |> Map.tryFind "includes" 
        |> Option.map (fun x -> x :?> Map<string,obj>) 
        |> Option.map asStringList 
        |> Option.defaultValue []
    
    let excludes (config:Configuration) = 
        config 
        |> Map.tryFind "excludes" 
        |> Option.map (fun x -> x :?> Map<string,obj>) 
        |> Option.map asStringList 
        |> Option.defaultValue []