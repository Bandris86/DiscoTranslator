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
        private static readonly HashSet<string> translatableFields =
            new HashSet<string>(new string[] { "Alternate1", "Alternate2", "Alternate3", "Alternate4", "Dialogue Text", "tooltip1",
                "tooltip10", "tooltip2", "tooltip3", "tooltip4", "tooltip5", "tooltip6", "tooltip7", "tooltip8", "tooltip9" });

        public static void ExportAll(string directory)
        {
            var languageSources = Resources.FindObjectsOfTypeAll<I2.Loc.LanguageSourceAsset>();
            var gen = new POGenerator();

            foreach (var source in languageSources)
            {
                var fullName = source.name;
                var shortName = fullName.Replace("Languages", "");
                POCatalog catalog = null;
                if (shortName == "Dialogue")
                    continue;

                catalog = LanguageSourceToCatalog(source);

                var catalogPath = BepInEx.Utility.CombinePaths(directory, shortName + ".pot");

                using (var file = File.Create(catalogPath))
                using (var writer = new StreamWriter(file))
                {
                    gen.Generate(writer, catalog);
                }
            }

            var db = Resources.FindObjectsOfTypeAll<DialogueDatabase>()[0];
            var dbCatalog = DialogueDatabaseToCatalog(db);
            var dbCatalogPath = BepInEx.Utility.CombinePaths(directory, "Dialogue.pot");

            using (var file = File.Create(dbCatalogPath))
            using (var writer = new StreamWriter(file))
            {
                gen.Generate(writer, dbCatalog);
            }
        }

        public static POCatalog LanguageSourceToCatalog(I2.Loc.LanguageSourceAsset languageSource)
        {
            var catalog = new POCatalog
            {
                Encoding = "UTF-8",
                Language = "en_US"
            };

            foreach (var term in languageSource.mSource.mTerms)
            {
                string key = term.Term;
                string source = term.Languages[0];

                if (string.IsNullOrEmpty(source))
                    continue;

                var entry = new POSingularEntry(new POKey(source, contextId: key));
                catalog.Add(entry);
            }

            return catalog;
        }

        public static IDictionary<int, string> GetActors(DialogueDatabase db)
        {
            var ret = new Dictionary<int, string>();

            foreach (var actor in db.actors)
            {
                ret[actor.id] = actor.Name;
            }
            return ret;
        }

        public static POCatalog DialogueDatabaseToCatalog(DialogueDatabase db)
        {
            var actors = GetActors(db);

            var catalog = new POCatalog
            {
                Encoding = "UTF-8",
                Language = "en_US"
            };

            foreach (var conversation in db.conversations)
            {
                foreach (var dialogue in conversation.dialogueEntries)
                {
                    foreach (var field in dialogue.fields)
                    {
                        if (!translatableFields.Contains(field.title))
                            continue;
                        if (string.IsNullOrEmpty(field.value))
                            continue;

                        string key = $"Dialogue System/Conversation/{dialogue.conversationID}/Entry/{dialogue.id}/{field.title}";
                        string source = field.value;
                        bool hasActor = actors.TryGetValue(dialogue.ActorID, out string actor);
                        bool hasConversant = actors.TryGetValue(dialogue.ConversantID, out string conversant);

                        var entry = new POSingularEntry(new POKey(source, contextId: key));
                        entry.Comments = new POComment[]
                        {
                            new POTranslatorComment { Text = $"Title = {conversation.Title}" },
                            new POTranslatorComment { Text = $"Description = {conversation.Description.Replace("\n", "\\n")}" },
                            new POTranslatorComment { Text = $"Actor = {actor}" },
                            new POTranslatorComment { Text = $"Conversant = {conversant}" }
                        };

                        catalog.Add(entry);
                    }
                }
            }

            return catalog;
        }
    }
}
