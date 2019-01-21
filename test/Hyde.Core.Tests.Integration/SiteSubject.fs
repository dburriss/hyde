module SiteSubject

open System.IO
open System

let private indexContent() = 
    "
---
layout: page
description: \"Mostly just adventures in code.\"
paginate: 10
---
<h1>{{ site.title }}</h1>"

let private postContent title = 
    sprintf "
---
layout: post
title: \"%s\"
---
## Introduction

Content for %s" title title

let private defaultLayoutContent() = 
    "
<!DOCTYPE html>
<html lang=\"en\" itemscope itemtype=\"http://schema.org/Article\">
{% include head.html %}
<body>
    {{ content }}
</body>
</html>"

let private pageLayoutContent() = 
    "
---
layout: default
---
    <div>
        { content }
    </div>"

let private postLayoutContent() = 
    "
---
layout: default
comments: true
---
    <h1>{{ page.title }}</h1>
    {% if page.TAGS %}
        <hr />
        {% for tag in page.TAGS %}
            <a href=\"/tag/{{ tag | slugify}}\" class=\"label label-default\">{{ tag }}</a>
        {% endfor %}
    {% endif %}
    <div>
        { content }
    </div>"

let private includeHeadContent() =
    "
<head>
    <title>{{ title }}</title>
    <meta name=\"description\" content=\"{{ description }}\">
    <meta name=\"author\" content=\"{{ site.author }}\">
</head>"

let private createDir path = Directory.CreateDirectory path |> ignore
let private createDirs ds = ds |> List.iter createDir

let private createFile (path,content) = File.WriteAllText(path, content) |> ignore
let private createFiles fs = fs |> List.iter createFile 

let private createSiteDirs path = 
    createDirs [
        (Path.Combine(path,"_posts"))
        (Path.Combine(path,"_includes"))
        (Path.Combine(path,"_layouts"))
    ]

let createPages path = 
    createFiles [
        (Path.Combine(path,"index.html"), indexContent())
    ]

let createPosts path = 
    createFiles [
        (Path.Combine(path,"_posts","2019-01-19-post-1.html"), postContent("Post 1"))
        (Path.Combine(path,"_posts","2019-01-20-post-2.html"), postContent("Post 3"))
    ]

let createLayouts path = 
    createFiles [
        (Path.Combine(path, "_layouts","default.html"), defaultLayoutContent())
        (Path.Combine(path, "_layouts","page.html"), pageLayoutContent())
        (Path.Combine(path, "_layouts","post.html"), postLayoutContent())
    ]

let createIncludes path = 
    createFiles [
        (Path.Combine(path, "_includes","head.html"), includeHeadContent())
    ]

let private createSiteFiles path =
    createIncludes path
    createLayouts path
    createPages path
    createPosts path

let exampleSite path =
    createDir path
    createSiteDirs path
    createSiteFiles path

let tempDir path =
    Directory.CreateDirectory(path) |> ignore
    { 
        new IDisposable with
            member this.Dispose() = if(Directory.Exists(path)) then Directory.Delete(path,true) else ()
    }