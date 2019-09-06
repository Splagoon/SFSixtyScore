module Mask

open System.IO
open Newtonsoft.Json
open System.Drawing

type MaskData =
  | Positive
  | Negative
  | Neutral

type PixelData = int * int * MaskData

type Point = int * int

let allPoints : Point list =
  seq {
    for y in 0 .. 14 do
      for x in 0 .. 12 do
        yield x, y
  } |> Seq.toList

let getPixelData (bitmap : Bitmap) : Map<Point, MaskData> =
  let getMaskData (color : Color) =
    if color.R = 255uy then
      Positive
    elif color.R = 0uy then
      Negative
    else
      Neutral

  seq {
    for (x, y) as point in allPoints do
      let color = bitmap.GetPixel(x, y)
      yield point, color |> getMaskData
  } |> Map.ofSeq

let getMasks () =
  let maskData =
    seq {
      for i in 0..9 do
        let mask = Bitmap.FromFile(sprintf "C:/Users/Rob/Documents/SFSixtyScore/SFSixtyScore/Asset/Mask/%i.png" i) :?> Bitmap
        yield i, mask |> getPixelData
    } |> Map.ofSeq

  let usefulPoints =
    let rec findUsefulPoints foundPoints remainingPoints =
      match remainingPoints with
      | [] -> foundPoints
      | (x, y as nextPoint) :: newRemainingPoints ->
          let pointFound =
            // Do not include points too close to those already found
            if x > 0 && foundPoints |> Set.contains (x-1, y) then
              None
            elif y > 0 && foundPoints |> Set.contains (x, y-1) then
              None
            elif x > 0 && y > 0 && foundPoints |> Set.contains (x-1, y-1) then
              None
            else
              let getPoint (maskMap : Map<Point, MaskData>) =
                maskMap.[nextPoint]
              let values =
                maskData
                |> Map.toSeq
                |> Seq.map (snd >> getPoint)

              let hasPositive = values |> Seq.contains Positive
              let hasNegative = values |> Seq.contains Negative
              if hasPositive && hasNegative then
                Some nextPoint
              else
                None

          let newFoundPoints =
            match pointFound with
            | None -> foundPoints
            | Some point -> foundPoints.Add(point)

          findUsefulPoints newFoundPoints newRemainingPoints
    findUsefulPoints (Set Seq.empty) allPoints |> Set.toArray

  let numUseful = usefulPoints.Length
  let numTotal = allPoints.Length
  let ratio = (numUseful |> float) / (numTotal |> float) * 100.

  printfn "Number of useful points: %i / %i (%.2f%%)" numUseful numTotal ratio

  use stream = File.CreateText("masks.json")
  use json = new JsonTextWriter(stream)

  json.WriteStartObject()

  json.WritePropertyName("points")

  json.WriteStartArray()
  for x, y in usefulPoints do
    json.WriteStartObject()
    json.WritePropertyName("x")
    json.WriteValue(x)
    json.WritePropertyName("y")
    json.WriteValue(y)
    json.WriteEndObject()
  json.WriteEndArray()

  json.WritePropertyName("masks")
  json.WriteStartObject()

  for i in 0..9 do
    json.WritePropertyName(i |> string)

    json.WriteStartObject()
    json.WritePropertyName("positive")
    json.WriteStartArray()
    let positivePoints =
      maskData.[i]
      |> Map.toSeq
      |> Seq.filter (fun (_, data) -> data = Positive)
      |> Seq.map fst
    for point in positivePoints do
      match usefulPoints |> Array.tryFindIndex ((=) point) with
      | Some i -> json.WriteValue(i)
      | None -> ()
    json.WriteEndArray()

    json.WritePropertyName("negative")
    json.WriteStartArray()
    let negativePoints =
      maskData.[i]
      |> Map.toSeq
      |> Seq.filter (fun (_, data) -> data = Negative)
      |> Seq.map fst
    for point in negativePoints do
      match usefulPoints |> Array.tryFindIndex ((=) point) with
      | Some i -> json.WriteValue(i)
      | None -> ()
    json.WriteEndArray()
    json.WriteEndObject()

  json.WriteEndObject()
 