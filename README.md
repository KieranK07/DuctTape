# DuctTape

Runs mods made before the December 2025 update on the current ROUNDS.

## Install

- **Mod manager:** install DuctTape from Thunderstore next to your mods.
- **Mac:** use [Crosswind](https://github.com/KieranK07/crosswind).
- **By hand:** unzip the Windows or macOS zip from [Releases](https://github.com/KieranK07/DuctTape/releases/latest)
  into the game folder. Windows needs [BepInExPack_ROUNDS](https://thunderstore.io/c/rounds/p/BepInEx/BepInExPack_ROUNDS/)
  first. On a Mac, launch with `./run_bepinex.sh` from the game folder.

Everyone in a lobby needs it.

## What it does

On launch it swaps the old UnboundLib, MMHook and RoundsWithFriends for Bknibb's ports, updates old mods' code for
the current game, patches the popular ones that needed more, and fixes in-game problems the update caused. Mods
behave as they did on the old game. Details: [AutoFix](docs/AUTOFIX.md), [Runtime](docs/RUNTIME.md).

Uninstall: set `RestoreOriginals = true` in `BepInEx/config/rounds-port.autofix.cfg`, launch once, remove DuctTape.

Broken mod: [open an issue](https://github.com/KieranK07/DuctTape/issues) with `BepInEx/LogOutput.log`.

## Building

Needs the .NET SDK, ROUNDS with BepInEx, and the [ROUNDS Porting Toolkit](https://github.com/KieranK07/rounds-porting-toolkit)
next to this repo ([Crosswind](https://github.com/KieranK07/crosswind) too, for the macOS zip).

```
python scripts/curated.py
dotnet build src/AutoFix -c Release
dotnet build src/Runtime -c Release
python scripts/package.py
```

Elsewhere: `-p:GameDir=` / `-p:Toolkit=` for the builds, `curated.py <toolkit>`, `ROUNDS_TOOLKIT=` / `CROSSWIND=` for the package.

## License

MIT. Bknibb's ports are included with his OK; UnboundLib is built from
[our fork](https://github.com/KieranK07/UnboundLib/tree/ducttape). GPL-3.0 parts: [NOTICE.md](thunderstore/NOTICE.md).
Not affiliated with Landfall.
