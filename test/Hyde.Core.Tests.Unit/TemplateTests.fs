module TemplateTests

open Xunit
open Swensen.Unquote
open Frankenstein

[<Fact>]
let ``Split content without FrontMatter`` () =
    let text = "Some text"
    let result = Template.parseText text
    test <@ result.FrontMatter = None @>
    test <@ result.RawContent = text @>

[<Fact>]
let ``Split content with FrontMatter`` () =
    let text = "
    ---
    Key: a value
    ---
    Some text"
    let result = Template.parseText text
    let expectedFM : FrontMatter = [("Key",box "a value")] |> Map.ofList
    let expectedText = "Some text"
    test <@ result.FrontMatter = Some expectedFM @>
    test <@ result.RawContent = expectedText @>

[<Fact>]
let ``Split content with FrontMatter and -- in content`` () =
    let text = "
    ---
    Key: a value
    ---
    Some --- text"
    let result = Template.parseText text
    let expectedFM : FrontMatter = [("Key",box "a value")] |> Map.ofList
    let expectedText = "Some --- text"
    test <@ result.FrontMatter = Some expectedFM @>
    test <@ result.RawContent = expectedText @>