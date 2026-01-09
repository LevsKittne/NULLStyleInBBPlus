using HarmonyLib;
using NULL.Manager;
using UnityEngine;

namespace NULL.ModPatches {
    [HarmonyPatch(typeof(LightController))]
    internal class LightingPatch {
        
        private static AccessTools.FieldRef<LightController, Color> colorRef = 
            AccessTools.FieldRefAccess<LightController, Color>("color");

        [HarmonyPatch("UpdateLighting")]
        [HarmonyPostfix]
        private static void Postfix(LightController __instance) {
            if (!NULL.BasePlugin.lightGlitch.Value) return;
            if (ModCache.NullNPC == null || !ModCache.NullNPC.gameObject.activeSelf) return;

            float lightX = __instance.position.x * 10f + 5f;
            float lightZ = __instance.position.z * 10f + 5f;
            Vector3 bossPos = ModCache.NullNPC.transform.position;

            float dx = lightX - bossPos.x;
            float dz = lightZ - bossPos.z;
            float sqrDist = dx*dx + dz*dz;
            
            float radius = 100f; 
            float sqrRadius = radius * radius;

            if (sqrDist < sqrRadius) {
                Color originalColor = colorRef(__instance);
                float dist = Mathf.Sqrt(sqrDist);
                float proximity = 1f - (dist / radius);
                proximity = Mathf.SmoothStep(0f, 1f, proximity);
                float uniqueSeed = (lightX * 0.43f) + (lightZ * 0.87f);
                float breathing = Mathf.PerlinNoise(Time.time * 2f, uniqueSeed);
                float lightHealth = Mathf.Lerp(1f, breathing * 0.2f, proximity);

                Color targetColor;

                if (ModManager.GlitchStyle) {
                    targetColor = originalColor * lightHealth;
                    float glitchWave = Mathf.PerlinNoise(Time.time * 1.5f, uniqueSeed + 50f);
                    if (glitchWave > 0.7f) {
                        Color inverted = new Color(1f - originalColor.r, 1f - originalColor.g, 1f - originalColor.b);
                        targetColor = Color.Lerp(targetColor, inverted * lightHealth, (glitchWave - 0.7f) * 3f);
                    }
                } 
                else {
                    Color deadRed = new Color(0.2f, 0f, 0f);
                    targetColor = Color.Lerp(deadRed, originalColor, lightHealth);
                }

                colorRef(__instance) = targetColor;
            }
        }
    }
}