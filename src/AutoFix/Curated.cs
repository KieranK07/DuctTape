using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using BepInEx.Logging;
using Mono.Cecil;

// What Crosswind does to a ROUNDS profile before launch (crosswind: src-tauri/src/rounds.rs), done here so players
// on any mod manager get it: old UnboundLib 3 / MMHook / RoundsWithFriends 2 files become Bknibb's ports, and exact mod
// versions that needed hand-made fixes get their patch from the toolkit's patches/ (curated/, made by
// scripts/curated.py). AutoFix's own fix runs on the result.
// Nothing is downloaded. The newest old releases (UnboundLib 3.2.14, MMHook 1.0.0, RoundsWithFriends 2.2.2) become
// the ports through hand-made patches (patches/PATCHLOG-libraries.md in the toolkit). If a port is installed as well,
// old copies get its bytes instead, so whatever loads a library by name gets the port.
sealed class Curated(string cache, ManualLogSource log)
{
    sealed record Port(string File, string Sha, string Package);

    static readonly Port[] Ports =
    {
        new("UnboundLib.dll", "d5e6b2411e1fa6f4bb5f4b2a32ddf49ad1deee24e2ca314d42bce346095d146d", "UnboundLib 4 (https://github.com/Bknibb/UnboundLib/releases)"),
        new("MMHOOK_Assembly-CSharp.dll", "926b53b329d94f6a8842e6d51ca17ff96f081df59695d7862845d5ccce9e5a62", "UnboundLib 4 (https://github.com/Bknibb/UnboundLib/releases)"),
        new("RoundsWithFriends.dll", "1bd4d5aa47de0e04661710a77bb0b5f1214dac4b3baabc9364b3418ecbc8ab61", "RoundsWithFriends 3 (https://github.com/Bknibb/RoundsWithFriends/releases)"),
    };

    sealed record Patch(string Name, string Before, string After, string Resource);
    static readonly Dictionary<string, Patch> Patches = ReadPatches();

    // old libraries this start replaces: library file -> (the port's file, where it came from for the log)
    readonly Dictionary<string, (string Path, string From)> needed = new(StringComparer.OrdinalIgnoreCase);

    // Which old libraries get an installed port: an old release is installed and so is something newer (Bknibb's port
    // first, whatever else isn't an old release otherwise). Without one, a patch turns the old release into the port;
    // an old release with no patch stays as it is, with a warning.
    public IEnumerable<string> Plan(IEnumerable<string> pluginFiles)
    {
        foreach (var g in pluginFiles.GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            var port = Ports.FirstOrDefault(p => p.File.Equals(g.Key, StringComparison.OrdinalIgnoreCase));
            if (port == null) continue;
            var copies = g.Select(f => (path: f, bytes: File.ReadAllBytes(f))).ToList();
            if (!copies.Any(c => IsOld(c.path, c.bytes))) continue;
            var installed = copies.Where(c => !IsOld(c.path, c.bytes)).OrderByDescending(c => Sha(c.bytes) == port.Sha).FirstOrDefault();
            if (installed.path != null) { needed[port.File] = (installed.path, Short(installed.path)); continue; }
            // patched now, before any mod is fixed: mods are fixed against the port, not the old release
            var old = copies.Where(c => IsOld(c.path, c.bytes)).Select(c => (c.path, c.bytes, patch: Patches.TryGetValue(g.Key + "\t" + Sha(c.bytes), out var p) ? p : null)).ToList();
            if (old.FirstOrDefault(o => o.patch != null) is { patch: { } first } src)
            {
                var staged = Path.Combine(cache, "ports", first.After + ".dll");
                if (!File.Exists(staged) || Sha(File.ReadAllBytes(staged)) != first.After)
                {
                    var result = Bspatch(src.bytes, Resource(first));
                    if (Sha(result) != first.After) { log.LogWarning($"the curated patch for {g.Key} gave the wrong file; left as it is"); continue; }
                    Directory.CreateDirectory(Path.GetDirectoryName(staged)!);
                    File.WriteAllBytes(staged, result);
                }
                needed[port.File] = (staged, "curated patch " + first.Resource.Substring("curated/".Length));
            }
            foreach (var (path, bytes, _) in old.Where(o => o.patch == null && !needed.ContainsKey(port.File)))
                log.LogWarning($"{Short(path)} is {OldName(bytes)}, which DuctTape can't update: update it in your mod manager (or install Bknibb's {port.Package}). Until then, mods that need it won't load");
        }
        return needed.Values.Select(n => n.Path);
    }

