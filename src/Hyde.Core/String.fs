module String

let trim (s:string) = s.Trim()
let indexOf (v:string) (start:int) (s:string) = s.IndexOf(v,start)
let indexOfLast (v:string) (s:string) = s.LastIndexOf(v)
let substring (start:int) (length:int) (s:string) = s.Substring(start,length)
let startsWith (value:string) (s:string) = s.StartsWith(value)
let equals (s1:string) (s2:string) = s1.Equals(s2)