using MTM101BaldAPI;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using System.IO;
using UnityEngine;

namespace NULL.Manager
{
    public class NullStyleOptionsCategory : CustomOptionsCategory
    {
        private MenuToggle ambienceToggle;
        private MenuToggle charactersToggle;

        public override void Build() {
            CreateText("Header", "Null Style Settings", new Vector3(0f, 120f, 0f), BaldiFonts.ComicSans24, TMPro.TextAlignmentOptions.Center, new Vector2(300f, 50f), Color.black);

            ambienceToggle = CreateToggle("AmbienceToggle", "Dark Ambience", OptionsManager.DarkAmbience, new Vector3(0f, 60f, 0f), 250f);
            AddTooltip(ambienceToggle, "There is no lighting in the school. Suspenseful background ambient track plays.");

            charactersToggle = CreateToggle("CharactersToggle", "Other Characters", OptionsManager.Characters, new Vector3(0f, 10f, 0f), 250f);
            AddTooltip(charactersToggle, "Oh no! Null called other characters to help!");

            CreateApplyButton(() =>
            {
                OptionsManager.SetValues(ambienceToggle.Value, charactersToggle.Value);
                OptionsManager.SaveOptions();
            });
        }
    }

    public static class OptionsManager
    {
        private static bool[] options = new bool[2];
        public static bool DarkAmbience => options[0];
        public static bool Characters => options[1];

        public static void Register() {
            CustomOptionsCore.OnMenuInitialize += OnMenuInitialize;
            CustomOptionsCore.OnMenuClose += OnMenuClose;
            LoadOptions();
        }

        private static void OnMenuInitialize(OptionsMenu menu, CustomOptionsHandler handler) {
            handler.AddCategory<NullStyleOptionsCategory>("Null Style");
        }

        private static void OnMenuClose(OptionsMenu menu, CustomOptionsHandler handler) {
            SaveOptions();
        }

        public static void SetValues(bool ambience, bool chars) {
            options[0] = ambience;
            options[1] = chars;
        }

        internal static void SaveOptions() {
            try
            {
                string path = Path.Combine(ModdedSaveSystem.GetCurrentSaveFolder(ModManager.plug), "options.txt");
                File.WriteAllLines(path, new string[] { DarkAmbience.ToString(), Characters.ToString() });
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error occurred while saving options!");
                Debug.LogException(e);
            }
        }

        internal static void LoadOptions() {
            string path = Path.Combine(ModdedSaveSystem.GetCurrentSaveFolder(ModManager.plug), "options.txt");

            if (!File.Exists(path))
            {
                try
                {
                    File.WriteAllLines(path, new string[] { "false", "false" });
                }
                catch { }
                options[0] = false;
                options[1] = false;
                return;
            }

            try
            {
                var f = File.ReadAllLines(path);
                if (f.Length >= 2)
                {
                    options[0] = bool.Parse(f[0]);
                    options[1] = bool.Parse(f[1]);
                }
            }
            catch
            {
                options[0] = false;
                options[1] = false;
            }
        }
    }
}