module DiscoveryTests

open System
open Xunit
open Swensen.Unquote
open Frankenstein
open System.IO
open FS

let testSiteName() = Guid.NewGuid().ToString()

let isTrue b = b = true

[<Fact>]
let ``discover contains items``() =
    let path = testSiteName()
    use dis = SiteSubject.tempDir path

    SiteSubject.exampleSite path

    let discovered = Site.discover path

    let b = discovered.Root |> FSItem.hasContent
    test <@ b |> isTrue @>

[<Fact>]
let ``discover contains 3 directories``() =
    let path = testSiteName()
    use dis = SiteSubject.tempDir path

    SiteSubject.exampleSite path

    let discovered = Site.discover path
    let isDir fsitem = fsitem |> FSItem.isDirectory
    let ds = discovered.Root |> FSItem.matches isDir
    test <@ ds |> List.length |> fun x -> x = 3 @>

[<Fact>]
let ``discover contains _layouts and _includes``() =
    let path = testSiteName()
    use dis = SiteSubject.tempDir path

    SiteSubject.exampleSite path

    let discovered = Site.discover path
    let isInclude (item:FSItem) = item |> fun i -> (i |> FSItem.name |> String.equals "_includes")
    let isLayout (item:FSItem) = item |> fun i -> (i |> FSItem.name |> String.equals "_layouts")

    let b = discovered.Root |> fun r -> (FSItem.contains isInclude r && FSItem.contains isLayout r)
    test <@ b |> isTrue @>

