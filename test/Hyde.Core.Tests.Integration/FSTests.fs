module FSTests

open Xunit
open Swensen.Unquote
open System.IO
open FS
open System

let allDirs (_:DirectoryInfo) = true
let allFiles (_:FileInfo) = true

[<Fact>]
let ``dirExists for non-existent dir`` () =
    let path = "i-do-not-exist"
    test  <@ (FS.dirExists path) = false @>

[<Fact>]
let ``dirExists for existing dir`` () =
    let path = "dir-with-no-files"
    use dis = SiteSubject.tempDir path
    test  <@ (FS.dirExists path) = true @>

[<Fact>]
let ``Empty directory contains no files`` () =
    let path = "dir-with-no-files"
    use dis = SiteSubject.tempDir path
    let fsItem = FS.FSItem.init path allDirs allFiles
    let dirEmpty = fsItem |> FSItem.isEmpty
    test  <@ dirEmpty = true @>

[<Fact>]
let ``FS on example site is not empty`` () =
    let path = "TestSite"
    use dis = SiteSubject.tempDir path
    SiteSubject.exampleSite path
    let fsItem = FS.FSItem.init path allDirs allFiles
    let hasContent = fsItem |> FSItem.hasContent
    test  <@ hasContent = true @>