using DevTools;
using DevTools.Extensions;
using MidiPlayerTK;
using MTM101BaldAPI.Reflection;
using NULL.Manager;
using NULL.NPCs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DevTools.ExtraVariables;

namespace NULL.Content {
    public class NullPlusManager : MainGameManager {
        public static NullPlusManager instance;
        [SerializeField] public NullNPC nullNpc;
        float glitchVal = 0f;
        BossManager Bm => BossManager.Instance;
        internal static LoopingSoundObject darkAmbience;

        public override void Initialize() {
            BasePlugin.RePatch();
            Reset();
            instance = this;
            if (BossManager.Instance == null)
                new GameObject("BossManager").AddComponent<BossManager>();

            base.Initialize();
            Ec.StartEventTimers();

            if (BasePlugin.darkAtmosphere.Value) {
                foreach (var cell in Ec.AllCells()) cell.SetLight(false);
                Shader.SetGlobalColor("_SkyboxColor", Color.black);
            }
            Ec.standardDarkLevel = new Color(0.35f, 0.35f, 0.35f);
            HideHuds(false);
            
            var player = Singleton<CoreGameManager>.Instance.GetPlayer(0);
            if (player != null)
                Ec.MakeNoise(player.transform.position, 127);
                
            if (BossManager.Instance != null) 
                BossManager.Instance.RemoveAllProjectiles();
            
            var hud = Singleton<CoreGameManager>.Instance.GetHud(0);
            if (hud != null && hud.BaldiTv != null)
                hud.BaldiTv.gameObject.TryAddComponent<CustomComponents.NullTV>();
        }

        public override void BeginPlay() {
            base.BeginPlay();
            if (BasePlugin.darkAtmosphere.Value && darkAmbience != null)
                Singleton<MusicManager>.Instance.QueueFile(darkAmbience, true);
            Singleton<MusicManager>.Instance.KillMidi();
        }

       protected override void VirtualUpdate() {
            base.VirtualUpdate();
            if (Bm == null || Ec == null || nullNpc == null) return;

            if (Bm.BossActive) {
                if (!Singleton<MusicManager>.Instance.MidiPlaying && Bm.holdBeat && !Bm.bossTransitionWaiting && !Core.Paused)
                    Bm.StartBossFight();

                var player = Singleton<CoreGameManager>.Instance.GetPlayer(0);
                if (player != null) {
                    var allNulls = FindObjectsOfType<NullNPC>();
                    float minDistance = 150f;
                    foreach (var n in allNulls) {
                        n.Hear(player.gameObject, player.transform.position, 127);
                        float dist = Vector3.Distance(n.transform.position, player.transform.position);
                        if (dist < minDistance) minDistance = dist;
                    }
                    float vol = Mathf.Clamp(1f - (minDistance - 75f) / 150f, 0f, 1f);
                    Singleton<MusicManager>.Instance.MidiPlayer.MPTK_ChannelVolumeSet(9, vol);
                }
            }
        }

        public override void CollectNotebook(Notebook notebook) {
            base.CollectNotebook(notebook);
            Ec.MakeNoise(notebook.transform.position, 69);
        }

        public override void LoadNextLevel() {
            if (Core.sceneObject.nextLevel is null)
                Core.ReturnToMenu();
            else
                base.LoadNextLevel();
        }

        protected override void LoadSceneObject(SceneObject sceneObject, bool restarting) {
            Singleton<CoreGameManager>.Instance.sceneObject = sceneObject;
            Singleton<AdditiveSceneManager>.Instance.LoadScene("Game");
        }

        public override void ExitedSpawn() {
            Ec.SpawnNPCs(); 
            var foundNull = FindObjectOfType<NullNPC>();
            if (foundNull != null) {
                this.nullNpc = foundNull;
                ModCache.NullNPC = foundNull;
                if (Ec.CellFromPosition(foundNull.transform.position).Null) {
                    var safeCell = Ec.RandomCell(false, false, false); 
                    if (safeCell != null) {
                        Vector3 safePos = safeCell.TileTransform.position + Vector3.up * 5f;
                        foundNull.transform.position = safePos;
                        foundNull.GetComponent<Navigator>().Entity.Teleport(safePos);
                    }
                }
            }
            base.ExitedSpawn();
        }

