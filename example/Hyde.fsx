#r "packages/Antlr4.Runtime/lib/net45/Antlr4.Runtime.dll"
#r "packages/Liquid.NET/lib/net452/Liquid.NET.dll"
#r "packages/YamlDotNet/lib/net45/YamlDotNet.dll"

namespace Hyde
    type FrontMatter = Map<string,obj>
    type ContentItem = {
        FrontMatter:FrontMatter
        ContentText:string
    }

    type SupportItem = {
        Src:string
        FrontMatter:FrontMatter
        ContentText:string
    }

    type TemplateItem = {
        Src:string
        Target:string
        FrontMatter:FrontMatter
        ContentText:string
    }

    // need location of file, frontmatter
    type Layout = | Layout of SupportItem
    type Include = | Include of SupportItem
    type Page = | Page of TemplateItem
    type Post = | Post of TemplateItem

    type SiteFileContext = {
        Includes : Map<string,Include>
        Layouts : Map<string,Layout>
        Pages : Page list
        Posts : Post list
        Assets : string list
    }

    module String =
        let trim (s:string) = s.Trim()
        let indexOf (v:string) (start:int) (s:string) = s.IndexOf(v,start)
        let indexOfLast (v:string) (s:string) = s.LastIndexOf(v)
        let substring (start:int) (length:int) (s:string) = s.Substring(start,length)

    module TypeHelpers =
        open System.Reflection
        open FSharp.Reflection
        let isSimple (o:obj) =
            let t = o.GetType()
            match o with
            | :? string -> true
            | _ when t.IsSubclassOf(typeof<System.ValueType>) -> true
            | _ -> false

        let (|IsSimple|_|) (o: obj) = if(isSimple o) then Some o else None
        let (|IsComplex|_|) (o: obj) = if(isSimple o |> not) then Some o else None

        let rec toMap (o:obj) =
            let getValOfProperty (oo:obj) (pi:PropertyInfo) = 
                printfn "Getting value for %s on %A" pi.Name oo
                let (k,v) = pi.GetValue(oo, null) |> (fun x -> pi.Name, x)
                match v with
                | IsSimple _ -> (k,v)
                | _ -> (k,(toMap v))
            //let ff = FSharpValue.GetRecordFields(o)          
            let pis = o.GetType().GetProperties(BindingFlags.Public ||| BindingFlags.Instance) |> Seq.toList          
            let m = pis |> List.map (getValOfProperty o) |> Map.ofList
            m :> obj

        let objToMap (o:obj) : Map<string,obj> =
            if(isNull o || isSimple o) then Map.empty
            else (toMap o :?> Map<string,obj>)
                
            
    module FS =
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

    module Yaml =
        open YamlDotNet.Serialization
        open System.IO

        let deserialize<'a> (s:string) = 
            let reader = new StringReader(s)
            let builder = new DeserializerBuilder()
            let deserializer = builder.Build()
            deserializer.Deserialize<'a>(reader)

        let xtoMap dictionary = 
            (dictionary :> seq<_>)
            |> Seq.map (|KeyValue|)
            |> Map.ofSeq

        let toMap (s:string) : Map<string,obj> =
            s |> deserialize<System.Collections.Generic.Dictionary<string,obj>>
            |> Seq.map (|KeyValue|)
            |> Map.ofSeq     

    module Liquid =
        open System
        open System.Collections
        open System.Collections.Generic
        open Liquid.NET
        open Liquid.NET.Constants
        open Liquid.NET.Utils
        open TypeHelpers

        let asLiquidValue<'a when 'a :> ILiquidValue> (o:'a) = o :> ILiquidValue
        let asLiquidOption<'a when 'a :> ILiquidValue> (o:'a) = Option.Create(o |> asLiquidValue)
        let intToNumber (i:int) = LiquidNumeric.Create(i)
        let longToNumber (f:int64) = LiquidNumeric.Create(f)
        let toDate (d:DateTime option) = new LiquidDate(d |> Option.toNullable)
        let toString (s:string) = LiquidString.Create(s)
        let toBoolean b = new LiquidBoolean(b)
        let toHash (mapf:'a -> ILiquidValue) (m:Map<string,obj>) = 
            let h = new LiquidHash()
            m |> Map.iter (fun k v -> h.Add(k,(v |> mapf |> asLiquidOption)))
            h
        let rec toLiquid (o:obj) =
            match o with
            | :? bool as b -> b |> toBoolean |> asLiquidValue
            | :? int as i -> i |> intToNumber |> asLiquidValue
            | :? int64 as i -> i |> longToNumber |> asLiquidValue
            | :? option<DateTime> as d -> d |> toDate |> asLiquidValue
            | :? string as s -> s |> toString |> asLiquidValue
            | :? Map<string,obj> as m -> m |> toHash toLiquid |> asLiquidValue
            | :? IEnumerable as e -> failwith "not implemented"
            | IsComplex x -> x |> TypeHelpers.objToMap |> toHash toLiquid |> asLiquidValue
            | _ -> o.ToString() |> toString |> asLiquidValue

        let addIntf (key:string) (i:int) = fun () -> key, (i |> intToNumber |> asLiquidOption)
        let addLongf (key:string) (i:int64) = fun () -> key, (i |> longToNumber |> asLiquidOption)
        let addDatef (key:string) (d:DateTime) = fun () -> key, (d |> Microsoft.FSharp.Core.option.Some |> toDate |> asLiquidOption)
        let addStringf (key:string) (s:string) = fun () -> key, (s |> toString |> asLiquidOption)
        let addBooleanf (key:string) (b:bool) = fun () -> key, (b |> toBoolean |> asLiquidOption)
        let addHashf (key:string) (h:Map<string,obj>) = fun () -> key, (h |> (toHash toLiquid) |> asLiquidOption)
        let addObjf (key:string) (o:obj) = fun () -> key, (o |> objToMap |> (toHash toLiquid) |> asLiquidOption)

        let defineVariable<'a> (addf:unit -> string*Option<ILiquidValue>) (ctx:ITemplateContext) =
            let (key, v) = addf()
            ctx.DefineLocalVariable(key, v) |> ignore
            ctx

        let addString k s (ctx:ITemplateContext) = defineVariable (addStringf k s) ctx

        let context() =
            let ctx = new TemplateContext() :> ITemplateContext 
            ctx.WithAllFilters() |> ignore
            ctx
        
        let init populate (config:obj) : ITemplateContext = 
            context() |> populate config

        let createTemplate (content:string) = LiquidTemplate.Create(content)
        let renderContent (handleErrors:string -> LiquidError list -> unit) (content:string) (ctx:ITemplateContext) =
            let template = createTemplate content
            // todo: handle errors well ie. pass forward
            if(template.HasParsingErrors) then handleErrors "Parsing" (template.ParsingErrors |> Seq.toList)
            let renderResult = template.LiquidTemplate.Render(ctx)
            if(renderResult.HasRenderingErrors) then handleErrors "Rendering" (renderResult.RenderingErrors |> Seq.toList)
            renderResult.Result

        module Defaults =
            open TypeHelpers
            let populateCtx (key:string) (o:obj) (ctx:ITemplateContext) =
                ctx |> defineVariable (addObjf key o) |> ignore
                ctx

            let errorHandler (contextDescription:string) (errs:LiquidError list) = 
                printfn "ERROR: %s" contextDescription
                errs |> List.iter   (fun e -> 
                                        printfn "Message: %s" e.Message
                                        printfn "Context: %s" e.Context
                                        printfn "Line: %i at char position %i" e.Line e.CharPositionInLine
                                        printfn "Problem with '%s' in '%s'" e.OffendingSymbol e.TokenSource
                                    )   

    module FrontMatter =
        let layout (fm:FrontMatter) = fm |> Map.tryFind "layout"  |> Option.map string
        let title (fm:FrontMatter) = fm |> Map.tryFind "title" |> Option.map string

    module Template = 

        let splitContent (s:string) =
            let shift i = if(i = -1) then i else (i + 3)
            let start = s |> String.indexOf "---" 0 
            let fin = s |> String.indexOfLast "---" //todo: what if content contains ---
            let fm = s.Substring(start |> shift, fin - (start |> shift)) |> String.trim
            let c = s.Substring(fin |> shift, (s.Length - ((fin |> shift) - start))) |> String.trim
            (fm,c)

        let parse (s:string) =
            let (fm,content) = s |> splitContent
            {
                FrontMatter = fm |> Yaml.toMap
                ContentText = content
            }     

        let render c = ""

    [<AutoOpen>]
    module Hyde =
        open FS
        let siteDir = "_site"
        let draftsDir = "_drafts"
        let includesDir = "_includes"
        let layoutsDir = "_layouts"
        let postsDir = "_posts"
        
        

        // let posts (dir:DItem) (): Post list =
        //         dir |> snd 
        //         |> List.filter FS.isFile 
        //         |> List.map(fun x -> match x with 
        //                              | FSItem.F f -> f
        //                      )
            
        //     []


        // let parse fsitem =

        // let goSeek dir = 
        //     FS.discover "*" "*" dir
        //     |> 

            
        // let make<'a> (dir:string) (config:'a) = 
              

        //     ()