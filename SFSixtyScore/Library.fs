namespace SFSixtyScore

open System
open SFSixtyScore.OCR

type ScoreReader(screenX, screenY, screenWidth, screenHeight) =
  let renderContext = new RenderContext()
  let screenCapture = new ScreenCapture(screenWidth, screenHeight)

  member val ScreenX = screenX with get, set
  member val ScreenY = screenY with get, set
  member val ScreenWidth = screenWidth
  member val ScreenHeight = screenHeight

  member this.Run(onScore) =
    let rec loop lastNScores =
      Async.Sleep(1000 / 30) |> Async.RunSynchronously

      use srcTexture = screenCapture.CaptureScreen(this.ScreenX, this.ScreenY)

      let scoreReading = getScore srcTexture renderContext
      match scoreReading with
      | Score score ->
          let newLastNScores = (score :: lastNScores) |> List.truncate 5
          let numDistinctScores = newLastNScores |> List.distinct |> List.length
          if numDistinctScores = 1 then
            score |> onScore

          loop newLastNScores
      | _ -> loop lastNScores

    loop []

  interface IDisposable with
    member __.Dispose() =
      (renderContext :> IDisposable).Dispose()
      (screenCapture :> IDisposable).Dispose()
