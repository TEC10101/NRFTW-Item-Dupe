# Duplicator

Small Windows console tool to duplicate/backup No Rest For The Wicked (NRFTW) character and realm files.

## Requirements

- WinRAR (or change the code to use another archiver). Common install paths the app expects:
  - `C:\Program Files\WinRAR\WinRAR.exe`
  - `C:\Program Files (x86)\WinRAR\WinRAR.exe`

## Game data location

By default the game stores data under:

`%USERPROFILE%\AppData\LocalLow\Moon Studios\NoRestForTheWicked\DataStore`

## FAQ
#### Q: Is there a step by step guide to how to get it to work?
#### A: Nope.
>It's not complicated you just need to look and see it.  If you're worried, make a new realm and character and you'll see how it works.
>You have character save data and you have realm save data it's all in AppData/LocalLow/Moon Studios/NoRestForTheWicked/DataStore
>
>You can manually copy and backup your character save file, log in and transfer items off your character, then log out and restore your files, and when you log back in your character will have the stuff they had on them again + of course it'll be in the stash too (that's the difference between the "realm" and the "character" save data).  Just log in and out and then sort the folder by time most recently modified to see what files are for what.
>
>The tool just assists in the backup and restoration of the save data using WinRAR.
>
>If you prefer, you anxious fuck, just manually backup the AppData/LocalLow/Moon Studios/NoRestForTheWicked/DataStore Then you do whatever you want (including uninstalling and reinstalling probably) and if you fuck something up just copy it back into place.

## Build (from repo)

From the `Duplicator` project folder run:

```powershell
dotnet build -c Release
```

The compiled binary will be at `bin\Release\net10.0\Duplicator.dll` (framework-dependent) or use `dotnet publish` to create a platform-specific EXE.

## Publish (single-file Windows EXE)

To produce a single, self-contained EXE for Windows x64:

```powershell
dotnet publish Duplicator.csproj -c Release -r win10-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

Output will be in `Duplicator/publish` (for example `Duplicator.exe`).

## Run

Run from a terminal to see console output:

```powershell
.\publish\Duplicator.exe
```

Or run the framework-dependent build with:

```powershell
dotnet bin\Release\net10.0\Duplicator.dll
```

## Versioning

The project currently sets `FileVersion` in the project file. You can control assembly and package/version metadata in `Duplicator.csproj` using these properties:

- `Version` — package/product semantic version (used by NuGet and `dotnet pack`).
- `AssemblyVersion` — CLR assembly identity (four-part). Change only for breaking API changes.
- `FileVersion` — Windows file version (four-part). Good for build/patch increments.
- `InformationalVersion` — human-readable string (can include commit sha or prerelease tag).

Example snippet to add to `Duplicator.csproj`:

```xml
<PropertyGroup>
  <Version>1.1.0</Version>
  <AssemblyVersion>1.1.0.0</AssemblyVersion>
  <FileVersion>1.1.0.0</FileVersion>
  <InformationalVersion>1.1.0+build.123</InformationalVersion>
</PropertyGroup>
```

Rebuild after changing these values to embed them into the produced assembly.

## Notes & Troubleshooting

- If WinRAR is not installed, either install it or modify the code to call an alternative archiver.
- Run the app from a terminal so you can see error messages and prompts.
- If you want a portable distribution, publish as self-contained single-file as shown above.

## License

GNU GENERAL PUBLIC LICENSE
