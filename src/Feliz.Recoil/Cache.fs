namespace Feliz.Recoil

[<RequireQualifiedAccess>]
module Cache =
    let cast (cache: #CacheImplementation<'T,'U>) = cache :> CacheImplementation<'T,'U>

    type Bypass<'T,'U> () =
        interface CacheImplementation<'T,'U> with
            member _.get _ = None
            member this.set _ _ = this :> CacheImplementation<'T,'U>
