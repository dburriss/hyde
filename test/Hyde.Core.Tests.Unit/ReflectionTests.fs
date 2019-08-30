module ReflectionTests

open System
open Xunit
open Swensen.Unquote
open Reflection

let containsKey k (m:Map<string,obj>) = test <@ m |> Map.containsKey k @> ; ignore()

let sItemIs k (v:string) (m:Map<string,obj>) = 
    containsKey k m
    test <@ (m.[k] :?> string) = v  @> 
    ignore()

let iItemIs k (v:int) (m:Map<string,obj>) = 
    containsKey k m
    test <@ (m.[k] :?> int) = v  @> 
    ignore()

let itemIsList k (v:string list) (m:Map<string,obj>) = 
    containsKey k m
    test <@ (m.[k] :?> obj seq |> Seq.toList |> List.map string) = v  @> 
    ignore()

let isTrue b = test <@ b = true @> ; ignore()
let isFalse b = test <@ b <> true @> ; ignore()

let trueIfSimple x = 
    match x with
    | IsSimple _ -> true
    | IsComplex _ -> false
    | _ -> failwith "Did not match IsSimple or IsComplex"

let trueIfComplex x = 
    match x with
    | IsSimple _ -> false
    | IsComplex _ -> true
    | _ -> failwith "Did not match IsSimple or IsComplex"

type TestSimple = {
    Age:int
    Name:string
}

[<Fact>]
let ``int is simple`` () = 
    1 |> Reflection.isSimple |> isTrue

[<Fact>]
let ``string is simple`` () = 
    "bob" |> Reflection.isSimple |> isTrue

[<Fact>]
let ``DateTime is simple`` () = 
    DateTime.Now |> Reflection.isSimple |> isTrue

[<Fact>]
let ``IsSimple on simple value is matched`` () =
    let x = 1
    let r = trueIfSimple x
    r |> isTrue

[<Fact>]
let ``IsSimple on complex value is not matched`` () =
    let data = { Age = 30; Name = "Bob"}
    let r = trueIfSimple data
    r |> isFalse

[<Fact>]
let ``IsComplex on simple value is not matched`` () =
    let x = 1
    let r = trueIfComplex x
    r |> isFalse

[<Fact>]
let ``IsList on an int does not match list`` () =
    let i = 1
    let islist x =
        match x with
        | IsList _ -> true
        | _ -> false
    let result = islist(i)
    test <@ result = false @>

[<Fact>]
let ``IsList on an list does match list`` () =
    let i = [1;2]
    let islist x =
        match x with
        | IsList _ -> true
        | _ -> false
    let result = islist(i)
    test <@ result = true @>

[<Fact>]
let ``IsList matches against other collections`` () =
    let i = [1;2]
    let whatType x =
        match x with
        | IsSeq _ -> "seq"
        | IsArray _ -> "array"
        | IsResizeArray _ -> "resizearray"
        | IsList _ -> "list"        
        | _ -> "unknown"
    let result = whatType(i)
    test <@ result = "list" @>

[<Fact>]
let ``IsSeq matches against other collections`` () =
    let i = seq { 1..2 }
    let whatType x =
        match x with
        | IsSeq _ -> "seq"
        | IsArray _ -> "array"
        | IsResizeArray _ -> "resizearray"
        | IsList _ -> "list"        
        | _ -> "unknown"
    let result = whatType(i)
    test <@ result = "seq" @>

[<Fact>]
let ``IsComplex on complex value is matched`` () =
    let data = { Age = 30; Name = "Bob"}
    let r = trueIfComplex data
    r |> isTrue

[<Fact>]
let ``toMap on a record contains keys of record`` () =   
    let data = { Age = 30; Name = "Bob"}
    let m = data |> Reflection.objToMap 
    m |> containsKey "Age"
    m |> containsKey "Name"

[<Fact>]
let ``toMap on a record contains values of record`` () =
    let data = { Age = 30; Name = "Bob"}
    let m = data |> Reflection.objToMap 
    m |> iItemIs "Age" data.Age
    m |> sItemIs "Name" data.Name

[<Fact>]
let ``toMap on a simple list returns map with index key`` () =
    let data = ["test0";"test1"]
    let m = data |> Reflection.objToMap 
    let v0 = m |> Map.find "0" |> string
    let v1 = m |> Map.find "1" |> string
    test <@ v0 = "test0" @>
    test <@ v1 = "test1" @>

[<Fact>]
let ``toMap on a complex list returns map with map inside`` () =
    let e1 = { Age = 30; Name = "Bob"}
    let e2 = { Age = 40; Name = "Dick"}
    let data = [e1;e2]
    let m = data |> Reflection.objToMap 
    let v0 = m |> Map.find "0" :?> Map<string,obj> |> Map.find "Name" |> string
    let v1 = m |> Map.find "1" :?> Map<string,obj> |> Map.find "Age" |> string |> int
    test <@ v0 = "Bob" @>
    test <@ v1 = 40 @>
