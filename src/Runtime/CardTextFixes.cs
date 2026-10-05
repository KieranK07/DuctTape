using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
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

    // RSCards, Root's packs and other mods bring their own card frame (a CardInfoDisplayer saved before the 2025 build).
    // It has the old name, description and stat texts and none of the localized ones the current DrawCard fills, so
    // their cards showed art and no text. Such a frame is drawn as the old game drew it.
    [HarmonyPatch(typeof(CardInfoDisplayer), nameof(CardInfoDisplayer.DrawCard))]
    internal static class OldCardFrame_Fix
    {
        static bool IsOld(CardInfoDisplayer d) =>
            d.nameText && (!d.m_localizedNameText
                           || d.statObject && d.statObject.transform.childCount > 0 && !d.statObject.transform.GetChild(0).GetComponent<UILocalizedString>());

        [HarmonyPriority(Priority.First)]
        static bool Prefix(CardInfoDisplayer __instance, CardInfoStat[] stats, LocalizedString cardName, LocalizedString description, Sprite image, bool charge)
        {
            var d = __instance;
            if (!IsOld(d)) return true;
            if (charge && d.chargeObj)
            {
                d.chargeObj.SetActive(true);
                d.chargeObj.transform.SetParent(d.grid.transform, true);
            }
            var effect = Text(description);
            if (effect != "" && d.effectText)
            {
                d.effectText.text = effect;
                d.effectText.gameObject.SetActive(true);
                d.effectText.transform.SetParent(d.grid.transform, true);
            }
            d.nameText.text = Text(cardName).ToUpper();
            bool numbers = Optionshandler.instance.OptionsData.GetSettingsData("OPTION_SHOWSTATNUMBERS").CurrentValueToggle;
            foreach (var s in stats ?? Array.Empty<CardInfoStat>())
            {
                var row = UnityEngine.Object.Instantiate(d.statObject, d.grid.transform.position, d.grid.transform.rotation, d.grid.transform);
                row.SetActive(true);
                row.transform.localScale = Vector3.one;
                var value = row.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                row.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.IsNullOrEmpty(s.stat) ? Text(s.LocalizedStat) : s.stat;
                value.text = s.simepleAmount != 0 && !numbers ? s.GetSimpleAmount() : s.amount;
                value.color = s.positive ? d.positiveColor : d.negativeColor;
            }
            if (image && d.icon) d.icon.sprite = image;
            if (d.effectText) d.effectText.transform.position += Vector3.up * 0.3f;
            return false;
        }

        // mod text is the key of a missing entry, which MissingText shows as is
        static string Text(LocalizedString s) => s == null || s.IsEmpty ? "" : s.GetLocalizedString() ?? "";
    }
}
