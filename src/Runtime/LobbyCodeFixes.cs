using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace RoundsPort.Runtime
{
    // LobbyImprovements' lobby code is "<region>:<room name>" packed as two base-62 numbers. The old game named rooms with
    // digits; the current one names them with letters (CSHERW). Packing parsed the name as a number and threw as soon as
    // a room was joined, so the lobby code box stayed empty and the throw cut off the Photon callbacks registered after
    // LobbyImprovements'; joining refused any name that wasn't all digits. Names are now packed as base-36 numbers,
    // which take letters and digits alike, and joining accepts both.
    internal static class LobbyCode
    {
        internal static readonly Type Packer = Types.Find("LobbyImprovements.Utils.ObfuscateJoinCode");
        internal static readonly Type Handler = Types.Find("LobbyImprovements.Networking.LobbyCodeHandler");
        const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        internal static long FromBase36(string s)
        {
            long v = 0;
            foreach (var c in s.ToUpperInvariant())
            {
                int d = Digits.IndexOf(c);
                if (d < 0) throw new FormatException();
                v = checked(v * 36 + d);
            }
            return v;
        }

        internal static string ToBase36(long v)
        {
            var s = "";
            do { s = Digits[(int)(v % 36)] + s; v /= 36; } while (v > 0);
            return s;
        }

        internal static object Call(string method, object arg) => AccessTools.Method(Packer, method).Invoke(null, new[] { arg });
        internal static char[] Delimiters => (char[])AccessTools.Field(Packer, "Delimeters").GetValue(null);
    }

    [HarmonyPatch]
    internal static class LI_PackCode_Fix
    {
        static MethodBase TargetMethod() => AccessTools.Method(LobbyCode.Packer, "Obfuscate");
        static bool Prepare() => LobbyCode.Packer != null && TargetMethod() != null;

        static bool Prefix(string code, ref string __result)
        {
            var parts = code.Split(':');
            if (parts.Length < 2) return true;   // not in a room: the original fails as it always did
            var region = (int)LobbyCode.Call("GetRegionNumber", parts[0]);
            var delimiters = LobbyCode.Delimiters;
            __result = (string)LobbyCode.Call("LongToBase", (long)region) + delimiters[UnityEngine.Random.Range(0, delimiters.Length)]
                       + (string)LobbyCode.Call("LongToBase", LobbyCode.FromBase36(parts[1]));
            return false;
        }
    }

    [HarmonyPatch]
    internal static class LI_UnpackCode_Fix
    {
        static MethodBase TargetMethod() => AccessTools.Method(LobbyCode.Packer, "DeObfuscate");
        static bool Prepare() => LobbyCode.Packer != null && TargetMethod() != null;

        static bool Prefix(string code, ref string __result)
        {
            var parts = code.Split(LobbyCode.Delimiters);
            if (parts.Length != 2) throw new FormatException();   // as the original: the join box says the code is invalid
            var region = (int)(long)LobbyCode.Call("BaseToLong", parts[0]);
            var room = (long)LobbyCode.Call("BaseToLong", parts[1]);
            __result = (string)LobbyCode.Call("GetRegionCode", region) + ":" + LobbyCode.ToBase36(room);
            return false;
        }
    }

    [HarmonyPatch]
    internal static class LI_JoinLetterRooms_Fix
    {
        static readonly MethodInfo IsDigit = AccessTools.Method(typeof(char), nameof(char.IsDigit), new[] { typeof(char) });
        static readonly MethodInfo IsLetterOrDigit = AccessTools.Method(typeof(char), nameof(char.IsLetterOrDigit), new[] { typeof(char) });

        static MethodBase TargetMethod() => AccessTools.Method(LobbyCode.Handler, "PureConnectToRoom");
        static bool Prepare() => LobbyCode.Handler != null && TargetMethod() != null;

        // the room-name check is `name.All(char.IsDigit)`
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code)
        {
            foreach (var i in code)
            {
                if (i.opcode == OpCodes.Ldftn && i.operand is MethodInfo m && m == IsDigit) i.operand = IsLetterOrDigit;
                yield return i;
            }
        }
    }
}
