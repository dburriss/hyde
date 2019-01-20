module YamlTests

open Xunit
open Swensen.Unquote

let containsKey k (m:Map<string,obj>) = test <@ m |> Map.containsKey k @>

let sItemIs k (v:string) (m:Map<string,obj>) = 
    containsKey k m
    test <@ (m.[k] :?> string) = v  @> 
    ignore()

let iItemIs k (v:int) (m:Map<string,obj>) = 
    containsKey k m
    test <@ (m.[k] :?> string |> int) = v  @> 
    ignore()

let itemIsList k (v:string list) (m:Map<string,obj>) = 
    containsKey k m
    test <@ (m.[k] :?> obj seq |> Seq.toList |> List.map string) = v  @> 
    ignore()


[<Fact>]
let ``deserialize string to map populates map`` () =
    let text = @"
        key: a value
        "
    text |> Yaml.toMap |> containsKey "key"

[<Fact>]
let ``deserialize string to map sets correct value`` () =
    let text = @"
        key: a value
        "
    text |> Yaml.toMap |> sItemIs "key" "a value"

[<Fact>]
let ``deserialize int to map sets correct value`` () =
    let text = @"
        key: 1
        "
    text |> Yaml.toMap |> iItemIs "key" 1

[<Fact>]
let ``deserialize array to map sets correct value`` () =
    let text = @"
        key: [text1,text2]
        "
    text |> Yaml.toMap |> itemIsList "key" ["text1";"text2"]