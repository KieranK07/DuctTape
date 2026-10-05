using HarmonyLib;
using UnityEngine.Localization;

namespace RoundsPort.Runtime
{
    // Cards saved in an asset bundle before the 2025 build and registered as they are (CustomCard.RegisterUnityCard:
    // RSCards and other prefab cards) carry their title and description only in cardName / cardDestription. The 2025
    // build draws both from localization (m_localizedCardName / m_localizedCardDescription), which these cards were
    // saved without, so they showed no description. A card that has old text and no localized text gets what UnboundLib
    // 4 gives the cards it builds itself: a LocalizedString with the text as its key, which MissingText shows as is.
    // Only instances are changed (Awake runs on the copy being drawn), so nothing needs undoing.
    [HarmonyPatch(typeof(CardInfo), "Awake")]
    internal static class PrefabCardText_Fix
    {
        static readonly AccessTools.FieldRef<CardInfo, string> Name = AccessTools.FieldRefAccess<CardInfo, string>("cardName");
        static readonly AccessTools.FieldRef<CardInfo, string> Description = AccessTools.FieldRefAccess<CardInfo, string>("cardDestription");
        static readonly AccessTools.FieldRef<CardInfo, LocalizedString> LocalizedName = AccessTools.FieldRefAccess<CardInfo, LocalizedString>("m_localizedCardName");
        static readonly AccessTools.FieldRef<CardInfo, LocalizedString> LocalizedDescription = AccessTools.FieldRefAccess<CardInfo, LocalizedString>("m_localizedCardDescription");

        static void Prefix(CardInfo __instance)
        {
            if (__instance == null) return;
            Fill(ref LocalizedName(__instance), Name(__instance));
            Fill(ref LocalizedDescription(__instance), Description(__instance));
        }

        static void Fill(ref LocalizedString localized, string text)
        {
            if (string.IsNullOrEmpty(text) || (localized != null && !localized.IsEmpty)) return;
            localized = new LocalizedString("StringTableCards", text);
        }
    }
}
