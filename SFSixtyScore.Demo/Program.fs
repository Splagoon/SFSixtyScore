open System
open CommandLine
open SFSixtyScore

type CLIOptions =
  { [<Option('x', "position-x", Required = true)>]
    x : int
     
    [<Option('y', "position-y", Required = true)>]
    y : int
    
    [<Option('w', "width", Default = 320)>]
    width : int
    
    [<Option('h', "height", Default = 240)>]
    height : int }

[<EntryPoint>]
let main argv =
  let argsParseResult = Parser.Default.ParseArguments<CLIOptions>(argv)
  match argsParseResult with
  | :? Parsed<CLIOptions> as options ->
    use scoreReader = new ScoreReader(options.Value.x,
                                      options.Value.y,
                                      options.Value.width,
                                      options.Value.height)

    let mutable lastScore = -1
    let onScore score =
      if score <> lastScore then
        Console.Clear()
        printfn "Score: %i" score
        lastScore <- score

    scoreReader.Run(onScore)
    0
  | _ -> 1
