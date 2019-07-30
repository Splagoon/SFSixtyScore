namespace SFSixtyScore

open System
open System.Drawing
open System.IO
open SFML.Graphics

type ScreenCapture(width : int, height : int) =
  let graphicsTarget = new Bitmap(width, height)
  let graphics = Graphics.FromImage(graphicsTarget)

  member __.CaptureScreen(x, y) =
    graphics.CopyFromScreen(Point(x, y), Point.Empty, Size(width, height))
    use memoryStream = new MemoryStream()
    graphicsTarget.Save(memoryStream, Imaging.ImageFormat.Bmp)
    new Texture(memoryStream)

  interface IDisposable with
    member __.Dispose() =
      graphicsTarget.Dispose()
      graphics.Dispose()