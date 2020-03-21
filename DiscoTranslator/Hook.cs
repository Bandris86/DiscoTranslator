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
    }
}
