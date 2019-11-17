using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoTranslator.Translation
{
    public static class TranslationManager
    {
        public static event Action<BepInEx.Logging.LogLevel, string> LogEvent;

        public static bool EnableTranslation { get; set; } = true;

        public static void Log(BepInEx.Logging.LogLevel level, string message)
        {
            LogEvent?.Invoke(level, message);
        }

        public static bool TryGetTranslationByKey(string Key, out string Translation)
        {
            Translation = null;
            if (!EnableTranslation)
                return false;

            foreach (var source in Sources)
            {
                if (source.TryGetTranslationByKey(Key, out string Tr))
                {
                    Translation = Tr;
                    break;
                }
            }
            return Translation != null;
        }

        public static bool TryGetTranslationBySource(string Source, out string Translation)
        {
            Translation = null;
            if (!EnableTranslation)
                return false;

            foreach (var source in Sources)
            {
                if (source.SourceTranslationAvailable && source.TryGetTranslationBySource(Source, out string Tr))
                {
                    Translation = Tr;
                    break;
                }
            }
            return Translation != null;
        }

        public static void AddSource(ITranslationSource source)
        {
            if (Sources.Contains(source))
                return;

            Sources.Add(source);
        }

        public static void ReloadAllSources()
        {
            foreach (var source in Sources)
            {
                source.Reload();
            }
        }

        public static void RemoveAllSources()
        {
            Sources.Clear();
        }

        private static readonly IList<ITranslationSource> Sources = new List<ITranslationSource>();
    }
}