        public void CheckBossTrigger() {
            if (freezeElevators) return;

            bool isFinalFloor = Core.sceneObject.nextLevel != null && Core.sceneObject.nextLevel.name == "NULL";
            if (!isFinalFloor) return;

            var elevatorManager = Ec.ElevatorManager;
            var brokenElevators = (List<Elevator>)elevatorManager.ReflectionGetVariable("brokenElevators");
            int foundBroken = (int)elevatorManager.ReflectionGetVariable("foundOutOfOrderElevators");
            int totalToBreak = elevatorManager.TotalOutOfOrderElevators;

            if (foundBroken >= totalToBreak) {
                Elevator targetElevator = null;
                float minDistance = float.MaxValue;
                Vector3 playerPos = Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position;

                foreach (var el in elevatorManager.Elevators) {
                    if (!el.IsSpawn && !brokenElevators.Contains(el)) {
                        float d = Vector3.Distance(playerPos, el.transform.position);
                        if (d < minDistance) {
                            minDistance = d;
                            targetElevator = el;
                        }
                    }
                }

                if (targetElevator != null) {
                    freezeElevators = true;
                    var allNulls = Object.FindObjectsOfType<NullNPC>();
                    foreach (var n in allNulls) {
                        n.Pause(0f);
                        n.behaviorStateMachine.ChangeState(new NullNPC_Preboss(n, targetElevator));
                    }
                    Debug.Log("NULL: Heading to elevator at " + targetElevator.transform.position);
                }
            }
        }

#pragma warning disable IDE0051
        new void OnEnable() => MusicManager.OnMidiEvent += MidiEvent;
        new void OnDisable() => MusicManager.OnMidiEvent -= MidiEvent;
#pragma warning restore

        void MidiEvent(MPTKEvent midiEvent) {
            if (Bm == null) return;
            if (Bm.BossActive && !Bm.holdBeat && midiEvent.Command == MPTKCommand.MetaEvent && midiEvent.Meta == MPTKMeta.TextEvent) {
                if (glitchVal <= 0f) StartCoroutine(UnGlitch());
                glitchVal = 1f;
                Shader.SetGlobalFloat("_VertexGlitchSeed", Random.Range(0f, 1000f));
                Shader.SetGlobalFloat("_VertexGlitchIntensity", glitchVal * 3f);
                Shader.SetGlobalFloat("_TileVertexGlitchSeed", Random.Range(0f, 1000f));
                Shader.SetGlobalFloat("_TileVertexGlitchIntensity", glitchVal * 3f);
            }
        }

        IEnumerator UnGlitch() {
            yield return null;
            while (glitchVal > 0f) {
                glitchVal -= Time.deltaTime * 4f;
                Shader.SetGlobalFloat("_VertexGlitchIntensity", glitchVal * 3f);
                Shader.SetGlobalFloat("_TileVertexGlitchIntensity", glitchVal * 3f);
                yield return null;
            }
            glitchVal = 0f;
            Shader.SetGlobalFloat("_VertexGlitchIntensity", 0f);
            Shader.SetGlobalFloat("_TileVertexGlitchIntensity", 0f);
            yield break;
        }

        public static IEnumerator AngerGlitch(float wait) {
            float glitchRate = 0.5f;
            while (wait > 0f) {
                wait -= Time.deltaTime;
                yield return null;
            }
            wait = 0f;
            Shader.SetGlobalInt("_ColorGlitching", 1);
            Shader.SetGlobalInt("_SpriteColorGlitching", 1);
            while (wait < 3f) {
                wait += Time.deltaTime / (ModManager.GlitchStyle ? 2 : 1);
                Shader.SetGlobalFloat("_VertexGlitchSeed", Random.Range(0f, 1000f));
                Shader.SetGlobalFloat("_TileVertexGlitchSeed", Random.Range(0f, 1000f));
                Singleton<InputManager>.Instance.Rumble(wait / 6f, 0.05f);
                if (!Singleton<PlayerFileManager>.Instance.reduceFlashing) {
                    glitchRate -= Time.unscaledDeltaTime;
                    Shader.SetGlobalFloat("_VertexGlitchIntensity", Mathf.Pow(wait, 2f));
                    Shader.SetGlobalFloat("_TileVertexGlitchIntensity", Mathf.Pow(wait, 2f));
                    Shader.SetGlobalFloat("_ColorGlitchPercent", wait * 0.05f);
                    Shader.SetGlobalFloat("_SpriteColorGlitchPercent", wait * 0.05f);
                    if (glitchRate <= 0f) {
                        Shader.SetGlobalInt("_ColorGlitchVal", Random.Range(0, 4096));
                        Shader.SetGlobalInt("_SpriteColorGlitchVal", Random.Range(0, 4096));
                        Singleton<InputManager>.Instance.SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
                        glitchRate = 0.55f - wait * 0.1f;
                    }
                }
                else {
                    Shader.SetGlobalFloat("_ColorGlitchPercent", wait * 0.25f);
                    Shader.SetGlobalFloat("_SpriteColorGlitchPercent", wait * 0.25f);
                    Shader.SetGlobalFloat("_VertexGlitchIntensity", wait * 2f);
                    Shader.SetGlobalFloat("_TileVertexGlitchIntensity", wait * 2f);
                }
                yield return null;
            }
            Shader.SetGlobalFloat("_VertexGlitchIntensity", 0f);
            Shader.SetGlobalFloat("_TileVertexGlitchIntensity", 0f);
            Shader.SetGlobalInt("_ColorGlitching", 0);
            Shader.SetGlobalInt("_SpriteColorGlitching", 0);
        }
    }
}