module Reflection

open System
open System.Reflection
open FSharp.Reflection
open Microsoft.FSharp.Quotations

let nameof (q:Expr<_>) = 
    match q with 
    | Patterns.Let(_, _, DerivedPatterns.Lambdas(_, Patterns.Call(_, mi, _))) -> mi.Name
    | Patterns.PropertyGet(_, mi, _) -> mi.Name
    | DerivedPatterns.Lambdas(_, Patterns.Call(_, mi, _)) -> mi.Name
    | _ -> failwith "Unexpected format"

let isTypeSimple (t:Type) =
    match t with
    | _ when t = typedefof<string> -> true
    | _ when t.IsSubclassOf(typeof<System.ValueType>) -> true
    | _ -> false

let isSimple (o:obj) =
    let t = o.GetType()
    match o with
    | :? string -> true
    | _ when t.IsSubclassOf(typeof<System.ValueType>) -> true
    | _ -> false

let (|IsSimple|_|) (o: obj) = if(isSimple o) then Some o else None
let (|IsComplex|_|) (o: obj) = if(isSimple o |> not) then Some o else None

let private asThisEnumerable<'T> (candidate : obj) =
    let t = candidate.GetType()
    if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<'T>
    then Some (candidate :?> System.Collections.IEnumerable)
    else None

let (|IsList|_|) (o : obj) = asThisEnumerable<list<_>> o

let (|IsArray|_|) (o : obj) = asThisEnumerable<array<_>> o

let (|IsResizeArray|_|) (o : obj) = asThisEnumerable<ResizeArray<_>> o

let (|IsEnumerable|_|) (o : obj) = 
    let t = o.GetType()
    if(t.GetInterfaces() |> Array.contains typeof<System.Collections.IEnumerable>)
    then Some o
    else None

let (|IsSeq|_|) (o : obj) = 
    match o with
    | IsList _ -> None
    | IsArray _ -> None
    | IsResizeArray _ -> None
    | IsEnumerable _ -> 
        let t = o.GetType()
        if(t.GetInterfaces() |> Array.contains typeof<System.Collections.IEnumerable>)
        then Some o
        else None
    | _ -> None

let rec toMap (o:obj) =
    let getValOfProperty (oo:obj) (pi:PropertyInfo) = 
        let (k,v) = pi.GetValue(oo, null) |> (fun x -> pi.Name, x)
        match v with
        | IsSimple _ -> (k,v)
        | _ -> (k,(toMap v))
    
    let seqToMap (xs : seq<obj>) : Map<string,obj> =
        let ls = xs |> List.ofSeq
        match ls with
        | [] -> Map.empty
        | h::_ ->   if(h.GetType() |> isTypeSimple) 
                    then ls |> List.mapi (fun i x -> (i |> string),(x)) |> Map.ofList
                    else ls |> List.mapi (fun i x -> (i |> string),(toMap x)) |> Map.ofList

    match o with
    | IsEnumerable xs -> xs |> unbox<seq<obj>> |> seqToMap |> box
    | _ ->
        let pis = o.GetType().GetProperties(BindingFlags.Public ||| BindingFlags.Instance) |> Seq.toList          
        let m = pis |> List.map (getValOfProperty o) |> Map.ofList
        m :> obj

let objToMap (o:obj) : Map<string,obj> =
    if(isNull o || isSimple o) then Map.empty
    else (toMap o :?> Map<string,obj>)