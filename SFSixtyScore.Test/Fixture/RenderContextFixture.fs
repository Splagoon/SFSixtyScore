namespace SFSixtyScore.Test

open System
open SFSixtyScore.OCR

type RenderContextFixture() =
  member __.RenderContext = new RenderContext()

  interface IDisposable with
    member this.Dispose() =
      (this.RenderContext :> IDisposable).Dispose()
