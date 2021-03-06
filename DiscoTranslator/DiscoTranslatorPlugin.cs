﻿using BepInEx;
using BepInEx.Configuration;
using DiscoTranslator.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace DiscoTranslator
{
    [BepInPlugin("akintos.DiscoTranslator", "Disco Translator", "0.2.0.0")]
    [BepInProcess("disco.exe")]
    public class DiscoTranslatorPlugin : BaseUnityPlugin
    {
        private const string PLUGIN_DIR = "DiscoTranslator";

        ConfigEntry<KeyboardShortcut> toggleKey, reloadKey;

        public DiscoTranslatorPlugin() : base()
        {
            toggleKey = Config.Bind("Hotkeys", "Toggle interface", new KeyboardShortcut(KeyCode.T, KeyCode.LeftAlt));
            reloadKey = Config.Bind("Hotkeys", "Reload translation", new KeyboardShortcut(KeyCode.R, KeyCode.LeftAlt));

            var harmony = BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hook), "akintos.DiscoTranslator");
            BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Patches.ConversationLoggerPatch), harmony);

            TranslationManager.LogEvent += Logger.Log;
        }

        void Awake()
        {
            LoadTranslation();
            ImageManager.LoadImages(Path.Combine(Paths.PluginPath, PLUGIN_DIR, "Images"));
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
                showingUI = !showingUI;
            }
        }

        private void ExportCatalog()
        {
            string catalog_dir = BepInEx.Utility.CombinePaths(Paths.PluginPath, PLUGIN_DIR, "Catalog");
            if (!Directory.Exists(catalog_dir))
                Directory.CreateDirectory(catalog_dir);

            CatalogExporter.ExportAll(catalog_dir);
            Logger.LogInfo("Export translation");
        }

        private void LoadTranslation()
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
                    POTranslationSource source = new POTranslationSource(filePath, true);
                    if (Path.GetFileNameWithoutExtension(filePath).ToLower().Contains("dialog"))
                    {
                        source.EnableStrNo = true;
                        source.StrNoPrefix = "D";
                    }
                    TranslationManager.AddSource(source);
                    Logger.LogInfo("Added translation source " + Path.GetFileName(filePath));
                }
            }
        }

        #region UI
        private Rect UI = CalculateWindowRect(300, 300, 0.15f, 0.2f);
        private bool prettyPrint = true;

        bool showingUI = false;
        void OnGUI()
        {
            if (showingUI)
                UI = GUI.Window(this.Info.Metadata.Name.GetHashCode(), UI, WindowFunction, this.Info.Metadata.Name);
        }

        void WindowFunction(int windowID)
        {
            TranslationManager.EnableTranslation = GUILayout.Toggle(TranslationManager.EnableTranslation, "Enable translation");

            if (GUILayout.Button("Reload translations"))
                TranslationManager.ReloadAllSources();

            if (GUILayout.Button("Export catalog"))
                ExportCatalog();

            GUILayout.Space(15);
            
            if (GUILayout.Button("Export dialogue database"))
            {
                var db = Resources.FindObjectsOfTypeAll<PixelCrushers.DialogueSystem.DialogueDatabase>()[0];
                var json = UnityEngine.JsonUtility.ToJson(db, prettyPrint);
                File.WriteAllText(BepInEx.Utility.CombinePaths(Paths.PluginPath, PLUGIN_DIR, "database.json"), json);
            }
            prettyPrint = GUILayout.Toggle(prettyPrint, "Pretty print(format)  JSON output");
            GUILayout.Space(15);

            if (GUILayout.Button("Export images"))
                ImageManager.ExportImages(Path.Combine(Paths.PluginPath, PLUGIN_DIR, "OriginalImages"));

            GUILayout.Space(15);
            GUILayout.Label($"Press {toggleKey.Value.ToString()} to close this window.");
            GUI.DragWindow();
        }

        private static Rect CalculateWindowRect(int width, int height, float xpos, float ypos)
        {
            var offsetX = Mathf.RoundToInt(Screen.width * xpos - width / 2);
            var offsetY = Mathf.RoundToInt(Screen.height * ypos - height /2);
            return new Rect(offsetX, offsetY, width, height);
        }
        #endregion
    }
}
