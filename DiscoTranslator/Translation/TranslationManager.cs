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

        public static bool TryGetTranslation(string Key, out string Translation)
        {
            Translation = null;
            if (!EnableTranslation)
                return false;

            foreach (var source in Sources)
                if (source.TryGetTranslation(Key, out Translation))
                    return true;

            return false;
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
