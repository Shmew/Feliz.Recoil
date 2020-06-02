namespace Feliz.Recoil

open Fable.Core

[<AutoOpen;Erase>]
module AsyncUtils =
    type Recoil with
        /// Converts an array of RecoilValue<'T,_> into a RecoilValue<'T [],_>.
        ///
        /// Requests all dependencies in parallel and waits for all to be
        /// available before returning a value.
        static member inline waitForAll (recoilValues: RecoilValue<'T,'Mode> []) = 
            Bindings.Recoil.waitForAll(ResizeArray recoilValues) 
            |> RecoilValue.map Array.ofSeq
        /// Converts a list of RecoilValue<'T,_> into a RecoilValue<'T list,_>.
        ///
        /// Requests all dependencies in parallel and waits for all to be
        /// available before returning a value.
        static member inline waitForAll (recoilValues: RecoilValue<'T,'Mode> list) = 
            Bindings.Recoil.waitForAll(ResizeArray recoilValues) 
            |> RecoilValue.map List.ofSeq
        /// Converts a ResizeArray of RecoilValue<'T,_> into a RecoilValue<ResizeArray<'T>,_>.
        ///
        /// Requests all dependencies in parallel and waits for all to be
        /// available before returning a value.
        static member inline waitForAll (recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) = 
            Bindings.Recoil.waitForAll(ResizeArray recoilValues)

        /// Converts an array of RecoilValue<'T,_> into a RecoilValue<Loadable<'T> [],_>.
        ///
        /// Requests all dependencies in parallel and waits for at least
        /// one to be available before returning results.
        static member inline waitForAny (recoilValues: RecoilValue<'T,'Mode> []) = 
            Bindings.Recoil.waitForAny(ResizeArray recoilValues) 
            |> RecoilValue.map Array.ofSeq
        /// Converts a list of RecoilValue<'T,_> into a RecoilValue<Loadable<'T> list,_>.
        ///
        /// Requests all dependencies in parallel and waits for at least
        /// one to be available before returning results.
        static member inline waitForAny (recoilValues: RecoilValue<'T,'Mode> list) =
            Bindings.Recoil.waitForAny(ResizeArray recoilValues) 
            |> RecoilValue.map List.ofSeq
        /// Converts a ResizeArray of RecoilValue<'T,_> into a RecoilValue<ResizeArray<Loadable<'T>>,_>.
        ///
        /// Requests all dependencies in parallel and waits for at least
        /// one to be available before returning results.
        static member inline waitForAny (recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) = 
            Bindings.Recoil.waitForAny(ResizeArray recoilValues)

        /// Converts an array of RecoilValue<'T,_> into a RecoilValue<Loadable<'T> [],_>.
        ///
        /// Requests all dependencies in parallel and immediately returns
        /// current results without waiting.
        static member inline waitForNone (recoilValues: RecoilValue<'T,'Mode> []) = 
            Bindings.Recoil.waitForNone(ResizeArray recoilValues) 
            |> RecoilValue.map Array.ofSeq
        /// Converts a list of RecoilValue<'T,_> into a RecoilValue<Loadable<'T> list,_>.
        ///
        /// Requests all dependencies in parallel and immediately returns
        /// current results without waiting.
        static member inline waitForNone (recoilValues: RecoilValue<'T,'Mode> list) = 
            Bindings.Recoil.waitForNone(ResizeArray recoilValues)
            |> RecoilValue.map List.ofSeq
        /// Converts a ResizeArray of RecoilValue<'T,_> into a RecoilValue<ResizeArray<Loadable<'T>>,_>.
        ///
        /// Requests all dependencies in parallel and immediately returns
        /// current results without waiting.
        static member inline waitForNone (recoilValues: ResizeArray<RecoilValue<'T,'Mode>>) = 
            Bindings.Recoil.waitForNone(ResizeArray recoilValues)
