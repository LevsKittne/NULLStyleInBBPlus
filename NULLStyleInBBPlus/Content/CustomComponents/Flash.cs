using DevTools.Extensions;
using NULL.Content;
using System.Collections;
using UnityEngine;

namespace NULL.CustomComponents
{
    public class Flash : MonoBehaviour
    {
        public void SetFlash(float time) => StartCoroutine(Flashing(time));
        IEnumerator Flashing(float time) {
            var renderer = gameObject.GetComponentInChildren<SpriteRenderer>();
            var spriteProperties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(spriteProperties);
            spriteProperties.SetInt("_SpriteColorGlitching", 1);
            spriteProperties.SetFloat("_SpriteColorGlitchPercent", 0.9f);
            spriteProperties.SetFloat("_SpriteColorGlitchVal", Random.Range(0f, 4096f));
            renderer.SetPropertyBlock(spriteProperties);
            while (time > 0f)
            {
                time -= Time.unscaledDeltaTime;
                renderer.GetPropertyBlock(spriteProperties);
                spriteProperties.SetFloat("_SpriteColorGlitchVal", Random.Range(0f, 4096f));
                renderer.SetPropertyBlock(spriteProperties);
                yield return null;
            }
            renderer.GetPropertyBlock(spriteProperties);
            spriteProperties.SetInt("_SpriteColorGlitching", 0);
            renderer.SetPropertyBlock(spriteProperties);

            if (BossManager.Instance.BossActive) {
                if (BossManager.Instance.TotalHealth > 1 && BossManager.Instance.TotalHealth < 10)
                    Singleton<MusicManager>.Instance.HangMidi(false, false);
            }
        }
    }
}