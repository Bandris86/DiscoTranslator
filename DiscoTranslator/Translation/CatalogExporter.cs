using Karambolo.PO;
using PixelCrushers.DialogueSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DiscoTranslator.Translation
{
    public static class CatalogExporter
    {
        private static readonly HashSet<string> ConversationTranslatableFields =
            new HashSet<string>(new string[] { "Title", "Description", "subtask_title_01", "subtask_title_02",
                "subtask_title_03", "subtask_title_04", "subtask_title_05", "subtask_title_06" });

        private static readonly HashSet<string> DialogueTranslatableFields =
            new HashSet<string>(new string[] { "Alternate1", "Alternate2", "Alternate3", "Alternate4", "Dialogue Text", "tooltip1",
                "tooltip10", "tooltip2", "tooltip3", "tooltip4", "tooltip5", "tooltip6", "tooltip7", "tooltip8", "tooltip9" });

        private static readonly string[] DIALOGUE_SYSTEM_SOURCE_PATHS = new string[]
        {
            "Languages/DialogueLanguages",
            "Languages/ConversationLanguages",
            "Languages/ActorsLanguages"
        };

        public static void ExportAll(string directory)
        {
            var languageSources = new List<I2.Loc.LanguageSourceAsset>(Resources.FindObjectsOfTypeAll<I2.Loc.LanguageSourceAsset>());
            var gen = new POGenerator();

            // Need to manually load some LanguageSourceAsset
            // foreach (var path in DIALOGUE_SYSTEM_SOURCE_PATHS)
            //     languageSources.Add(Resources.Load<I2.Loc.LanguageSourceAsset>(path));

            foreach (var source in languageSources)
            {
                var fullName = source.name;
                var shortName = fullName.Replace("Languages", "");
                POCatalog catalog = null;
                if (shortName == "Dialogue")
                    continue;

                catalog = LanguageSourceToCatalog(source, 0);

                var catalogPath = Path.Combine(directory, shortName + ".pot");

                using (var file = File.Create(catalogPath))
                using (var writer = new StreamWriter(file))
                {
                    gen.Generate(writer, catalog);
                }
            }

            // Manually generate dialogue catalog for additional infos
            // Actor, conversant, conversation context

            var db = Resources.FindObjectsOfTypeAll<DialogueDatabase>()[0];

            var dialogueCatalog = GetDialogueCatalog(db);
            var dialogueCatalogPath = Path.Combine(directory, "Dialogue.pot");

            using (var file = File.Create(dialogueCatalogPath))
            using (var writer = new StreamWriter(file))
            {
                gen.Generate(writer, dialogueCatalog);
            }
        }

        public static POCatalog LanguageSourceToCatalog(I2.Loc.LanguageSourceAsset languageSource, int languageIndex)
        {
            var catalog = InitCatalog();

            foreach (var term in languageSource.mSource.mTerms)
            {
                string key = term.Term;
                string source = term.Languages[languageIndex];

                if (string.IsNullOrEmpty(source))
                    continue;

                var entry = new POSingularEntry(new POKey(source, contextId: key));
                catalog.Add(entry);
            }

            return catalog;
        }

        public static IDictionary<int, Actor> GetActors(DialogueDatabase db)
        {
            var ret = new Dictionary<int, Actor>();

            foreach (var actor in db.actors)
            {
                ret[actor.id] = actor;
            }
            return ret;
        }
        
        public static POCatalog GetDialogueCatalog(this DialogueDatabase db)
        {
            var actors = GetActors(db);

            var catalog = InitCatalog();

            foreach (var conversation in db.conversations)
            {
                foreach (var dialogue in conversation.dialogueEntries)
                {
                    foreach (var field in dialogue.fields)
                    {
                        if (string.IsNullOrWhiteSpace(field.value))
                            continue;
                        if (!DialogueTranslatableFields.Contains(field.title))
                            continue;

                        string key = $"{field.title}/{Field.LookupValue(dialogue.fields, "Articy Id")}";
                        string source = field.value;

                        var entry = new POSingularEntry(new POKey(source, contextId: key))
                        {
                            Comments = new List<POComment>()
                        };

                        entry.Comments.Add(new POTranslatorComment { Text = $"Title = {conversation.Title}" });
                        entry.Comments.Add(new POTranslatorComment { Text = $"Description = {conversation.Description.Replace("\n", "\\n")}" });
                        if (actors.TryGetValue(dialogue.ActorID, out Actor actor))
                            entry.Comments.Add(new POTranslatorComment { Text = $"Actor = {actor.Name}" });
                        if (actors.TryGetValue(dialogue.ConversantID, out Actor conversant))
                            entry.Comments.Add(new POTranslatorComment { Text = $"Conversant = {conversant.Name}" });

                        catalog.Add(entry);
                    }
                }
            }

            return catalog;
        }

        public static POCatalog InitCatalog()
        {
            return new POCatalog
            {
                Encoding = "UTF-8",
                Language = "en_US"
            };
        }
    }
}
