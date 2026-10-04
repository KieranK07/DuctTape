# DuctTape

Holds old ROUNDS mods together on the current game until their authors update them.

## Your mods broke after the ROUNDS update?

Mods made before the December 2025 update don't work on the current game. The usual fix is to roll back to the
`old-rounds-for-mods` beta, but that beta has no Mac version, so Steam keeps putting a Mac back on the newest ROUNDS.

DuctTape lets you stay on the current game and keep the mods you already have. No old beta, no emulator, no waiting
for new versions of your mods, and no rebuilding your mod list.

### On a Mac (Apple Silicon or Intel)

DuctTape comes built into [Crosswind](https://github.com/KieranK07/crosswind), a mod manager for Mac and Windows.

1. Download **Crosswind-macOS.zip** from [Crosswind's releases](https://github.com/KieranK07/crosswind/releases/latest),
   unzip it and drag **Crosswind** into Applications.
2. Open it. The first time, macOS blocks it: go to **System Settings → Privacy & Security** and click **Open Anyway**.
3. Choose **ROUNDS** and bring your mods as they are: **Import → ...profile from code** if you have a code, or install
   them from the mod list. Old versions are fine.
4. Open Steam, then press **Launch** in Crosswind. Steam's own Play button starts the game without mods.

### Playing with friends on Windows

Everyone in a lobby needs the same ROUNDS version and the same mods. Your friends:

1. Steam → right-click ROUNDS → **Properties → Betas → None**, so they're on the current game like you.
2. Install **Crosswind-Windows-setup.exe** from the same [releases page](https://github.com/KieranK07/crosswind/releases/latest).
3. Import your mods: you **Export → ...profile as code**, they **Import → ...profile from code**.

### r2modman, Thunderstore Mod Manager or Gale

The Thunderstore package is coming. It waits on Bknibb's ports being on Thunderstore, so they can be dependencies
instead of downloads. Until then, use Crosswind.

## What it does

Every time the game starts, it puts in [Bknibb](https://github.com/Bknibb)'s updated UnboundLib and
RoundsWithFriends, ports each old mod's code, applies hand-made fixes for the mods that needed more, and fixes the
problems that only show while playing. Mods behave as they did on the old game. The details:
[thunderstore/README.md](thunderstore/README.md). How the 98 most-downloaded mods did:
[compatibility list](https://github.com/KieranK07/rounds-porting-toolkit/blob/main/docs/COMPATIBILITY.md).

A mod still broken? [Open an issue](https://github.com/KieranK07/DuctTape/issues) with its name and your
`BepInEx/LogOutput.log` (in Crosswind: **File → Open profile folder**).

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
