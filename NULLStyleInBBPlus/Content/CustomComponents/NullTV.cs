using DevTools.Extensions;
using MTM101BaldAPI.Reflection;
using NULL.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace NULL.CustomComponents {

    [RequireComponent(typeof(BaldiTV))]
    public class NullTV : MonoBehaviour {
        [SerializeField] Sprite balSpr;
        [SerializeField] Sprite nullSpr;
        [SerializeField] BaldiTV tv;
#pragma warning disable IDE0051
        void Start() {
            tv = GetComponent<BaldiTV>();
            balSpr = ((Image)tv.ReflectionGetVariable("baldiImage")).sprite;
            nullSpr = ModManager.m.Get<Sprite>((ModManager.GlitchStyle ? "Glitch" : "Null") + "TV");
            Reset(true);
        }
        void Reset(bool nullStyle = false) {
            if (tv == null)
                tv = GetComponent<BaldiTV>();

            Image baldiImage = (Image)tv.ReflectionGetVariable("baldiImage");
            Image staticImage = (Image)tv.ReflectionGetVariable("staticImage");

            if (!nullStyle) {
                tv.ReflectionInvoke("ResetScreen", null);
                baldiImage.SetAlpha(1);
                baldiImage.sprite = balSpr;
                staticImage.rectTransform.SetSiblingIndex(baldiImage.rectTransform.GetSiblingIndex());
                return;
            }
            baldiImage.sprite = nullSpr;
            baldiImage.SetAlpha(0.5f);
            baldiImage.rectTransform.SetSiblingIndex(staticImage.rectTransform.GetSiblingIndex());
        }
        void OnDestroy() => Reset();
        void OnDisable() => Reset();
        void LateUpdate() {
            Image baldiImage = (Image)tv.ReflectionGetVariable("baldiImage");
            if (nullSpr && baldiImage.sprite != nullSpr)
                baldiImage.sprite = nullSpr;
        }
#pragma warning restore
    }
}