    // The bytes to start from instead of a plugin's own (the same array when nothing applies).
    public byte[] Apply(string path, byte[] bytes, string sha)
    {
        var file = Path.GetFileName(path);
        if (needed.TryGetValue(file, out var port) && IsOld(path, bytes))
        {
            log.LogInfo($"{Short(path)}: Bknibb's port in place of the old release ({port.From})");
            bytes = File.ReadAllBytes(port.Path);
            sha = Sha(bytes);   // the installed port may already have its hand-made patch from an earlier start
        }
        // one patch after another: an old library becomes its port, which then gets its own (macOS) patch
        for (var n = 0; n < 4 && Patches.TryGetValue(file + "\t" + sha, out var patch); n++)
        {
            var result = Bspatch(bytes, Resource(patch));
            if (Sha(result) != patch.After) throw new InvalidDataException($"the curated patch for {file} gave the wrong file");
            log.LogInfo($"{Short(path)}: curated patch ({patch.Resource.Substring("curated/".Length)})");
            bytes = result;
            sha = patch.After;
        }
        return bytes;
    }

    // <package folder>/<file>, as AutoFix's other lines name plugins
    static string Short(string path) => Path.GetFileName(Path.GetDirectoryName(path)) + "/" + Path.GetFileName(path);

    static byte[] Resource(Patch patch)
    {
        using var s = typeof(Curated).Assembly.GetManifestResourceStream(patch.Resource)!;
        var ms = new MemoryStream();
        s.CopyTo(ms);
        return ms.ToArray();
    }

    static string OldName(byte[] bytes)
    {
        try { using var m = ModuleDefinition.ReadModule(new MemoryStream(bytes)); return $"{m.Assembly.Name.Name} {m.Assembly.Name.Version.ToString(3)}"; }
        catch { return "an old release"; }
    }

    static bool IsOld(string path, byte[] bytes)
    {
        try
        {
            using var m = ModuleDefinition.ReadModule(new MemoryStream(bytes));
            return m.Assembly != null && Game.OldLibrary(path, m.Assembly.Name) != null;
        }
        catch { return false; }
    }

    // ROUNDS has no Linux build (Linux players run the Windows one), so a Unix Mono is macOS
    static bool OnMac => Environment.OSVersion.Platform is PlatformID.Unix or PlatformID.MacOSX;

    static Dictionary<string, Patch> ReadPatches()
    {
        var d = new Dictionary<string, Patch>(StringComparer.OrdinalIgnoreCase);
        using var s = typeof(Curated).Assembly.GetManifestResourceStream("curated/patches.tsv");
        if (s == null) return d;
        foreach (var line in new StreamReader(s).ReadToEnd().Split('\n'))
        {
            var c = line.Split('\t');
            // a fifth column "macos": only there (UnboundLib's Windows-only "hold Left Shift" check)
            if (c.Length >= 5 && c[4].Trim() == "macos" && !OnMac) continue;
            if (c.Length >= 4) d[c[0] + "\t" + c[1]] = new Patch(c[0], c[1], c[2], "curated/" + c[3].Trim());   // two mods ship the same file
        }
        return d;
    }

    // BSDIFF40 with raw deflate blocks in place of bzip2 ("BSDIFFDF", see scripts/curated.py)
    static byte[] Bspatch(byte[] old, byte[] patch)
    {
        if (Encoding.ASCII.GetString(patch, 0, 8) != "BSDIFFDF") throw new InvalidDataException("not a BSDIFFDF patch");
        long clen = Off(patch, 8), dlen = Off(patch, 16), size = Off(patch, 24);
        using var ctrl = Block(patch, 32, clen);
        using var diff = Block(patch, 32 + clen, dlen);
        using var extra = Block(patch, 32 + clen + dlen, patch.Length - 32 - clen - dlen);
        var result = new byte[size];
        var c = new byte[24];
        long o = 0, n = 0;
        while (n < size)
        {
            Fill(ctrl, c, 0, 24);
            long add = Off(c, 0), copy = Off(c, 8), seek = Off(c, 16);
            Fill(diff, result, n, add);
            for (long i = 0; i < add; i++)
                if (o + i >= 0 && o + i < old.Length) result[n + i] += old[o + i];
            n += add; o += add;
            Fill(extra, result, n, copy);
            n += copy; o += seek;
        }
        return result;
    }

    static DeflateStream Block(byte[] b, long at, long len) => new(new MemoryStream(b, (int)at, (int)len), CompressionMode.Decompress);

    static void Fill(Stream s, byte[] into, long at, long len)
    {
        while (len > 0)
        {
            int r = s.Read(into, (int)at, (int)Math.Min(len, int.MaxValue));
            if (r <= 0) throw new InvalidDataException("the patch is cut short");
            at += r; len -= r;
        }
    }

    static long Off(byte[] b, long at)
    {
        long x = BitConverter.ToInt64(b, (int)at);
        return x < 0 ? -(x & long.MaxValue) : x;
    }

    static string Sha(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }
}
