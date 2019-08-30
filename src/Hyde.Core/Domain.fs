namespace Frankenstein

type FrontMatter = Map<string,obj>

type ContentItem = {
    FrontMatter:FrontMatter option
    ContentText:string
}

type Configuration = Map<string,obj>

//================================
// DISCOVER
//================================
type Discovery = {
    Root : FS.FSItem
    Config : Configuration
}
//================================
// GROUP
//================================
type PagePath = string
type LayoutPath = string
type IncludePath = string
type AssetPath = string
type DataPath = string

type SiteFiles = {
    Root: string
    Config: Configuration
    Includes : Map<string,IncludePath>
    Layouts : Map<string,LayoutPath>
    Collections : Map<string, PagePath list>
    Pages : PagePath list
    Assets : AssetPath list
}

type SupportItem = {
    FrontMatter: unit -> FrontMatter
    ContentText: unit -> string
}

type AssetItem = {
    Copy : unit -> unit
}

type TemplateItem = {
    FrontMatter: unit -> FrontMatter
    ContentText: unit -> string
    Target: string
}

//================================
// READY TO READ IN
//================================
type LayoutSrc = SupportItem
type IncludeSrc = SupportItem
type PageSrc = TemplateItem
type AssetSrc = AssetItem

type SiteSource = {
    Config: Configuration
    Includes : Map<string,IncludeSrc>
    Layouts : Map<string,LayoutSrc>
    Collections : Map<string, PageSrc list>
    Pages : PageSrc list
    Assets : AssetSrc list
}

type SiteContent = {
    Site: Configuration
    Includes : Map<string,IncludeSrc>
    Layouts : Map<string,LayoutSrc>
    Collections : Map<string, PageSrc list>
    Pages : PageSrc list
    Assets : AssetSrc list
}

module FrontMatter = 
    let emptyMap() : Map<string,obj> = Map.empty
    let empty : FrontMatter = emptyMap()
    let layout (fm:FrontMatter) = fm |> Map.tryFind "layout"  |> Option.map string
    let title (fm:FrontMatter) = fm |> Map.tryFind "title" |> Option.map string