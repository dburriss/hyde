module Yaml

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