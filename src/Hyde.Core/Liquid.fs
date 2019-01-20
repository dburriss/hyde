module Liquid

open System
open System.Collections
open System.Collections.Generic
open Liquid.NET
open Liquid.NET.Constants
open Liquid.NET.Utils
open Reflection

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
    | IsComplex x -> x |> Reflection.objToMap |> toHash toLiquid |> asLiquidValue
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