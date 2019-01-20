module FSTests

open Xunit
open Swensen.Unquote
open System.IO
open FS
open System

let allDirs (_:DirectoryInfo) = true
let allFiles (_:FileInfo) = true

let tempDir path =
    Directory.CreateDirectory(path) |> ignore
    { 
        new IDisposable with
            member this.Dispose() = if(Directory.Exists(path)) then Directory.Delete(path,true) else ()
    }

[<Fact>]
let ``dirExists for non-existent dir`` () =
    let path = "i-do-not-exist"
    test  <@ (FS.dirExists path) = false @>

[<Fact>]
let ``dirExists for existing dir`` () =
    let path = "dir-with-no-files"
    use dis = tempDir path
    test  <@ (FS.dirExists path) = true @>

[<Fact>]
let ``Empty directory contains no files`` () =
    let path = "dir-with-no-files"
    use dis = tempDir path
    let fsItem = FS.FSItem.init path allDirs allFiles
    let dirEmpty = fsItem |> FSItem.isEmpty
    test  <@ dirEmpty = true @>

[<Fact>]
let ``FS on example site is not empty`` () =
    let path = "TestSite"
    use dis = tempDir path
    SiteSubject.standard path
    let fsItem = FS.FSItem.init path allDirs allFiles
    let hasContent = fsItem |> FSItem.hasContent
    test  <@ hasContent = true @>