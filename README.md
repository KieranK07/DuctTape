# DuctTape

Holds old ROUNDS mods together on the current game until their authors update them.

## Your mods broke after the ROUNDS update?

Mods made before the December 2025 update don't work on the current game. The usual fix is to roll back to the
`old-rounds-for-mods` beta, but that beta has no Mac version, so Steam keeps putting a Mac back on the newest ROUNDS.

DuctTape lets you stay on the current game and keep the mods you already have. No old beta, no emulator, no waiting
for new versions of your mods, and no rebuilding your mod list.

## Install

### What you need

- **ROUNDS on the current version:** Steam → right-click ROUNDS → **Properties → Betas → None**.
- **Your mods, any version,** plus the mods they need (each mod's Thunderstore page lists them, like UnboundLib and
  MMHook). On Thunderstore, a mod's page has a **Manual Download** button.
- **Internet the first time you start the game with DuctTape:** it downloads [Bknibb](https://github.com/Bknibb)'s
  updated UnboundLib and RoundsWithFriends from GitHub.
- **BepInEx,** the mod loader: on Windows, BepInExPack_ROUNDS (step 1 below). The Mac download already has it.

Your **game folder** is where all of this goes: Steam → right-click ROUNDS → **Manage → Browse local files**.

### On a Mac (Apple Silicon or Intel)

1. Download the **DuctTape-macOS** zip from [Releases](https://github.com/KieranK07/DuctTape/releases/latest), unzip
   it and move everything inside it into your game folder. Already have a `BepInEx` folder there? Hold **Option** while
   you drag and choose **Merge**, so your mods stay.
2. Unzip each mod into its own folder inside `BepInEx/plugins`.
3. Open Steam. Then open Terminal and paste this:

   ```
   cd ~/Library/Application\ Support/Steam/steamapps/common/ROUNDS && ./run_bepinex.sh
   ```

   Start the game this way every time. Steam's Play button starts it without mods.

### On Windows

1. Download **BepInExPack_ROUNDS** from [Thunderstore](https://thunderstore.io/c/rounds/p/BepInEx/BepInExPack_ROUNDS/)
   and copy what's inside its `BepInExPack_ROUNDS` folder into your game folder.
2. Download the **DuctTape-Windows** zip from [Releases](https://github.com/KieranK07/DuctTape/releases/latest) and
   unzip it into your game folder.
3. Unzip each mod into its own folder inside `BepInEx\plugins`.
4. Start ROUNDS from Steam as usual.

### Did it work?

The first start takes a few seconds longer. In the game folder, `BepInEx/LogOutput.log` then has lines from
`rounds-port` (DuctTape's name in the log), like `Bknibb's port in place of the old release` and
`6 mods: 5 fixed, 1 need nothing`.

### Playing with friends

Everyone in a lobby needs the current ROUNDS, the same mods and DuctTape. Mac and Windows players can play together.

## Rather not set it up by hand?

[Crosswind](https://github.com/KieranK07/crosswind) is a mod manager for Mac and Windows with DuctTape built in. Pick
your mods from a list, share your mod list with friends as a code, and press Launch. Steps are on
[its page](https://github.com/KieranK07/crosswind#rounds-mods-on-a-mac-and-windows).

On r2modman, Thunderstore Mod Manager or Gale: the Thunderstore package is coming. It waits on Bknibb's ports being on
Thunderstore, so they can be dependencies instead of downloads.

## What it does

Every time the game starts, it puts in Bknibb's updated UnboundLib and RoundsWithFriends, ports each old mod's code,
applies hand-made fixes for the mods that needed more, and fixes the problems that only show while playing. Mods
behave as they did on the old game. The details: [thunderstore/README.md](thunderstore/README.md). How the 98
most-downloaded mods did:
[compatibility list](https://github.com/KieranK07/rounds-porting-toolkit/blob/main/docs/COMPATIBILITY.md).

To turn it off, delete the `DuctTape` folders in `BepInEx/patchers` and `BepInEx/plugins`, and set
`RestoreOriginals = true` in `BepInEx/config/rounds-port.autofix.cfg` first if you want your mods' original files
back.

A mod still broken? [Open an issue](https://github.com/KieranK07/DuctTape/issues) with its name and your
`BepInEx/LogOutput.log`.

## Building

Needs the .NET SDK, ROUNDS with BepInEx, and the [ROUNDS Porting Toolkit](https://github.com/KieranK07/rounds-porting-toolkit)
checked out next to this repo: AutoFix builds from its porting code, and the package takes its hand-made patches and
Odin stand-in. The Mac zip also takes the Apple Silicon BepInEx from a [Crosswind](https://github.com/KieranK07/crosswind)
checkout next to it.

```
python scripts/curated.py                   # the toolkit's patches into src/AutoFix/curated
dotnet build src/AutoFix -c Release
dotnet build src/Runtime -c Release
python scripts/package.py                   # dist/: the Thunderstore package, the Windows and Mac zips
```

ROUNDS somewhere else: `-p:GameDir=...`. The toolkit somewhere else: `-p:Toolkit=...` for the builds,
`python scripts/curated.py <toolkit>` and `ROUNDS_TOOLKIT=<toolkit>` for the package; Crosswind: `CROSSWIND=<path>`.

More: [AutoFix](docs/AUTOFIX.md) (the load-time patcher), [Runtime](docs/RUNTIME.md) (the in-game fixes).

MIT licensed. ROUNDS is © Landfall Games; not affiliated with Landfall.
