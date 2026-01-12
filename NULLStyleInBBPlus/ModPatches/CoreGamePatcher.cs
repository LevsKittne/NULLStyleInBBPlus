using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using NULL.Content;
using NULL.Manager;
using System.Collections;
using UnityEngine;
using static DevTools.ExtraVariables;

namespace NULL.ModPatches {
    [ConditionalPatchNULL]
    [HarmonyPatch(typeof(CoreGameManager))]
    internal class CoreGamePatcher {
        [HarmonyPatch("ReturnToMenu")]
        [HarmonyPrefix]
        static void ReturnToMenu_Fix() {
            ModManager.NullStyle = false;
            Reset();
            BossManager.Instance?.RemoveAllProjectiles();
            Singleton<MusicManager>.Instance.KillMidi();
            try {
                Object.Destroy(Singleton<CoreGameManager>.Instance.GetHud(0).BaldiTv.gameObject.GetComponent<CustomComponents.NullTV>());
            }
            catch { }
        }

        [HarmonyPatch("Quit")]
        [HarmonyPrefix]
        static bool TryQuit() {
            var nullNpc = NullPlusManager.instance.nullNpc;
            if (nullNpc != null && !nullNpc.Hidden && !nullNpc.IsPaused()
                && Singleton<BaseGameManager>.Instance.FoundNotebooks >= 2) {
                Core.Pause(false);
                nullNpc.transform.position = pm.transform.position + pm.transform.forward * 2f;
                nullNpc.transform.LookAt(pm.transform);
                Singleton<CoreGameManager>.Instance.StartCoroutine(CrashSequence(Singleton<CoreGameManager>.Instance, pm.transform, nullNpc, true));
                return false;
            }

            return true;
        }

        static IEnumerator CrashSequence(CoreGameManager manager, Transform player, Baldi baldi, bool fatalError) {
            Time.timeScale = 0f;
            Singleton<MusicManager>.Instance.StopMidi();
            manager.disablePause = true;

            var cam = manager.GetCamera(0);
            cam.UpdateTargets(baldi.transform, 0);
            cam.offestPos = (player.position - baldi.transform.position).normalized * 2f + Vector3.up * (baldi.gameObject.name.Contains("GLITCH") ? 1 : 1.25f);
            cam.SetControllable(false);
            cam.matchTargetRotation = false;

            manager.audMan.volumeModifier = 0.6f;
            Singleton<InputManager>.Instance.Rumble(1f, 2f);

            if (fatalError) {
                try { Object.Destroy(canvas.gameObject); } catch { }
            }

            HideHuds(true);
            manager.audMan.Play("NullEnd");

            float time = 0f;
            float glitchRate = 0.5f;
            Shader.SetGlobalInt("_ColorGlitching", 1);

            if (Singleton<PlayerFileManager>.Instance.reduceFlashing)
                Shader.SetGlobalInt("_ColorGlitchVal", Random.Range(0, 4096));

            yield return null;

            while (time <= 5f) {
                time += Time.unscaledDeltaTime * 0.5f;
                Shader.SetGlobalFloat("_VertexGlitchSeed", Random.Range(0f, 1000f));
                Shader.SetGlobalFloat("_TileVertexGlitchSeed", Random.Range(0f, 1000f));
                Singleton<InputManager>.Instance.Rumble(time / 5f, 0.05f);

                if (!Singleton<PlayerFileManager>.Instance.reduceFlashing) {
                    glitchRate -= Time.unscaledDeltaTime;
                    Shader.SetGlobalFloat("_VertexGlitchIntensity", Mathf.Pow(time, 2.2f));
                    Shader.SetGlobalFloat("_TileVertexGlitchIntensity", Mathf.Pow(time, 2.2f));
                    Shader.SetGlobalFloat("_ColorGlitchPercent", time * 0.05f);

                    if (glitchRate <= 0f) {
                        Shader.SetGlobalInt("_ColorGlitchVal", Random.Range(0, 4096));
                        Singleton<InputManager>.Instance.SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
                        glitchRate = 0.55f - time * 0.1f;
                    }
                }
                else {
                    Shader.SetGlobalFloat("_ColorGlitchPercent", time * 0.25f);
                    Shader.SetGlobalFloat("_VertexGlitchIntensity", time * 2f);
                    Shader.SetGlobalFloat("_TileVertexGlitchIntensity", time * 2f);
                }
                yield return null;
            }

            if (fatalError) {
                ClearEffects();
                Application.Quit();
            }
            else {
                ClearEffects();
                manager.ResetShaders();
                int lives = (int)manager.ReflectionGetVariable("lives");
                int extraLives = (int)manager.ReflectionGetVariable("extraLives");
                int attempts = (int)manager.ReflectionGetVariable("attempts");

                if (lives < 1 && extraLives < 1) {
                    Singleton<GlobalCam>.Instance.SetListener(true);
                    manager.ReturnToMenu();
                }
                else {
                    if (lives > 0) {
                        manager.ReflectionSetVariable("lives", lives - 1);
                        manager.ReflectionSetVariable("attempts", attempts + 1);
                    }
                    else {
                        manager.ReflectionSetVariable("extraLives", extraLives - 1);
                    }

                    Singleton<BaseGameManager>.Instance.RestartLevel();
                }
            }
        }

        [HarmonyPatch("EndGame")]
        [HarmonyPrefix]
        private static bool EndGame_Prefix(CoreGameManager __instance, Transform player, Baldi baldi) {
            if (baldi.gameObject.name.Contains("NULL")) {
                bool isFatal = (Singleton<BaseGameManager>.Instance.FoundNotebooks >= 2 && BasePlugin.gameCrash.Value == true);
                __instance.StartCoroutine(CrashSequence(__instance, player, baldi, isFatal));
                return false;
            }

            return true;
        }
    }
}