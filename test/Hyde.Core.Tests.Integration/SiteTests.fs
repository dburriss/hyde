module SiteTests

open Xunit
open Swensen.Unquote
open Frankenstein
open System.IO

let generateSite path =
    path
    |> Site.discover
    |> Site.collect
    |> Site.group
    |> Site.render
    |> Site.generate
    |> ignore

[<Fact>]
let ``Generating site create index html in _site`` () =
    let path = "TestSite"
    use dis = SiteSubject.tempDir path
    SiteSubject.exampleSite path

    generateSite path

    let file = Path.Combine(path, "_site", "index.html")
    test <@ File.Exists(file) @>

[<Fact>]
let ``index file contains title`` () =
    let path = "TestSite"
    use dis = SiteSubject.tempDir path
    SiteSubject.exampleSite path

    generateSite path

    let file = Path.Combine(path, "_site", "index.html")
    let text = FS.loadFile file |> Option.defaultValue ""
    test <@ text.Contains("title") = true @>