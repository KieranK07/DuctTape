# DuctTape

Runs mods made before the December 2025 update on the current ROUNDS.

## Install

Install it next to your mods. Keep UnboundLib, MMHook and RoundsWithFriends installed as usual.

Everyone in a lobby needs it. On a Mac, use [Crosswind](https://github.com/KieranK07/crosswind).

## What it does

On launch it swaps the old UnboundLib, MMHook and RoundsWithFriends for Bknibb's ports, updates old mods' code for
the current game, and patches the popular ones that needed more. The first launch takes a few seconds longer. It
also turns on `HideManagerGameObject` in `BepInEx.cfg`, which the current game needs.

Mods behave as they did on the old game. Mods already updated are left alone.
[How the top 98 mods do.](https://github.com/KieranK07/rounds-porting-toolkit/blob/main/docs/COMPATIBILITY.md)

## Uninstall

Set `RestoreOriginals = true` in `BepInEx/config/rounds-port.autofix.cfg`, launch once, then uninstall.

## A mod still broken?

If `BepInEx/LogOutput.log` marks it MANUAL, its author has to update it. Bugs it had on the old game stay. Anything
else: [open an issue](https://github.com/KieranK07/DuctTape/issues) with the log.

## Credits

[Bknibb](https://github.com/Bknibb)'s UnboundLib and RoundsWithFriends ports, included with his OK. UnboundLib is
built from [our fork](https://github.com/KieranK07/UnboundLib/tree/ducttape). Odin Serializer (Apache 2.0), Octokit
(MIT), Mono.Cecil, Harmony. GPL-3.0 parts: `patchers/NOTICE.md`. Not affiliated with Landfall.
