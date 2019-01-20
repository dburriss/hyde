#r "packages/YamlDotNet/lib/net45/YamlDotNet.dll"

module Yaml =
    open YamlDotNet.RepresentationModel
    open YamlDotNet.Serialization
    open System.IO

    let private stringReader (s:string) = new StringReader(s)
    let parse (s:string) =
        let reader = stringReader s
        let yaml = new YamlStream()
        yaml.Load(reader)
        yaml

    let deserialize<'a> s = 
        let reader = stringReader s
        let builder = new DeserializerBuilder()
        let deserializer = builder.Build()
        deserializer.Deserialize<'a>(reader)

    let serialize x = 
        let builder = new SerializerBuilder()
        let serializer = builder.Build()
        serializer.Serialize(x)    

let yamlText = @"---
            receipt:    Oz-Ware Purchase Invoice
            date:        2007-08-06
            customer:
                given:   Dorothy
                family:  Gale

            items:
                - part_no:   A4786
                  descrip:   Water Bucket (Filled)
                  price:     1.47
                  quantity:  4

                - part_no:   E1628
                  descrip:   High Heeled ""Ruby"" Slippers
                  price:     100.27
                  quantity:  1

            bill-to:  &id001
                street: |
                        123 Tornado Alley
                        Suite 16
                city:   East Westville
                state:  KS

            ship-to:  *id001

            specialDelivery:  >
                Follow the Yellow Brick
                Road to the Emerald City.
                Pay no attention to the
                man behind the curtain.
..."

// low level
open YamlDotNet
open YamlDotNet.RepresentationModel
open System.Collections.Generic

let yaml = yamlText |> Yaml.parse

let p s = printf "%s" s
let printNode (n:YamlNode) = printfn "NODE NodeType:%A | Tag:%s | AllNodes length:%i" (n.NodeType) (n.Tag) (n.AllNodes |> Seq.length)
let printMappingNode (n:YamlMappingNode) = printfn "NODE NodeType:%A | Tag:%s | Style:%A | Children:%i" (n.NodeType) (n.Tag) (n.Style) (n.Children.Keys |> Seq.length)
let printChildren (children:IDictionary<YamlNode,YamlNode>) = 
    let toPair (m:IDictionary<YamlNode,YamlNode>) = m.Keys |> Seq.map (fun k -> (k, m.[k]))
    children |> toPair |> Seq.iter (fun (k,v) -> p "K:";printNode k;p "V:";printNode v)

let printScalar (n:YamlScalarNode) = printfn "NODE NodeType:%A | Tag:%s | Style:%A | Valye:%s" (n.NodeType) (n.Tag) (n.Style) (n.Value)

let yamlNodeRoot = yaml.Documents.[0].RootNode
yamlNodeRoot |> printNode
let mappingNode = yamlNodeRoot :?> YamlMappingNode
mappingNode |> printMappingNode
printChildren mappingNode.Children

let receipt = mappingNode.Children.[new YamlScalarNode("receipt")] :?> YamlScalarNode
printScalar receipt

let items = mappingNode.Children.[new YamlScalarNode("items")] :?> YamlSequenceNode;

items |> Seq.iter printNode