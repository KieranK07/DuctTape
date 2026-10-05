# DuctTape

Makes mods from before the December 2025 update work on the current version of ROUNDS.

## Install

Install DuctTape with your mod manager, next to your mods. Keep UnboundLib, MMHook and RoundsWithFriends installed
and updated as usual.

Everyone in a lobby needs DuctTape and the same mods. Windows and Mac players can play together; on a Mac, use
[Crosswind](https://github.com/KieranK07/crosswind).

## What it does

Every time the game starts, DuctTape:

- swaps the old UnboundLib, MMHook and RoundsWithFriends for Bknibb's versions for the current game
- updates each old mod's code for the current game (the first start takes a few seconds longer)
- applies hand-made fixes to popular mods that needed more
- fixes in-game problems the update caused
- adds Odin Serializer and turns on `HideManagerGameObject` in `BepInEx.cfg`, which the current game needs

Mods behave as they did on the old game. Mods already updated for the current game are left alone. See how the 98
most-downloaded mods do:
[compatibility list](https://github.com/KieranK07/rounds-porting-toolkit/blob/main/docs/COMPATIBILITY.md).

## Turning it off

Set `RestoreOriginals = true` in `BepInEx/config/rounds-port.autofix.cfg` and start the game once. Your mods get their
original files back, and you can uninstall DuctTape. To leave a mod untouched, add it to `Exclude` in the same file.

On the `old-rounds-for-mods` beta, DuctTape puts the original files back and does nothing else.

## Problems

Some mods need their author to update them; `BepInEx/LogOutput.log` lists these as MANUAL. Bugs a mod already had
on the old game stay.

Anything else: [open an issue](https://github.com/KieranK07/DuctTape/issues) with your `BepInEx/LogOutput.log`.

## Credits

- [Bknibb](https://github.com/Bknibb)'s UnboundLib and RoundsWithFriends ports, included with his OK. UnboundLib is
  built from [our fork](https://github.com/KieranK07/UnboundLib/tree/ducttape) of his.
- Hand-made fixes from the [ROUNDS Porting Toolkit](https://github.com/KieranK07/rounds-porting-toolkit).
- [Odin Serializer](https://github.com/TeamSirenix/odin-serializer) (Apache 2.0) and
  [Octokit](https://github.com/octokit/octokit.net) (MIT), licences included. Built with
  [Mono.Cecil](https://github.com/jbevain/cecil) and [Harmony](https://github.com/pardeike/Harmony).
- RoundsWithFriends and the fixes for GPL-3.0 mods are GPL-3.0. Source links are in `patchers/NOTICE.md`.

ROUNDS is © Landfall Games. DuctTape is not affiliated with Landfall.
