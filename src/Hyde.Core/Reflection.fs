module Reflection

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