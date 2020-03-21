using DiscoTranslator.Translation;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DiscoTranslator
{
    public static class Hook
    {
        public static bool EnableImageHook = true;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(I2.Loc.LocalizationManager), nameof(I2.Loc.LocalizationManager.GetTranslation))]
        static bool GetTermTranslationPrefix(string Term, ref string __result)
        {
            if (TranslationManager.TryGetTranslation(Term, out string Translation))
            {
                __result = Translation;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(I2.Loc.LocalizationManager), nameof(I2.Loc.LocalizationManager.FindAsset))]
        static bool FindAssetPrefix(string value, ref object __result)
        {
            if (!EnableImageHook) return true;

            if (ImageManager.TryGetImage(value, out UnityEngine.Sprite sprite))
            {
                __result = sprite;
                return false;
            }

            return true;
        }
    }
}
