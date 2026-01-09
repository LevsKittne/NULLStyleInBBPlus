using BepInEx.Configuration;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.UI;
using NULL.CustomComponents;
using TMPro;
using UnityEngine;

namespace NULL.Manager {
    public class NullStyleOptionsCategory : CustomOptionsCategory {
        private ConfigEntry<bool> ambienceConfig => BasePlugin.darkAtmosphere;
        private ConfigEntry<bool> charactersConfig => BasePlugin.characters;
        private ConfigEntry<bool> resultsTvConfig => BasePlugin.disableResultsTV;
        private ConfigEntry<bool> lightGlitchConfig => BasePlugin.lightGlitch;
        private ConfigEntry<bool> gameCrashConfig => BasePlugin.gameCrash;
        private ConfigEntry<int> healthConfig => BasePlugin.nullHealth;

        private MenuToggle ambienceToggle;
        private MenuToggle charactersToggle;
        private MenuToggle resultsTvToggle;
        private MenuToggle lightGlitchToggle;
        private MenuToggle gameCrashToggle;

        public override void Build() {
            ambienceToggle = CreateToggle("AmbienceToggle", "Dark Ambience", ambienceConfig.Value, new Vector3(0f, 75f, 0f), 300f);
            if (ambienceToggle != null) {
                AddTooltip(ambienceToggle, "There is no lighting in the school. Suspenseful background ambient track plays.");
                StyleToggleCentered(ambienceToggle, 150f);
                var btn = ambienceToggle.transform.Find("HotSpot").GetComponent<StandardMenuButton>();
                btn.OnPress.AddListener(() => {
                    ambienceConfig.Value = ambienceToggle.Value;
                    OptionsManager.SaveOptions();
                });
            }

            charactersToggle = CreateToggle("CharactersToggle", "Other Characters", charactersConfig.Value, new Vector3(0f, 35f, 0f), 300f);
            if (charactersToggle != null) {
                AddTooltip(charactersToggle, "Oh no! Null called other characters to help!");
                StyleToggleCentered(charactersToggle, 150f);
                var btn = charactersToggle.transform.Find("HotSpot").GetComponent<StandardMenuButton>();
                btn.OnPress.AddListener(() => {
                    charactersConfig.Value = charactersToggle.Value;
                    OptionsManager.SaveOptions();
                });
            }

            resultsTvToggle = CreateToggle("ResultsTvToggle", "Disable Results TV", resultsTvConfig.Value, new Vector3(0f, -5f, 0f), 300f);
            if (resultsTvToggle != null) {
                AddTooltip(resultsTvToggle, "If enabled, the score screen in the elevator will be hidden and the animation skipped.");
                StyleToggleCentered(resultsTvToggle, 150f);
                var btn = resultsTvToggle.transform.Find("HotSpot").GetComponent<StandardMenuButton>();
                btn.OnPress.AddListener(() => {
                    resultsTvConfig.Value = resultsTvToggle.Value;
                    OptionsManager.SaveOptions();
                });
            }

            lightGlitchToggle = CreateToggle("LightGlitchToggle", "Dynamic Lighting", lightGlitchConfig.Value, new Vector3(0f, -45f, 0f), 300f);
            if (lightGlitchToggle != null) {
                AddTooltip(lightGlitchToggle, "If enabled, lights near the boss will flicker.");
                StyleToggleCentered(lightGlitchToggle, 150f);
                var btn = lightGlitchToggle.transform.Find("HotSpot").GetComponent<StandardMenuButton>();
                btn.OnPress.AddListener(() => {
                    lightGlitchConfig.Value = lightGlitchToggle.Value;
                    OptionsManager.SaveOptions();
                });
            }

            gameCrashToggle = CreateToggle("GameCrashToggle", "Game Crash", gameCrashConfig.Value, new Vector3(0f, -85f, 0f), 300f);
            if (gameCrashToggle != null) {
                AddTooltip(gameCrashToggle, "If enabled, the game will force close when you are caught by NULL.");
                StyleToggleCentered(gameCrashToggle, 150f);
                var btn = gameCrashToggle.transform.Find("HotSpot").GetComponent<StandardMenuButton>();
                btn.OnPress.AddListener(() => {
                    gameCrashConfig.Value = gameCrashToggle.Value;
                    OptionsManager.SaveOptions();
                });
            }

            CreateClickableTextInt(
                "HealthBtn",
                "Health: ",
                healthConfig,
                1, 1000000,
                "",
                new Vector3(0f, -125f, 0f),
                "Click to type custom health value (1-1000000). <color=#008000ff>Default is 10."
            );
        }
        
        private void StyleToggleCentered(MenuToggle toggle, float checkboxOffset) {
            if (toggle == null) return;

            Transform textObj = toggle.transform.Find("ToggleText");
            if (textObj != null) {
                TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null) {
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    tmp.rectTransform.anchoredPosition = Vector2.zero;
                    tmp.rectTransform.sizeDelta = new Vector2(400f, 50f);
                }
            }

            Transform boxObj = toggle.transform.Find("Box");
            if (boxObj != null) {
                boxObj.localPosition = new Vector3(checkboxOffset, 0f, 0f);
            }

            Transform hotSpot = toggle.transform.Find("HotSpot");
            if (hotSpot != null) {
                hotSpot.localPosition = Vector3.zero;
            }
        }

        private void CreateClickableTextInt(string name, string prefix, ConfigEntry<int> config, int min, int max, string suffix, Vector3 pos, string tooltip) {
            StandardMenuButton button = CreateTextButton(
                () => { },
                name,
                prefix + config.Value + suffix,
                pos,
                BaldiFonts.ComicSans24,
                TextAlignmentOptions.Center,
                new Vector2(400f, 40f),
                Color.black
            );

            GameClickableText clickable = button.gameObject.AddComponent<GameClickableText>();
            clickable.button = button;
            clickable.Init(config, min, max, prefix, suffix);

            button.OnPress.RemoveAllListeners();
            button.OnPress.AddListener(clickable.OnClick);

            AddTooltip(button, tooltip);
        }
    }

    public static class OptionsManager {
        public static void Register() {
            CustomOptionsCore.OnMenuInitialize += OnMenuInitialize;
            CustomOptionsCore.OnMenuClose += OnMenuClose;
        }

        private static void OnMenuInitialize(OptionsMenu menu, CustomOptionsHandler handler) {
            handler.AddCategory<NullStyleOptionsCategory>("Null Style");
        }

        private static void OnMenuClose(OptionsMenu menu, CustomOptionsHandler handler) {
            SaveOptions();
        }

        internal static void SaveOptions() {
            if (ModManager.plug != null)
                ModManager.plug.Config.Save();
        }
    }
}