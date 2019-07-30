module SFSixtyScore.OCR

open System
open System.IO
open System.Reflection
open SFML.Graphics

type Glyph =
  | Glyph of int
  | UncertainGlyph

type ScoreReading =
  | Score of int
  | UncertainScore

/// Glyph confidence is based on number of matching pixels, but each glyph has
/// a different number of pixels. These are constants based on each glyph's
/// pixel area to normalize confidence across them
let glyphBias = function
  | Glyph 0 -> 1.200
  | Glyph 1 -> 2.364
  | Glyph 2 -> 1.147
  | Glyph 3 -> 1.418
  | Glyph 4 -> 1.182
  | Glyph 5 -> 1.258
  | Glyph 6 -> 1.114
  | Glyph 7 -> 1.814
  | Glyph 8 -> 1.000
  | Glyph 9 -> 1.083
  | _ -> 0.000

let filePath relativePath =
  let currentDirectory = Assembly.GetCallingAssembly().Location |> Path.GetDirectoryName
  Path.Combine(currentDirectory, relativePath)

type RenderContext() =
  let renderTarget : RenderTexture = new RenderTexture(13u, 15u)
  let compareShader : Shader = new Shader(null, null, filePath "Asset/Shader/Compare.glsl")

  let masks =
    seq {
      for i in 0..9 ->
        let filename = sprintf "Asset/Mask/%i.png" i
        Glyph i, new Texture(new Image(filePath filename))
    } |> Map.ofSeq

  member __.GetConfidence glyph input x =
    let mask = masks |> Map.find glyph
    compareShader.SetUniform("maskTexture", mask)
    compareShader.SetUniform("inputTexture", Shader.CurrentTexture)
    compareShader.SetUniformArray(
      "expectedColors",
      [| Color(151uy, 202uy, 226uy) |> Glsl.Vec4
         Color(226uy, 160uy, 113uy) |> Glsl.Vec4 |]
    )

    renderTarget.Draw(input, RenderStates(compareShader))
    renderTarget.Display()

    renderTarget.Texture.GenerateMipmap() |> ignore
    let sprite = new Sprite(renderTarget.Texture)
    sprite.Scale <- SFML.System.Vector2f(1.0f / 13.0f, 1.0f / 15.0f)
    renderTarget.Draw(sprite)
    renderTarget.Display()
    
    let renderedImage = renderTarget.Texture.CopyToImage()

    renderedImage.Pixels.[0] |> float

  interface IDisposable with
    member __.Dispose() =
      renderTarget.Dispose()
      compareShader.Dispose()
      masks |> Map.iter(fun _ texture -> texture.Dispose())

let getGlyph (texture : Texture) x y (context : RenderContext) =
  let cropArea = IntRect(x, y, 13, 15)
  let croppedInput = new Sprite(texture, cropArea)

  let glyphs = seq {
    for i in 0..9 do
      let glyph = Glyph i
      let confidence = context.GetConfidence glyph croppedInput x
      let bias = glyph |> glyphBias

      yield glyph, (confidence * bias)
  }

  let ((bestGlyph, bestScore), (_, secondScore)) =
    glyphs
    |> Seq.sortBy snd
    |> Seq.pairwise
    |> Seq.head
  
  if bestScore >= 100. || secondScore - bestScore < 1. then
    UncertainGlyph
  else
    bestGlyph

let glyphsToScore a b c =
  match a, b, c with
  | Glyph x, Glyph y, Glyph z -> Score ((x * 100) + (y * 10) + z)
  | _ -> UncertainScore

let getScore texture context =
  let glyph1 = getGlyph texture 24 32 context
  let glyph2 = getGlyph texture 37 32 context
  let glyph3 = getGlyph texture 50 32 context
  glyphsToScore glyph1 glyph2 glyph3