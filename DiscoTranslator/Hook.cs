using DiscoTranslator.Translation;
using HarmonyLib;
using LocalizationCustomSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DiscoTranslator
{
    public static class Hook
    {
        private static readonly Dictionary<string, string> UITranslation = new Dictionary<string, string>();
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(I2.Loc.LocalizationManager), nameof(I2.Loc.LocalizationManager.GetTermTranslation))]
        static bool GetTermTranslationPrefix(string Term, ref string __result)
        {
            if (TranslationManager.TryGetTranslationByKey(Term, out string Translation))
            {
                __result = Translation;
                return false;
            }

            return true;
        }

        public static bool ContainsHangul(string s)
        {
            foreach (var c in s)
            {
                if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                    return true;
            }
            return false;
        }

        static bool HandleUITranslation(ref string value)
        {
            if (value == null) return false;
            if (ContainsHangul(value)) return true;

            if (UITranslation.TryGetValue(value, out string cache))
            {
                value = cache;
                return true;
            }

            if (TranslationManager.TryGetTranslationBySource(value.ToLower(), out string Translation))
            {
                UITranslation[value] = Translation;
                value = Translation;
            }
            else
            {
                UITranslation[value] = value;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnityEngine.UI.Text), "text", MethodType.Setter)]
        static bool UnityEngine_UI_Text_SetTextPrefix(ref string value)
        {
            return HandleUITranslation(ref value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnityEngine.UI.Text), "OnEnable")]
        static bool UnityEngine_UI_Text_OnEnable_Prefix(ref string ___m_Text)
        {
            return HandleUITranslation(ref ___m_Text);
        }
    }
}
