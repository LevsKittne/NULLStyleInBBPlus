using DevTools;
using HarmonyLib;
using ModdedModesAPI.ModesAPI;
using MTM101BaldAPI;
using NULL.Manager;
using TMPro;
using UnityEngine;

namespace NULL.ModPatches {
    [ConditionalPatchAlways]
    internal class MenuPatcher {
        [HarmonyPatch(typeof(MainMenu), "Start")]
        [HarmonyPrefix]
        static void EnsureInit() {
        }

        public static void ConstructMenu() {
            ModeObject nullScreen = ModeObject.CreateBlankScreenInstance("NullPickMode", false, new Vector2[] {
                new Vector2(-150, 0),
                new Vector2(0, 0),
                new Vector2(150, 0)
            });

            ModeObject mainScreen = ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.MainScreen);
            StandardMenuButton mainButton = mainScreen.StandardButtonBuilder.CreateTransitionButton(nullScreen, 0f, UiTransition.Dither);
            mainButton.name = "MainNull";
            mainButton.AddTextVisual("But_NullMode", out TextMeshProUGUI mainText);
            mainButton.transform.localPosition = new Vector3(0f, 0f);
            mainScreen.StandardButtonBuilder.AddDescriptionText(mainButton, "Men_NullDesc");

            void CreateImageButton(string btnName, string spriteIdleName, string spriteSelectedName, System.Action onPress) {
                StandardMenuButton button = nullScreen.StandardButtonBuilder.CreateBlankButton(btnName);

                Sprite idle = ModManager.m.Get<Sprite>(spriteIdleName);
                Sprite selected = ModManager.m.Get<Sprite>(spriteSelectedName);

                if (idle != null) {
                    button.AddVisual(idle);
                }

                if (idle != null && selected != null) {
                    button.AddHighlightAnimation(selected, idle);
                }

                if (button.image != null) {
                    button.image.rectTransform.sizeDelta = new Vector2(145, 145);
                }

                button.OnPress.AddListener(() => {
                    nullScreen.ScreenTransform.gameObject.SetActive(false);
                    onPress?.Invoke();
                });
            }

            CreateImageButton("NullStory", "NullSelect", "NullSelect_Selected", () => {
                ModManager.NullStyle = true;
                ModManager.DoubleTrouble = false;
                ExtraVariables.LoadGame(customScene: ModManager.nullScenes[0]);
            });

            CreateImageButton("GlitchStory", "GlitchSelect", "GlitchSelect_Selected", () => {
                ModManager.GlitchStyle = true;
                ModManager.DoubleTrouble = false;
                ExtraVariables.LoadGame(customScene: ModManager.nullScenes[0]);
            });

            CreateImageButton("DoubleTrouble", "DoubleSelect", "DoubleSelect_Selected", () => {
                ModManager.NullStyle = false;
                ModManager.GlitchStyle = false;
                ModManager.DoubleTrouble = true;
                ExtraVariables.LoadGame(customScene: ModManager.nullScenes[0]);
            });
        }
    }
}