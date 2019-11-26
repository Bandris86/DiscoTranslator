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
        
        private IDictionary<string, POTranslationEntry> keyTranslationData;
        private IDictionary<string, string> sourceTranslationData;

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
            SourceTranslationAvailable = catalog.Count < 1000;

            keyTranslationData = new Dictionary<string, POTranslationEntry>();
            sourceTranslationData = new Dictionary<string, string>();

            for (int i = 0; i < catalog.Count; i++)
            {
                var entry = catalog[i];

                if (entry is POSingularEntry sEntry)
                {
                    string translation;

                    if (!string.IsNullOrWhiteSpace(sEntry.Translation))
                    {
                        translation = sEntry.Translation;
                        if (SourceTranslationAvailable)
                            sourceTranslationData[sEntry.Key.Id.ToLower().Trim(' ')] = sEntry.Translation;
                    }
                    else if (LoadUntranslated)
                        translation = sEntry.Key.Id;
                    else
                        continue;

                    keyTranslationData.Add(sEntry.Key.ContextId, new POTranslationEntry(translation, i+1));
                }
            }
        }

        public bool TryGetTranslationByKey(string Key, out string Translation)
        {
            if (keyTranslationData.TryGetValue(Key, out var TranslationEntry))
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

        public bool TryGetTranslationBySource(string Source, out string Translation)
        {
            return sourceTranslationData.TryGetValue(Source, out Translation);
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
