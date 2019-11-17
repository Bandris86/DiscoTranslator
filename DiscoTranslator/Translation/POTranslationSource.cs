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
        
        private IDictionary<string, string> keyTranslationData;
        private IDictionary<string, string> sourceTranslationData;

        public POTranslationSource(string poFilePath)
        {
            POFilePath = poFilePath;
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

            keyTranslationData = new Dictionary<string, string>();
            sourceTranslationData = new Dictionary<string, string>();

            foreach (var entry in catalog)
            {
                if (entry is POSingularEntry sEntry && !string.IsNullOrWhiteSpace(sEntry.Translation))
                {
                    keyTranslationData.Add(sEntry.Key.ContextId, sEntry.Translation);
                    if (SourceTranslationAvailable)
                        sourceTranslationData[sEntry.Key.Id.ToLower().Trim(' ')] = sEntry.Translation;
                }
            }
        }

        public bool TryGetTranslationByKey(string Key, out string Translation)
        {
            return keyTranslationData.TryGetValue(Key, out Translation);
        }

        public bool TryGetTranslationBySource(string Source, out string Translation)
        {
            return sourceTranslationData.TryGetValue(Source, out Translation);
        }
    }
}
