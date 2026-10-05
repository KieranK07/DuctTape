using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace RoundsPort.Runtime
{
    // Bknibb's UnboundLib 4 colours a health bar pink while its player has respawns left: a postfix on HealthBar.Update
    // reading data.stats. Health bars on things that aren't players (Cards+ snakes carry a CharacterData without
    // stats) threw it every frame; UnboundLib 3 had no such patch. Skip it where there are no stats to read.
    [HarmonyPatch]
    internal static class UL_HealthBarRespawns_Fix
    {
        static MethodBase Target() => AccessTools.Method(Types.Find("UnboundLib.Patches.HealthBar_Patch_Update"), "Postfix");
        static bool Prepare() => Target() != null;
        static MethodBase TargetMethod() => Target();

        // the target's parameters are (HealthBar __instance, CharacterData ___data): read by position
        static bool Prefix(object[] __args) => __args.Length > 1 && __args[1] is CharacterData data && data != null && data.stats != null;
    }

    // Bknibb's UnboundLib 4 and RoundsWithFriends 3 ask GitHub on every start whether a newer release is out, and put
    // "<mod> has an update available!" on the main menu. UnboundLib 3 had no such check. The copies AutoFix makes from
    // the old packages can't be updated by a mod manager, and one put in by hand loses the macOS fix, so the line only
    // misleads: it's skipped for the exact versions AutoFix makes. Any other version, and other mods' checks, stay.
    [HarmonyPatch]
    internal static class UL_UpdateNotice_Fix
    {
        // "<repo owner>/<repo name> <version>" of the ports AutoFix's patches produce
        static readonly string[] Ours = { "Bknibb/UnboundLib 4.2.7", "Bknibb/RoundsWithFriends 3.0.10" };
        static readonly HashSet<string> logged = new HashSet<string>();
        internal static ManualLogSource Log;

        // registering starts the GitHub request; the menu line is made once it answers
        static IEnumerable<MethodBase> Targets()
        {
            var t = Types.Find("UnboundLib.Utils.UI.UpdateChecker");
            return t == null ? Enumerable.Empty<MethodBase>()
                : new[] { "RegisterModUpdateChecker", "CreateUpdateMenu" }.Select(n => (MethodBase)AccessTools.Method(t, n)).Where(m => m != null);
        }
        static bool Prepare() => Targets().Any();
        static IEnumerable<MethodBase> TargetMethods() => Targets();

        // both take one UpdateChecker.ModUpdateChecker: read by position
        static bool Prefix(object[] __args, MethodBase __originalMethod)
        {
            if (__args.Length == 0 || __args[0] == null) return true;
            var c = Traverse.Create(__args[0]);
            var key = $"{c.Field("repoOwner").GetValue<string>()}/{c.Field("repoName").GetValue<string>()} {c.Field("currentVersion").GetValue<string>()}";
            if (Array.IndexOf(Ours, key) < 0) return true;
            if (__originalMethod.Name == "RegisterModUpdateChecker" && logged.Add(key))
                Log?.LogInfo($"update notice for {key} skipped: DuctTape makes this version");
            return false;
        }
    }
}
