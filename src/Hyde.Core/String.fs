module String

let trim (s:string) = s.Trim()
let indexOf (v:string) (start:int) (s:string) = s.IndexOf(v,start)
let indexOfLast (v:string) (s:string) = s.LastIndexOf(v)
let substring (start:int) (length:int) (s:string) = s.Substring(start,length)