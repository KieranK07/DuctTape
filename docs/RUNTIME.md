# rounds-port Runtime

A BepInEx plugin for problems `fix` can't solve in a mod's DLL: old mods that load on the current game but then throw,
or draw wrong, while running together. Each patch puts back what the old game did. It adds nothing to the menus
except DuctTape's line in UnboundLib's credits (only when it runs from the DuctTape package), and writes to the log as
`rounds-port`.

Install: `rounds-port.Runtime.dll` in `BepInEx/plugins/`. Needs BepInEx 5.4.23 and the current game. If the
rounds-mac-modpack's Mac Compat Fixes is installed, BepInEx skips this plugin: that one has the same fixes.

| Fix | What goes wrong without it |
|---|---|
| Shaders (Metal only) | Mod asset bundles only have D3D11 shaders, so on macOS their cards and effects draw pink. Materials are pointed at the game's own Metal shaders, or rebuilt on `Particles/Standard Unlit` |
| Letterbox | On 16:10 screens nothing clears the bars around the 16:9 picture, so old frames stay there |
| Card names | UnboundLib's cards have no entry in the game's string table: titles show as the missing-translation text |
| Camera stack | The main camera's post-processing writes straight to the screen, and since Unity 2022 a camera rendering after it copies the stack's buffer over that: with Map Embiggener (its out-of-bounds camera) only the background showed, in menus, picks and battles |
| Stat names | Cards draw stat names from a localized string now; mods only set the plain `stat` text, so their stat lines showed the number with no name |
| `GetSourceCard` | Picked cards don't always carry the `(Clone)` name UnboundLib looks for: empty card bar buttons |
| Card bar hover | A button whose card was destroyed throws on hover |
| Stats panel | UnboundLib 4.2.5's stats panel calls `ResetStats` on detached components; mods patching `ResetStats` throw, and the first card pick never shows |
| Card visuals | RarityLib throws on destroyed rarity markers; the new card prefab's particles cover cards in the toggle-cards menu; menu card art doesn't animate on hover; selected menu buttons turn white |
| MapsExtended | Its object manager is destroyed at startup, so clients load custom maps without their physics objects |
| `CardBar.Update` patches | The game's `CardBar` has no `Update` now. AutoFix disables patches on it (they'd stop the mod's `PatchAll`); these call them every frame for each active card bar, as `Update` did (LocalZoom keeps the hovered card's zoom in step with the camera) |
| Cards Plus | Its Cyberpunk cards' effect also lands on the card prefab UnboundLib builds, which has no visual, and throws there at startup; skipped on the prefab only |
| Card source online | UnboundLib makes each modded card its own `sourceCard`, and the 2025 `CardInfo.Awake` only looks the source up when it's empty: a card another player picked pointed at the destroyed pick-screen copy, so card rules (ModdingUtils, Cosmic Rounds' Beetle) differed between machines |
| UnboundLib 4 health bars | Bknibb's UnboundLib 4 colours health bars for players with respawns left, reading `data.stats` every frame; things that aren't players (Cards+ snakes) have none and it threw every frame. UnboundLib 3 had no such patch |
| Update notices | Bknibb's RoundsWithFriends 3 puts "has an update available!" on the main menu when GitHub has a newer release (our UnboundLib fork doesn't check). RoundsWithFriends 2 had no such check, and the copy AutoFix makes can't be updated by a mod manager. Skipped for the exact version AutoFix makes (RoundsWithFriends 3.0.10) |
| Undrawn text materials | Asking a TextMeshPro text that hasn't been drawn yet for its materials throws now: LobbyImprovements builds its lobby code box from the main menu's font that way, so the box was never made |
| Prefab card text | Cards saved in an asset bundle and registered as they are (`CustomCard.RegisterUnityCard`: RSCards and others) keep their title and description in `cardName` / `cardDestription`; the current game draws both from localization, which they were saved without, so they showed no description. A card with old text and no localized text gets a LocalizedString with the text as its key, as UnboundLib 4 gives the cards it builds (`CardTextFixes.cs`). |
| LobbyImprovements lobby code | LobbyImprovements packs "<region>:<room name>" into a short code, parsing the room name as a number. The old game named rooms with digits, the current one with letters: packing threw as soon as a room was joined (the code box stayed empty, and the throw cut off the Photon callbacks after it) and joining refused letter names. Names are packed as base-36 numbers and joining takes letters (`LobbyCodeFixes.cs`). |
| LobbyImprovements hidden rooms | The old game hid private rooms and joined them by name, so LobbyImprovements took a visible room for a quick-match room: a host hid it, a client left it. The current game keeps private rooms visible in its room-code lobby and joins them by searching it, so nobody could join a LobbyImprovements host by code ("Found no rooms to join"). Rooms with a room code are left as they are (`LobbyCodeFixes.cs`). |

Source: `src/Runtime`. Patches whose target mod isn't installed are skipped. Safe to swap with Hot Reload: every load
patches under its own Harmony id and undoes everything when it unloads.
