using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace RoundsPort.Runtime
{
    // LobbyImprovements builds its lobby code box from the main menu's font, read off the "Local" button's text before
    // that text has ever been drawn. The current game's TextMeshPro throws there (no text info until it's drawn), so the
    // box and the controls sorted after it were never made; the old game answered. A text that hasn't been drawn has
    // one material: its font's.
    [HarmonyPatch(typeof(TextMeshProUGUI), "GetMaterials")]
    internal static class UndrawnTextMaterials_Fix
    {
        static readonly AccessTools.FieldRef<TMP_Text, TMP_TextInfo> TextInfo = AccessTools.FieldRefAccess<TMP_Text, TMP_TextInfo>("m_textInfo");

        static Exception Finalizer(Exception __exception, TextMeshProUGUI __instance, ref Material[] __result)
        {
            if (__exception is not NullReferenceException || __instance == null || TextInfo(__instance) != null) return __exception;
            __result = new[] { __instance.fontSharedMaterial };
            return null;
        }
    }
}
