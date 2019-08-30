module SiteTests

open System
open Xunit
open Swensen.Unquote
open Frankenstein
open System.IO

let testSiteName() = Guid.NewGuid().ToString()

//let generateSite path =
//    let getConfig() : (unit -> Configuration) = Site.defaultConfiguration path
//    path
//    |> Site.discover FS.Defaults.directoryFilter FS.Defaults.fileFilter
//    |> Site.group getConfig
//    |> Site.collect
//    |> Site.render
//    |> Site.generate
//    |> ignore

//[<Fact>]
//let ``Generating site create index html in _site`` () =
//    let path = testSiteName()
//    use dis = SiteSubject.tempDir path
//    SiteSubject.exampleSite path

//    generateSite path

//    let file = Path.Combine(path, "_site", "index.html")
//    test <@ File.Exists(file) @>

//[<Fact>]
//let ``index file contains title`` () =
//    let path = testSiteName()
//    use dis = SiteSubject.tempDir path
//    SiteSubject.exampleSite path

//    generateSite path

//    let file = Path.Combine(path, "_site", "index.html")
//    let text = FS.loadFile file |> Option.defaultValue ""
//    test <@ text.Contains("title") = true @>

//[<Fact>]
//let ``index file contains template html`` () =
//    let path = testSiteName()
//    use dis = SiteSubject.tempDir path
//    SiteSubject.exampleSite path

//    generateSite path

//    let file = Path.Combine(path, "_site", "index.html")
//    let text = FS.loadFile file |> Option.defaultValue ""
//    test <@ text.Contains("html") = true @>