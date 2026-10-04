"""The downloads, into dist/:
- DuctTape-<version>.zip, the Thunderstore package: thunderstore/ (manifest, README, icon) + AutoFix + Runtime + Odin
  Serializer, laid out as r2modman/Gale install it (patchers/ -> BepInEx/patchers/<pkg>, plugins/ -> BepInEx/plugins/<pkg>).
- DuctTape-Windows-<version>.zip and DuctTape-macOS-<version>.zip, unzipped into the game folder by hand: the same
  files already in BepInEx/. The Mac one adds a BepInEx that runs on Apple Silicon (Crosswind's copy) and its launch
  script (mac/).

    python scripts/package.py [version]

Build AutoFix and the Runtime first (Release). Odin comes from the toolkit next to this repo ($ROUNDS_TOOLKIT), the Mac
BepInEx from the Crosswind checkout next to it ($CROSSWIND).
"""
import json, os, sys, zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
TS = os.path.join(ROOT, "thunderstore")
TK = os.environ.get("ROUNDS_TOOLKIT") or next(
    (p for p in (os.path.join(ROOT, "..", n) for n in ("toolkit-repo", "rounds-porting-toolkit")) if os.path.isdir(p)), "")
ODIN = os.path.join(TK, "odin")
CW = os.environ.get("CROSSWIND") or next(
    (p for p in (os.path.join(ROOT, "..", n) for n in ("gale-mac", "crosswind")) if os.path.isdir(p)), "")
MAC_BEPINEX = os.path.join(CW, "src-tauri", "resources", "macos")
MAC = os.path.join(ROOT, "mac")
BIN = lambda p, f: os.path.join(ROOT, "src", p, "bin", "Release", "net472", f)

FILES = {
    "patchers/rounds-port.AutoFix.dll": BIN("AutoFix", "rounds-port.AutoFix.dll"),
    "plugins/rounds-port.Runtime.dll": BIN("Runtime", "rounds-port.Runtime.dll"),
    "plugins/OdinSerializer/Sirenix.Serialization.dll": os.path.join(ODIN, "Sirenix.Serialization.dll"),
    "plugins/OdinSerializer/Sirenix.Serialization.Config.dll": os.path.join(ODIN, "Sirenix.Serialization.Config.dll"),
    "plugins/OdinSerializer/Sirenix.Utilities.dll": os.path.join(ODIN, "Sirenix.Utilities.dll"),
    "plugins/OdinSerializer/LICENSE.txt": os.path.join(ODIN, "Sirenix-OdinSerializer-LICENSE.txt"),
    "README.md": os.path.join(TS, "README.md"),
    "CHANGELOG.md": os.path.join(TS, "CHANGELOG.md"),
    "icon.png": os.path.join(TS, "icon.png"),
}


def main():
    manifest = json.load(open(os.path.join(TS, "manifest.json"), encoding="utf-8"))
    if len(sys.argv) > 1: manifest["version_number"] = sys.argv[1]
    assert len(manifest["description"]) <= 250, "Thunderstore: description is 250 characters at most"
    out = os.path.join(ROOT, "dist", f"{manifest['name']}-{manifest['version_number']}.zip")
    os.makedirs(os.path.dirname(out), exist_ok=True)
    with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
        z.writestr("manifest.json", json.dumps(manifest, indent=2) + "\n")
        for arc, src in FILES.items():
            z.write(src, arc)
    print(f"{out} ({os.path.getsize(out)} bytes)")

    drop_in(manifest, "Windows", lambda put, z: None)
    drop_in(manifest, "macOS", mac_files)


def drop_in(manifest, system, extra):
    """A zip unzipped into the game folder by hand: the package's files already in BepInEx/, plus extra()."""
    out = os.path.join(ROOT, "dist", f"{manifest['name']}-{system}-{manifest['version_number']}.zip")
    with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
        def put(src, arc, mode=0o644):
            info = zipfile.ZipInfo.from_file(src, arc)
            info.create_system, info.external_attr = 3, (0o100000 | mode) << 16   # Unix permissions, kept by unzip
            with open(src, "rb") as f: z.writestr(info, f.read(), zipfile.ZIP_DEFLATED)
        extra(put, z)
        for arc, src in FILES.items():
            if arc.startswith(("patchers/", "plugins/")):
                top, rest = arc.split("/", 1)
                put(src, f"BepInEx/{top}/DuctTape/{rest}")
    print(f"{out} ({os.path.getsize(out)} bytes)")


def mac_files(put, z):
    """A BepInEx that runs on Apple Silicon (Crosswind's copy) and its launch script."""
    for base, _, files in os.walk(MAC_BEPINEX):
        for f in files:
            src = os.path.join(base, f)
            put(src, os.path.relpath(src, MAC_BEPINEX).replace(os.sep, "/"))
    put(os.path.join(MAC, "run_bepinex.sh"), "run_bepinex.sh", 0o755)
    info = zipfile.ZipInfo("steam_appid.txt", (2026, 1, 1, 0, 0, 0))   # started outside Steam: which app it is
    info.create_system, info.external_attr = 3, 0o100644 << 16
    z.writestr(info, "1557740\n", zipfile.ZIP_DEFLATED)
    for f in ("NOTICE.txt", "LICENSE-BepInEx.txt", "LICENSE-UnityDoorstop.txt"):
        put(os.path.join(MAC, f), f"BepInEx/plugins/DuctTape/{f}")


main()
