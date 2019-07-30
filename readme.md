# SFSixtyScore

A tool to read the player's current score from a screenshot or video feed of Star Fox 64. It is _pre-alpha_ and is not yet ready for general use. It has only been tested on Windows, though a Linux version is planned.

## Running the code

Requires .NET Core 2.2.401 or newer.

To run the automated tests, enter the SFSixtyScore.Test folder and run:
```sh
dotnet test
```

To run the demo application, enter the SFSixtyScore.Demo folder and run:
```sh
dotnet run -x <x> -y <y>
```
...where `<x>` and `<y>` are the x- and y-coordinates of a Star Fox 64 video feed on your desktop. Right now the video feed must be exactly 320x240 pixels, though support for arbitrary resolutions is planned.
