using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoTranslator.Translation
{
    public interface ITranslationSource
    {
        string Name { get; }
        bool SourceTranslationAvailable { get; }

        bool TryGetTranslation(string Key, out string Translation);

        void Reload();
    }
}
