using DevTools;
using DevTools.Extensions;
using MTM101BaldAPI.Reflection;
using NULL.CustomComponents;
using NULL.Manager;
using NULL.NPCs;
using System.Collections.Generic;
using UnityEngine;
using static DevTools.ExtraVariables;
using static NULL.Manager.CompatibilityModule.Plugins;

namespace NULL.Content {
    public class BossManager : MonoBehaviour {
        public static BossManager Instance { get; private set; }
        public bool BossActive { get; set; } = false;
        public bool PlayerHasProjectile { get; set; } = false;
        public int health = BasePlugin.nullHealth.Value;
        public bool bossTransitionWaiting = false, holdBeat = true;
        readonly float initMusSpeed = 0.8f;
        MusicManager MusMan => Singleton<MusicManager>.Instance;
        internal static List<NullProjectile> projectiles = new List<NullProjectile>();
        internal NullNPC nullNpc => ModCache.NullNPC;

        public float currentBossSpeed = 24f;
        public float currentPlayerSpeed = 24f;

        void Awake() {
            Instance = this;
            ModCache.BossManager = this;
        }

        void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }

        void Update() {
            if (BossActive && !bossTransitionWaiting) {
                if (nullNpc != null) {
                    if (!nullNpc.slideMode) {
                        nullNpc.slideMode = true;
                    }
                }
            }
        }

        public void UpdateBossSpeed() {
            if (nullNpc != null) {
                nullNpc.baseSpeed = currentBossSpeed;
                nullNpc.ReflectionSetVariable("baseSpeed", currentBossSpeed);
                if (nullNpc.Navigator != null && nullNpc.Navigator.Am != null) {
                    nullNpc.Navigator.Am.ReflectionSetVariable("ignoreMultiplier", true);
                }
            }
        }

        public void StartBossIntro() {
            if (nullNpc == null) {
                return;
            }

            if (!nullNpc.isGlitch) {
                ModCache.NullAudio.QueueAudio("Null_PreBoss_Intro");
                ModCache.NullAudio.QueueAudio("Null_PreBoss_Loop", true);
            }
            if (!IsTimes) {
                nullNpc.GetAngry(-168);
            }

            nullNpc.Pause();
            MusMan.PlayMidi("custom_BossIntro", true);
            MusMan.SetSpeed(initMusSpeed);
            holdBeat = true;

            if (pm != null && pm.itm != null) {
                pm.itm.enabled = false;
            }

            ClearEffects();
            RemoveAllProjectiles();
            ec.StopAllCoroutines();
            MusMan.StopFile();
            HideHuds(true);
            freezeElevators = false;
            ForceCloseAllElevators();
            StopAllEvents();
            RemoveAllItems();
            SpawnInitialProjectiles();
        }

        public void StartBossFight() {
            if (nullNpc == null) {
                return;
            }

            holdBeat = false;
            nullNpc.slideMode = true;
            nullNpc.behaviorStateMachine.ChangeState(new NullNPC_Chase(nullNpc, nullNpc));
            MusMan.PlayMidi("custom_BossLoop", true);
            MusMan.SetSpeed(initMusSpeed + (10 - health) / 10f);
            UpdateBossSpeed();
        }

        public void SpawnProjectiles(int count = 1) {
            for (int i = 0; i < count; i++) {
                try {
                    Cell spawnCell = RandomCellFromHallway;
                    if (spawnCell == null) spawnCell = ec.RandomCell(false, false, false);
                    if (spawnCell == null) return;

                    var vector = spawnCell.TileTransform.position;
                    var prList = ModManager.m.GetAll<NullProjectile>();
                    if (prList.Length > 0) {
                        var projectile = Instantiate(prList[Random.Range(0, prList.Length)]);
                        projectile.transform.position = vector;
                        DontDestroyOnLoad(projectile.gameObject);
                        projectile.gameObject.SetActive(true);
                    }
                }
                catch (System.Exception ex) {
                    Debug.LogWarning($"Failed to spawn projectile: {ex.Message}");
                }
            }
        }

        public void SpawnInitialProjectiles(int divider = 3) {
            int count = AllCellsInHall.Count;
            if (count > 0) {
                for (int i = 0; i < (count / divider); i++) {
                    SpawnProjectiles();
                }
            }
        }

        public void RemoveAllProjectiles() {
            try {
                var found = FindObjectsOfType<NullProjectile>();
                foreach (var projectile in found) {
                    if (projectile != null) {
                        Destroy(projectile.gameObject);
                    }
                }
                PlayerHasProjectile = false;
            }
            catch { }
        }

        public void NullHit(int val, bool pause = true) {
            health -= val;

            if (!BossActive) {
                BossActive = true;
                RemoveAllProjectiles();

                currentBossSpeed = 24.5f;
                currentPlayerSpeed = 24f;

                bossTransitionWaiting = true;
                if (Singleton<BaseGameManager>.Instance != null) {
                    Singleton<BaseGameManager>.Instance.StartCoroutine(NullPlusManager.AngerGlitch(8.5f));
                }
            }
            else {
                currentBossSpeed += 2f;
                currentPlayerSpeed += 2f;
            }

            UpdateBossSpeed();

            if (health >= 0) {
                MusMan.SetSpeed(initMusSpeed + (10 - health) * 0.1f);
                if (health > 1 && health < 10) {
                    MusMan.HangMidi(true, true);
                }
                if (health < 10) {
                    SpawnProjectiles(Mathf.FloorToInt((health - 1) / 3));
                }

                if (health >= 10) {
                    int projCount = Mathf.FloorToInt((health - 1) / (IsTimes ? 1.25f : 2.5f));
                    SpawnProjectiles(Mathf.Clamp(projCount, 1, 12));
                }
            }

            if (health == 1 && nullNpc != null) {
                MusMan.HangMidi(true, true);
                StartCoroutine(nullNpc.Rage());
            }

            if (health <= 0) {
                BossActive = false;
                if (Singleton<BaseGameManager>.Instance != null) {
                    Singleton<BaseGameManager>.Instance.LoadNextLevel();
                }
                ClearEffects();
                ec.StopAllCoroutines();
            }
        }
    }
}