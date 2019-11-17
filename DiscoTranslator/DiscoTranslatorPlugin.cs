using BepInEx;
using BepInEx.Configuration;
using DiscoTranslator.Translation;
using LocalizationCustomSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DiscoTranslator
{
    [BepInPlugin("akintos.DiscoTranslator", "Disco Translator", "0.1.0.0")]
    [BepInProcess("disco.exe")]
    public class DiscoTranslatorPlugin : BaseUnityPlugin
    {
        private const string PLUGIN_DIR = "DiscoTranslator";

        ConfigEntry<KeyboardShortcut> toggleKey, reloadKey, exportKey;

        public DiscoTranslatorPlugin() : base()
        {
            var harmony = BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hook));
            TranslationManager.LogEvent += Logger.Log;

            var method = harmony.GetPatchedMethods().FirstOrDefault();
            Logger.LogDebug(method.Name);

            toggleKey = Config.Bind("Hotkeys", "Toggle translation", new KeyboardShortcut(KeyCode.T, KeyCode.LeftAlt));
            reloadKey = Config.Bind("Hotkeys", "Reload translation", new KeyboardShortcut(KeyCode.R, KeyCode.LeftAlt));

            exportKey = Config.Bind("Hotkeys", "Export translation catalog", new KeyboardShortcut(KeyCode.E, KeyCode.LeftAlt));
        }

        void Awake()
        {
            LoadTranslation();
        }

        void Start()
        {

        }

        void Update()
        {
            if (reloadKey.Value.IsDown())
            {
                TranslationManager.ReloadAllSources();
                Logger.LogInfo("Reload sources");
            }

            if (toggleKey.Value.IsDown())
            {
                TranslationManager.EnableTranslation = !TranslationManager.EnableTranslation;
                Logger.LogInfo("Toggle translation");
            }


            if (exportKey.Value.IsDown())
            {
                string catalogue_dir = BepInEx.Utility.CombinePaths(Paths.PluginPath, PLUGIN_DIR, "Catalog");
                if (!Directory.Exists(catalogue_dir))
                    Directory.CreateDirectory(catalogue_dir);

                CatalogExporter.ExportAll(catalogue_dir);
                Logger.LogInfo("Export translation");
            }
        }
        
        void LoadTranslation()
        {
            string translation_dir = BepInEx.Utility.CombinePaths(Paths.PluginPath, PLUGIN_DIR, "Translation");
            if (!Directory.Exists(translation_dir))
            {
                Logger.LogError("Translation directory does not exist : " + translation_dir);
                return;
            }

            foreach (var filePath in Directory.GetFiles(translation_dir))
            {
                var extension = Path.GetExtension(filePath);

                if (extension == ".po")
                {
                    var source = new POTranslationSource(filePath);
                    TranslationManager.AddSource(source);
                    Logger.LogInfo("Added translation source " + Path.GetFileName(filePath));
                }
            }
        }
    }
}
