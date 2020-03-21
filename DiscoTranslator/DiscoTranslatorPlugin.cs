using BepInEx;
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
            var harmony = BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hook));
            TranslationManager.LogEvent += Logger.Log;

            toggleKey = Config.Bind("Hotkeys", "Toggle interface", new KeyboardShortcut(KeyCode.T, KeyCode.LeftAlt));
            reloadKey = Config.Bind("Hotkeys", "Reload translation", new KeyboardShortcut(KeyCode.R, KeyCode.LeftAlt));
        }

        void Awake()
        {
            LoadTranslation();
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

        private void ExportButtonImages()
        {
            var src = I2.Loc.LocalizationManager.Sources.Find(x => x.ownerObject.name == "ButtonsImagesLanguages");

            int engIndex = src.GetLanguageIndex("English");

            foreach (var term in src.mTerms)
            {
                var imageName = term.Languages[engIndex];
                var texture = (src.FindAsset(imageName) as Sprite).texture;

                // From https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-

                // Create a temporary RenderTexture of the same size as the texture
                RenderTexture tmp = RenderTexture.GetTemporary(
                                    texture.width,
                                    texture.height,
                                    0,
                                    RenderTextureFormat.Default,
                                    RenderTextureReadWrite.Linear);

                // Blit the pixels on texture to the RenderTexture
                Graphics.Blit(texture, tmp);
                // Backup the currently set RenderTexture
                RenderTexture previous = RenderTexture.active;
                // Set the current RenderTexture to the temporary one we created
                RenderTexture.active = tmp;
                // Create a new readable Texture2D to copy the pixels to it
                Texture2D myTexture2D = new Texture2D(texture.width, texture.height);
                // Copy the pixels from the RenderTexture to the new Texture
                myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                myTexture2D.Apply();
                // Reset the active RenderTexture
                RenderTexture.active = previous;
                // Release the temporary RenderTexture
                RenderTexture.ReleaseTemporary(tmp);

                var pngData = ImageConversion.EncodeToPNG(myTexture2D);

                File.WriteAllBytes(Path.Combine(Paths.PluginPath, PLUGIN_DIR, "OriginalButtonImages", imageName + ".png"), pngData);
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

            GUILayout.BeginVertical();

            if (GUILayout.Button("Export dialogue database"))
            {
                var db = Resources.FindObjectsOfTypeAll<PixelCrushers.DialogueSystem.DialogueDatabase>()[0];
                var json = UnityEngine.JsonUtility.ToJson(db, prettyPrint);
                File.WriteAllText(BepInEx.Utility.CombinePaths(Paths.PluginPath, PLUGIN_DIR, "database.json"), json);
            }
            prettyPrint = GUILayout.Toggle(prettyPrint, "Pretty print(format)  JSON output");
            GUILayout.Label("Warning : This may take very long time.");
            GUILayout.EndVertical();
            GUILayout.Space(15);

            if (GUILayout.Button("Export button images"))
                ExportButtonImages();

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
