namespace Frankenstein

type FrontMatter = Map<string,obj>
type ContentItem = {
    FrontMatter:FrontMatter option
    ContentText:string
}

type SupportItem = {
    Src:string
    FrontMatter:FrontMatter
    ContentText:string
}

type TemplateItem = {
    Src:string
    Target:string
    FrontMatter:FrontMatter
    ContentText:string
}

// need location of file, frontmatter
type Layout = | Layout of SupportItem
type Include = | Include of SupportItem
type Page = | Page of TemplateItem
type Post = | Post of TemplateItem

type SiteFiles = {
    Site: Map<string,obj>
    Includes : Map<string,Include>
    Layouts : Map<string,Layout>
    Pages : Page list
    Posts : Post list
    Assets : string list
}

module FrontMatter = 
    let private emptyMap() : Map<string,obj> = Map.empty
    let empty : FrontMatter = emptyMap()
    let layout (fm:FrontMatter) = fm |> Map.tryFind "layout"  |> Option.map string
    let title (fm:FrontMatter) = fm |> Map.tryFind "title" |> Option.map string