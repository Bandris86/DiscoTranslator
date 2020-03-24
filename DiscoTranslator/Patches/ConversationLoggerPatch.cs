using HarmonyLib;
using LocalizationCustomSystem;
using PixelCrushers.DialogueSystem;
using Sunshine.Metric;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoTranslator.Patches
{
    /// <summary>
    /// Patch to bypass LocalizationManager.GetCurrentLanguageName() == "English" check which prevents Localized dialog to load
    /// </summary>
    public static class ConversationLoggerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Sunshine.ConversationLogger), nameof(Sunshine.ConversationLogger.AssembleFinalEntry))]
        static bool AssembleFinalEntryPrefix(DialogueEntry entry, CheckResult checkResult, bool isLocalizationTrigger, ref FinalEntry __result)
        {
            FinalEntry finalEntry;
            if (JanusNode.IsJanusNode(entry))
            {
                finalEntry = JanusNode.HandleEntry(entry);
            }
            else if (WhiteCheckNode.IsWhiteCheckNode(entry))
            {
                finalEntry = ((checkResult == null) ? CheckNodeUtil.HandleEntry(entry) : CheckNodeUtil.HandleEntry(entry, checkResult));
            }
            else if (RedCheckNode.IsRedCheckNode(entry))
            {
                finalEntry = ((checkResult == null) ? CheckNodeUtil.HandleEntry(entry) : CheckNodeUtil.HandleEntry(entry, checkResult));
            }
            else if (PassiveNode.IsPassiveNode(entry))
            {
                finalEntry = PassiveNode.HandleEntry(entry, true);
            }
            else if (CostOptionNode.IsCostOptionNode(entry))
            {
                finalEntry = CostOptionNode.HandleEntry(entry, isLocalizationTrigger);
            }
            else if (KimSwitchNode.IsKimSwitchNode(entry))
            {
                string localizedTerm = LocalizationManager.GetLocalizedTerm(LocalizationUtils.GetDialogueEntryLocalizationTerm(entry));
                string spokenLine = (localizedTerm == null || localizedTerm.Equals(string.Empty)) ? entry.subtitleText : LocalizationUtils.FormatLocalizedDialogueText(localizedTerm);
                KimSwitchNode.HandleEntry(entry, isLocalizationTrigger);
                finalEntry = new FinalEntry(entry, spokenLine, FinalEntry.GetSpeakerName(entry));
            }
            else if (FakeCheckNode.IsFakeCheckNode(entry))
            {
                finalEntry = CheckNodeUtil.HandleEntry(entry);
            }
            else if (checkResult != null)
            {
                finalEntry = CheckNodeUtil.HandleEntry(entry, checkResult);
                string localizedTerm2 = LocalizationManager.GetLocalizedTerm(LocalizationUtils.GetDialogueEntryLocalizationTerm(entry));
                string spokenLine2 = (localizedTerm2 == null || localizedTerm2.Equals(string.Empty)) ? entry.subtitleText : LocalizationUtils.FormatLocalizedDialogueText(localizedTerm2);
                finalEntry.spokenLine = spokenLine2;
            }
            else
            {
                string localizedTerm3 = LocalizationManager.GetLocalizedTerm(LocalizationUtils.GetDialogueEntryLocalizationTerm(entry));
                string spokenLine3 = (localizedTerm3 == null || localizedTerm3.Equals(string.Empty)) ? entry.subtitleText : LocalizationUtils.FormatLocalizedDialogueText(localizedTerm3);
                finalEntry = new FinalEntry(entry, spokenLine3, FinalEntry.GetSpeakerName(entry));
            }

            __result = finalEntry;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Sunshine.ConversationLogger), nameof(Sunshine.ConversationLogger.ChooseResponseText))]
        public static bool ChooseResponseTextPrefix(Response response, ref FinalResponseText __result)
        {
            DialogueEntry destinationEntry = response.destinationEntry;
            if (JanusNode.IsJanusNode(destinationEntry))
            {
                __result = JanusNode.HandleResponseText(response);
            }
            else if (WhiteCheckNode.IsWhiteCheckNode(destinationEntry))
            {
                __result = WhiteCheckNode.HandleResponseText(response);
            }
            else if (RedCheckNode.IsRedCheckNode(destinationEntry))
            {
                __result = RedCheckNode.HandleResponseText(response);
            }
            else if (PassiveNode.IsPassiveNode(destinationEntry))
            {
                __result = PassiveNode.HandleResponseText(response);
            }
            else if (CostOptionNode.IsCostOptionNode(destinationEntry))
            {
                __result = CostOptionNode.HandleResponseText(response);
            }
            else if (FakeCheckNode.IsFakeCheckNode(destinationEntry))
            {
                __result = FakeCheckNode.HandleResponseText(response);
            }
            else
            {
                string localizedTerm = LocalizationManager.GetLocalizedTerm(LocalizationUtils.GetDialogueEntryLocalizationTerm(destinationEntry));
                string responseText = (localizedTerm == null || localizedTerm.Equals(string.Empty)) ? destinationEntry.subtitleText : LocalizationUtils.FormatLocalizedDialogueText(localizedTerm);
                __result = new FinalResponseText(response, responseText);
            }

            return false;
        }
    }
}
