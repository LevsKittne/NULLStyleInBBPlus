using BepInEx.Configuration;
using UnityEngine;
using NULL.Manager;

namespace NULL.CustomComponents {
    public class GameClickableText : MonoBehaviour {
        public StandardMenuButton button;
        public ConfigEntry<int> intConfig;

        public float min;
        public float max;

        public string prefix;
        public string suffix = "";

        private bool isEditing = false;
        private bool justClicked = false;
        private string editBuffer = "";
        private int caretPos = 0;
        private float blinkTimer = 0f;
        private Color originalColor;

        public void Init(ConfigEntry<int> config, int min, int max, string prefix, string suffix = "") {
            intConfig = config;
            this.min = min;
            this.max = max;
            this.prefix = prefix;
            this.suffix = suffix;

            if (button != null && button.text != null)
                originalColor = button.text.color;

            UpdateDisplay();
        }

        public void OnClick() {
            if (isEditing) return;

            isEditing = true;
            justClicked = true;

            editBuffer = intConfig.Value.ToString();
            caretPos = editBuffer.Length;

            if (button != null && button.text != null)
                button.text.color = Color.red;
        }

        private void Update() {
            if (!isEditing) return;

            if (justClicked) {
                justClicked = false;
                return;
            }

            foreach (char c in Input.inputString) {
                if (c == '\b') {
                    if (caretPos > 0 && editBuffer.Length > 0) {
                        editBuffer = editBuffer.Remove(caretPos - 1, 1);
                        caretPos--;
                    }
                }
                else if (c == '\n' || c == '\r') {
                    StopEditing();
                    return;
                }
                else {
                    if (char.IsDigit(c)) {
                        editBuffer = editBuffer.Insert(caretPos, c.ToString());
                        caretPos++;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                caretPos = Mathf.Max(0, caretPos - 1);
                blinkTimer = 0f;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                caretPos = Mathf.Min(editBuffer.Length, caretPos + 1);
                blinkTimer = 0f;
            }

            if (Singleton<InputManager>.Instance.GetDigitalInput("Interact", true)) {
                if (button != null && !RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, Input.mousePosition)) {
                    StopEditing();
                    return;
                }
            }

            UpdateDisplayEditing();
        }

        private void StopEditing() {
            isEditing = false;
            if (button != null && button.text != null)
                button.text.color = originalColor;

            ApplyValue();
            UpdateDisplay();

            OptionsManager.SaveOptions();
        }

        private void ApplyValue() {
            if (string.IsNullOrEmpty(editBuffer)) editBuffer = min.ToString();

            if (int.TryParse(editBuffer, out int result)) {
                result = Mathf.Clamp(result, (int)min, (int)max);
                intConfig.Value = result;
            }
        }

        private void UpdateDisplay() {
            if (button != null && button.text != null)
                button.text.text = prefix + intConfig.Value + suffix;
        }

        private void UpdateDisplayEditing() {
            blinkTimer += Time.unscaledDeltaTime;
            string cursorChar = (blinkTimer % 1f < 0.5f) ? "|" : " ";

            string visualText = editBuffer;
            if (caretPos >= 0 && caretPos <= visualText.Length)
                visualText = visualText.Insert(caretPos, cursorChar);
            else
                visualText += cursorChar;

            if (button != null && button.text != null)
                button.text.text = prefix + visualText + suffix;
        }
    }
}