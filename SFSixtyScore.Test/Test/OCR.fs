namespace SFSixtyScore.Test

open SFSixtyScore.OCR
open SFML.Graphics
open Xunit
open FsUnit.Xunit

type OCRTests(fixture : RenderContextFixture) =
  let context = fixture.RenderContext
  interface IClassFixture<RenderContextFixture>

  [<Theory>]
  [<InlineData("Asset/Test/ScreenShot_0.png",     0)>]
  [<InlineData("Asset/Test/ScreenShot_15.png",   15)>]
  [<InlineData("Asset/Test/ScreenShot_777.png", 777)>]
  [<InlineData("Asset/Test/ScreenShot_94.png",   94)>]
  [<InlineData("Asset/Test/ScreenShot_14.png",   14)>]
  // [<InlineData("Asset/Test/ScreenShot_80.png",   80)>]
  // [<InlineData("Asset/Test/ScreenShot_78.png",   78)>]
  [<InlineData("Asset/Test/ScreenShot_207.png", 207)>]
  [<InlineData("Asset/Test/ScreenShot_227.png", 227)>]
  member __.``OCR should read score from a screenshot with score``(filename : string, expectedScore) =
    use screenshot = new Texture(filename)
    let actualScore = getScore screenshot context
    actualScore |> should equal (Score expectedScore)

  [<Theory>]
  [<InlineData("Asset/Test/ScreenShot_NoScore.png")>]
  member __.``OCR should not read score from a screenshot without score``(filename : string) =
    use screenshot = new Texture(filename)
    let actualScore = getScore screenshot context
    actualScore |> should equal UncertainScore