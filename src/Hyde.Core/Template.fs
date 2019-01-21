namespace Frankenstein

module Template = 
    open System
    
    let private explainString (s:string) =
        let nl = Environment.NewLine
        let x = s.Replace("\r", "\\r").Replace("\n","\\n")
        let arrS = s.ToCharArray() |> Array.map (sprintf " %c ")
        let arrArrows = arrS |> Array.map (fun _ -> " ^ ")
        let arrPos = arrS |> Array.mapi (fun i _ -> (i |> string)) |> Array.map (fun s -> s.PadLeft(2).PadRight(3))
        sprintf @"
        %s\n
        %A\n
        %A\n
        %A" x (arrS |> String.concat "") (arrArrows |> String.concat "") (arrPos |> String.concat "")

    let splitContent (s:string) =
        let exp = explainString s
        
        let shift i = if(i = -1) then i else (i + 3)
        let start = s |> String.indexOf "---" 0 
        if(start = -1) then
            (None,s)
        else 
            let fmfin = s |> String.indexOf "---" (start |> shift)
            let fm = s.Substring(start |> shift, fmfin - (start |> shift))
            let cStart = fmfin |> shift
            let cEndLen = (s.Length - cStart)
            let c = s.Substring(cStart, cEndLen)
            (Some (fm |> String.trim),(c |> String.trim))

    let parseText (s:string) =
        let (fm,content) = s |> splitContent
        {
            FrontMatter = fm |> Option.map Yaml.toMap
            ContentText = content
        }     

    let render c = ""