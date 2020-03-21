using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Karambolo.PO;

namespace DiscoTranslator.Translation
{
    public class POTranslationSource : ITranslationSource
    {
        public string POFilePath { get; set; }

        public string Name { get; private set; } = string.Empty;
        public bool SourceTranslationAvailable { get; private set; } = false;

        public bool LoadUntranslated { get; set; }

        public bool EnableStrNo { get; set; } = false;
        public string StrNoPrefix { get; set; } = string.Empty;
        
        private IDictionary<string, POTranslationEntry> TranslationData;

        public POTranslationSource(string poFilePath, bool loadUntranslated)
        {
            POFilePath = poFilePath;
            LoadUntranslated = loadUntranslated;
            Name = Path.GetFileNameWithoutExtension(poFilePath).ToLower();
            LoadPOFile();
        }

        public void Reload()
        {
            LoadPOFile();
        }

        private void LoadPOFile()
        {
            var parser = new POParser();

            POParseResult result = null;

            using (var file = File.OpenRead(POFilePath))
            {
                result = parser.Parse(file);
            }

            if (!result.Success)
                throw new Exception($"Failed to parse po file {POFilePath}");

            var catalog = result.Catalog;

            TranslationData = new Dictionary<string, POTranslationEntry>();

            for (int i = 0; i < catalog.Count; i++)
            {
                var entry = catalog[i];

                if (entry is POSingularEntry sEntry)
                {
                    string translation;

                    if (!string.IsNullOrWhiteSpace(sEntry.Translation))
                    {
                        translation = sEntry.Translation;
                    }
                    else if (LoadUntranslated)
                        translation = sEntry.Key.Id;
                    else
                        continue;

                    TranslationData.Add(sEntry.Key.ContextId, new POTranslationEntry(translation, i+1));
                }
            }
        }

        public bool TryGetTranslation(string Key, out string Translation)
        {
            if (TranslationData.TryGetValue(Key, out var TranslationEntry))
            {
                if (EnableStrNo)
                    Translation = string.Format("{0}{1}:{2}", StrNoPrefix, TranslationEntry.StrNo, TranslationEntry.Translation);
                else
                    Translation = TranslationEntry.Translation;

                return true;
            }

            Translation = null;
            return false;
        }

        private class POTranslationEntry
        {
            public readonly string Translation;
            public readonly int StrNo;

            public POTranslationEntry(string translation, int strNo)
            {
                Translation = translation;
                StrNo = strNo;
            }
        }
    }
}
