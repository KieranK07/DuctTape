# DuctTape

Makes mods from before the December 2025 update work on the current version of ROUNDS.

## Install

- **Mod manager** (r2modman, Thunderstore Mod Manager, Gale): install DuctTape from Thunderstore next to your mods.
- **Mac:** use [Crosswind](https://github.com/KieranK07/crosswind), a mod manager with DuctTape built in.
- **By hand:** download the Windows or macOS zip from [Releases](https://github.com/KieranK07/DuctTape/releases/latest)
  and unzip it into your game folder (Steam: right-click ROUNDS > Manage > Browse local files).
  - Windows: install [BepInExPack_ROUNDS](https://thunderstore.io/c/rounds/p/BepInEx/BepInExPack_ROUNDS/) first, then
    start the game from Steam.
  - Mac: the zip includes BepInEx. Start the game from Terminal with `./run_bepinex.sh` in the game folder.

Keep UnboundLib, MMHook and RoundsWithFriends installed and updated as usual. Everyone in a lobby needs DuctTape and
the same mods.

## What it does

Every time the game starts, DuctTape swaps the old UnboundLib, MMHook and RoundsWithFriends for Bknibb's versions for
the current game, updates each old mod's code, applies hand-made fixes to popular mods that needed more, and fixes
in-game problems the update caused. Mods behave as they did on the old game.

Details: [AutoFix](docs/AUTOFIX.md) (the load-time patcher) and [Runtime](docs/RUNTIME.md) (the in-game fixes). How
the 98 most-downloaded mods do:
[compatibility list](https://github.com/KieranK07/rounds-porting-toolkit/blob/main/docs/COMPATIBILITY.md).

To turn it off, set `RestoreOriginals = true` in `BepInEx/config/rounds-port.autofix.cfg`, start the game once, then
remove DuctTape.

A mod still broken? [Open an issue](https://github.com/KieranK07/DuctTape/issues) with its name and your
`BepInEx/LogOutput.log`.

## Building

Needs the .NET SDK, ROUNDS with BepInEx, and the [ROUNDS Porting Toolkit](https://github.com/KieranK07/rounds-porting-toolkit)
checked out next to this repo. The macOS zip also takes BepInEx from a [Crosswind](https://github.com/KieranK07/crosswind)
checkout next to it.

```
python scripts/curated.py                   # the toolkit's patches into src/AutoFix/curated
dotnet build src/AutoFix -c Release
dotnet build src/Runtime -c Release
python scripts/package.py                   # dist/: the Thunderstore package, the Windows and macOS zips
```

Other locations: `-p:GameDir=...` and `-p:Toolkit=...` for the builds, `python scripts/curated.py <toolkit>`,
`ROUNDS_TOOLKIT=<toolkit>` and `CROSSWIND=<path>` for the package.

## License

MIT. Bknibb's UnboundLib and RoundsWithFriends ports are included with his OK; UnboundLib is built from
[our fork](https://github.com/KieranK07/UnboundLib/tree/ducttape) of his. RoundsWithFriends and the fixes for GPL-3.0
mods are GPL-3.0 (see [NOTICE.md](thunderstore/NOTICE.md)). ROUNDS is © Landfall Games. DuctTape is not affiliated
with Landfall.
