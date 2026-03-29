using BepInEx.Configuration;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.UI;
using NULL.CustomComponents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NULL.Manager {
    public class NullStyleOptionsCategory : CustomOptionsCategory {
        private ConfigEntry<bool> ambienceConfig => BasePlugin.darkAtmosphere;
        private ConfigEntry<bool> charactersConfig => BasePlugin.characters;
        private ConfigEntry<bool> allEventsConfig => BasePlugin.allEvents;
        private ConfigEntry<bool> resultsTvConfig => BasePlugin.disableResultsTV;
        private ConfigEntry<bool> lightGlitchConfig => BasePlugin.lightGlitch;
        private ConfigEntry<bool> gameCrashConfig => BasePlugin.gameCrash;
        private ConfigEntry<int> healthConfig => BasePlugin.nullHealth;
        private ConfigEntry<bool> extraFloorsConfig => BasePlugin.extraFloors;

        private RectTransform container;

        public override void Build() {
            GameObject vpObj = new GameObject("Viewport");
            RectTransform viewport = vpObj.AddComponent<RectTransform>();
            viewport.SetParent(this.transform, false);
            viewport.sizeDelta = new Vector2(480f, 260f);
            viewport.anchoredPosition = new Vector2(0f, -35f);
            vpObj.AddComponent<RectMask2D>();

            GameObject go = new GameObject("OptionsContainer");
            container = go.AddComponent<RectTransform>();
            container.SetParent(viewport, false);
            container.anchorMin = new Vector2(0.5f, 1f);
            container.anchorMax = new Vector2(0.5f, 1f);
            container.pivot = new Vector2(0.5f, 1f);
            container.sizeDelta = new Vector2(480f, 650f);
            container.anchoredPosition = Vector2.zero;

            var scrollScript = go.AddComponent<ScrollController>();
            scrollScript.target = container;

            float curY = -20f;
            float step = -42f;

            AddToggleElement(CreateToggle("AmbienceToggle", "Dark Ambience", ambienceConfig.Value, Vector3.zero, 300f),
                "There is no lighting in the school. Suspenseful background ambient track plays.\n<color=#008000ff>Default is true.", (val) => ambienceConfig.Value = val, ref curY, step);

            AddToggleElement(CreateToggle("CharactersToggle", "Other Characters", charactersConfig.Value, Vector3.zero, 300f),
                "Oh no! Null called other characters to help!\n<color=#008000ff>Default is false.", (val) => charactersConfig.Value = val, ref curY, step);

            AddToggleElement(CreateToggle("AllEventsToggle", "Turn All Events", allEventsConfig.Value, Vector3.zero, 300f),
                "Oh no! All random events are happening at once!\n<color=#008000ff>Default is false.", (val) => allEventsConfig.Value = val, ref curY, step);

            AddToggleElement(CreateToggle("ResultsTvToggle", "Disable Results TV", resultsTvConfig.Value, Vector3.zero, 300f),
                "If enabled, the score screen in the elevator will be hidden and the animation skipped.\n<color=#008000ff>Default is true.", (val) => resultsTvConfig.Value = val, ref curY, step);

            AddToggleElement(CreateToggle("LightGlitchToggle", "Dynamic Lighting", lightGlitchConfig.Value, Vector3.zero, 300f),
                "If enabled, lights near the boss will flicker.\n<color=#008000ff>Default is false.", (val) => lightGlitchConfig.Value = val, ref curY, step);

            AddToggleElement(CreateToggle("GameCrashToggle", "Game Crash", gameCrashConfig.Value, Vector3.zero, 300f),
                "If enabled, the game will force close when you are caught by NULL.\n<color=#008000ff>Default is true.", (val) => gameCrashConfig.Value = val, ref curY, step);

            AddToggleElement(CreateToggle("ExtraFloorsToggle", "Extra Floors (F4 & F5)", extraFloorsConfig.Value, Vector3.zero, 300f),
                "Adds two extra floors (F4 and F5) before the boss fight.\n<color=#008000ff>Default is false.", (val) => extraFloorsConfig.Value = val, ref curY, step);

            CreateClickableTextInt(
                "HealthBtn",
                "Health: ",
                healthConfig,
                1, 1000000,
                "",
                new Vector3(0f, curY, 0f),
                "Click to type custom health value (1-1000000).\n<color=#008000ff>Default is 10."
            );

            GameObject hintObj = new GameObject("ScrollHint");
            TextMeshProUGUI hintText = hintObj.AddComponent<TextMeshProUGUI>();
            hintText.transform.SetParent(this.transform, false);
            hintText.text = "Use mouse wheel to scroll";
            hintText.font = BaldiFonts.ComicSans18.FontAsset();
            hintText.fontSize = 18f;
            hintText.color = Color.red;
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.enableWordWrapping = false;
            hintText.rectTransform.sizeDelta = new Vector2(480f, 30f);
            hintText.rectTransform.anchoredPosition = new Vector2(0f, -170f);

            scrollScript.maxScroll = Mathf.Max(0, Mathf.Abs(curY + step) - 200f);
        }

        private void AddToggleElement(MenuToggle toggle, string tooltip, System.Action<bool> onToggle, ref float y, float step) {
            if (toggle == null) return;
            toggle.transform.SetParent(container, false);
            toggle.transform.localPosition = new Vector3(0f, y, 0f);
            StyleToggleCentered(toggle, 150f);
            AddTooltip(toggle, tooltip);

            var btn = toggle.transform.Find("HotSpot").GetComponent<StandardMenuButton>();
            btn.OnPress.AddListener(() => {
                onToggle(toggle.Value);
                OptionsManager.SaveOptions();
            });
            y += step;
        }

        private void StyleToggleCentered(MenuToggle toggle, float checkboxOffset) {
            if (toggle == null) return;
            Transform textObj = toggle.transform.Find("ToggleText");
            if (textObj != null) {
                TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                tmp.rectTransform.anchoredPosition = Vector2.zero;
                tmp.rectTransform.sizeDelta = new Vector2(400f, 50f);
            }
            Transform boxObj = toggle.transform.Find("Box");
            if (boxObj != null) boxObj.localPosition = new Vector3(checkboxOffset, 0f, 0f);
            Transform hotSpot = toggle.transform.Find("HotSpot");
            if (hotSpot != null) hotSpot.localPosition = Vector3.zero;
        }

        private void CreateClickableTextInt(string name, string prefix, ConfigEntry<int> config, int min, int max, string suffix, Vector3 pos, string tooltip) {
            StandardMenuButton button = CreateTextButton(() => { }, name, prefix + config.Value + suffix, pos, BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(400f, 40f), Color.black);
            button.transform.SetParent(container, false);
            GameClickableText clickable = button.gameObject.AddComponent<GameClickableText>();
            clickable.button = button;
            clickable.Init(config, min, max, prefix, suffix);
            button.OnPress.RemoveAllListeners();
            button.OnPress.AddListener(clickable.OnClick);
            AddTooltip(button, tooltip);
        }
    }

    public class ScrollController : MonoBehaviour {
        public RectTransform target;
        public float maxScroll;
        private float scrollY = 0f;
        private float targetScrollY = 0f;
        private const float sensitivity = 30f;
        private const float lerpSpeed = 12f;

        void Update() {
            float delta = Input.mouseScrollDelta.y;
            if (delta != 0) targetScrollY -= delta * sensitivity;
            targetScrollY = Mathf.Clamp(targetScrollY, 0f, maxScroll);
            scrollY = Mathf.Lerp(scrollY, targetScrollY, Time.unscaledDeltaTime * lerpSpeed);
            if (target != null) target.anchoredPosition = new Vector2(0f, scrollY);
        }
    }

    public static class OptionsManager {
        public static void Register() {
            CustomOptionsCore.OnMenuInitialize += (menu, handler) => handler.AddCategory<NullStyleOptionsCategory>("Null Style");
        }
        internal static void SaveOptions() {
            if (ModManager.plug != null) ModManager.plug.Config.Save();
        }
    }
}