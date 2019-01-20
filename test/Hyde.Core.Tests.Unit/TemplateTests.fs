module TemplateTests

open Xunit
open Swensen.Unquote
open Hyde

[<Fact>]
let ``Split content without FrontMatter`` () =
    let text = "Some text"
    let result = Template.parseText text
    test <@ result.FrontMatter = None @>
    test <@ result.ContentText = text @>

[<Fact>]
let ``Split content without fontmatter`` () =
    let text = "
    ---
    Key: a value
    ---
    Some text"
    let result = Template.parseText text
    let expectedFM : FrontMatter = [("Key",box "a value")] |> Map.ofList
    test <@ result.FrontMatter = Some expectedFM @>
    test <@ result.ContentText = text @>