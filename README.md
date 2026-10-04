# DuctTape

Holds old ROUNDS mods together on the current game until their authors update them.

Mods made before the December 2025 update break on the current version of ROUNDS. DuctTape fixes them every time the
game starts: it puts in [Bknibb](https://github.com/Bknibb)'s updated UnboundLib and RoundsWithFriends, ports each old
mod's code, applies hand-made fixes for the mods that needed more, and fixes the problems that only show while playing.
Mods behave as they did on the old game. The details: [thunderstore/README.md](thunderstore/README.md). How the 98
most-downloaded mods did: [compatibility list](https://github.com/KieranK07/rounds-porting-toolkit/blob/main/docs/COMPATIBILITY.md).

## Get it

- **On a Mac, or want it all in one app:** [Crosswind](https://github.com/KieranK07/crosswind) has DuctTape built in.
- **r2modman, Thunderstore Mod Manager or Gale:** the Thunderstore package is coming. It waits on Bknibb's ports being
  on Thunderstore, so they can be dependencies instead of downloads.

## Building

Needs the .NET SDK, ROUNDS with BepInEx, and the [ROUNDS Porting Toolkit](https://github.com/KieranK07/rounds-porting-toolkit)
checked out next to this repo: AutoFix builds from its porting code, and the package takes its hand-made patches and
Odin stand-in.

```
python scripts/curated.py                   # the toolkit's patches into src/AutoFix/curated
dotnet build src/AutoFix -c Release
dotnet build src/Runtime -c Release
python scripts/package.py                   # dist/DuctTape-<version>.zip
```

ROUNDS somewhere else: `-p:GameDir=...`. The toolkit somewhere else: `-p:Toolkit=...` for the builds,
`python scripts/curated.py <toolkit>` and `ROUNDS_TOOLKIT=<toolkit>` for the package.

More: [AutoFix](docs/AUTOFIX.md) (the load-time patcher), [Runtime](docs/RUNTIME.md) (the in-game fixes).

MIT licensed. ROUNDS is © Landfall Games; not affiliated with Landfall.
