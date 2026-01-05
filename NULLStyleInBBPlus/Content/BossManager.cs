using DevTools;
using DevTools.Extensions;
using MTM101BaldAPI.Reflection;
using NULL.Content;
using NULL.CustomComponents;
using NULL.Manager;
using NULL.NPCs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DevTools.ExtraVariables;
using static NULL.Manager.CompatibilityModule.Plugins;
using static NULL.Manager.ModManager;

namespace NULL.Content
{
    public class BossManager : MonoBehaviour
    {
        public static BossManager Instance { get; private set; }
        public bool BossActive { get; set; } = false;
        public bool fightStarted = false;
        public bool PlayerHasProjectile { get; set; } = false;

        public bool bossTransitionWaiting = false, holdBeat = true;
        readonly float initMusSpeed = 0.8f;
        MusicManager MusMan { get => Singleton<MusicManager>.Instance; }
        internal static List<NullProjectile> projectiles = new List<NullProjectile>();

        public IEnumerable<NullNPC> ActiveBosses => bossHealths.Keys;

        public float currentBossSpeed = 6f;
        public float currentPlayerSpeed = 19f;

        private Dictionary<NullNPC, int> bossHealths = new Dictionary<NullNPC, int>();
        private Dictionary<NullNPC, int> healthOverrides = new Dictionary<NullNPC, int>();
        private int totalMaxHealth = 0;

        public int TotalHealth => bossHealths.Values.Sum();

        void Awake() {
            Instance = this;
        }

        void Update() {
            if (BossActive && fightStarted && !bossTransitionWaiting)
            {
                foreach (var npc in ActiveBosses)
                {
                    if (npc == null) continue;
                    if (npc.baseSpeed != currentBossSpeed)
                    {
                        npc.baseSpeed = currentBossSpeed;
                        npc.ReflectionSetVariable("baseSpeed", currentBossSpeed);
                    }
                    if (!npc.slideMode) npc.slideMode = true;
                }
            }
        }

        public void SetNPCHealth(NullNPC npc, int hp) {
            if (healthOverrides.ContainsKey(npc))
            {
                healthOverrides[npc] = hp;
            }
            else
            {
                healthOverrides.Add(npc, hp);
            }
        }

        public void StartBossIntro() {
            BossActive = true;
            fightStarted = false;

            try
            {
                ForceCloseAllElevators();
                HideHuds(true);
                pm.itm.enabled = false;
                StopAllEvents();
                RemoveAllItems();
                ClearEffects();
                RemoveAllProjectiles();
                ec.StopAllCoroutines();
                freezeElevators = false;

                bossHealths.Clear();
                totalMaxHealth = 0;
                var allBosses = FindObjectsOfType<NullNPC>();

                foreach (var npc in allBosses)
                {
                    int hp = healthOverrides.ContainsKey(npc) ? healthOverrides[npc] : (npc.isGlitch ? BasePlugin.glitchHealth.Value : BasePlugin.nullHealth.Value);

                    bossHealths.Add(npc, hp);
                    totalMaxHealth += hp;

                    if (!npc.isGlitch)
                    {
                        npc.AudMan.QueueAudio("Null_PreBoss_Intro");
                        npc.AudMan.QueueAudio("Null_PreBoss_Loop", true);
                    }
                    else
                    {
                        npc.AudMan.FlushQueue(true);
                    }

                    if (!IsTimes)
                    {
                        npc.GetAngry(-168);
                    }

                    npc.transform.position = ec.CellFromPosition(pm.transform.position).TileTransform.position + pm.transform.forward * 15f;
                    npc.transform.LookAt(pm.transform);
                    npc.Pause();
                }

                MusMan.PlayMidi("custom_BossIntro", false);
                MusMan.SetSpeed(initMusSpeed);
                MusMan.StopFile();

                holdBeat = true;
                SpawnInitialProjectiles();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NULL Mod] Error in StartBossIntro: {e.Message}");
                holdBeat = false;
                StartBossFight();
            }
        }

        public void StartBossFight() {
            holdBeat = false;
            fightStarted = true;

            foreach (var npc in ActiveBosses)
            {
                if (npc == null) continue;
                npc.slideMode = true;
                npc.behaviorStateMachine.ChangeState(new NullNPC_Chase(npc, npc));
            }

            MusMan.PlayMidi("custom_BossLoop", true);
            MusMan.SetSpeed(initMusSpeed);
        }

        public void SpawnProjectiles(int count = 1) {
            for (int i = 0; i < count; i++)
            {
                var vector = RandomCellFromHallway.TileTransform.position;
                var prList = ModManager.m.GetAll<NullProjectile>();
                var projectile = Instantiate(prList[Random.Range(0, prList.Length)]);

                projectile.transform.position = vector;
                DontDestroyOnLoad(projectile.gameObject);
                projectile.gameObject.SetActive(true);
            }
        }

        public void SpawnInitialProjectiles(int divider = 3) {
            for (int i = 0; i < (AllCellsInHall.Count / divider); i++)
            {
                SpawnProjectiles();
            }
        }

        public void RemoveAllProjectiles() {
            try
            {
                foreach (var projectile in FindObjectsOfType<NullProjectile>())
                {
                    Destroy(projectile.gameObject);
                }

                PlayerHasProjectile = false;
            }
            catch { }
        }

        public void NullHit(NullNPC target, int val, bool pause = true) {
            if (!bossHealths.ContainsKey(target)) return;

            bossHealths[target] -= val;
            int currentTotalHealth = TotalHealth;

            if (!fightStarted)
            {
                fightStarted = true;
                BossActive = true;
                RemoveAllProjectiles();

                currentBossSpeed = 6f;
                currentPlayerSpeed = 24f;

                bossTransitionWaiting = true;
                Singleton<BaseGameManager>.Instance.StartCoroutine(NullPlusManager.AngerGlitch(8.5f));
            }
            else
            {
                currentBossSpeed += 0.5f;
                currentPlayerSpeed += 0.575f;
            }

            foreach (var npc in ActiveBosses)
            {
                if (npc == null) continue;
                npc.baseSpeed = currentBossSpeed;
                npc.ReflectionSetVariable("baseSpeed", currentBossSpeed);
            }

            if (currentTotalHealth >= 0)
            {
                float progress = 1f - ((float)currentTotalHealth / totalMaxHealth);
                MusMan.SetSpeed(initMusSpeed + progress);

                if (bossHealths[target] > 1 && bossHealths[target] < 10)
                {
                    MusMan.HangMidi(true, true);
                }

                if (currentTotalHealth < 10)
                {
                    SpawnProjectiles(Mathf.FloorToInt((currentTotalHealth - 1) / 3));
                }

                if (currentTotalHealth >= 10)
                {
                    SpawnProjectiles(Mathf.FloorToInt((currentTotalHealth - 1) / (IsTimes ? 1.25f : 2.5f)));
                }
            }

            if (bossHealths[target] <= 0)
            {
                StartCoroutine(target.Rage());
                target.Despawn();
                bossHealths.Remove(target);
            }
            else if (bossHealths[target] == 1)
            {
                MusMan.HangMidi(true, true);
                StartCoroutine(target.Rage());
            }

            if (bossHealths.Count == 0)
            {
                BossActive = false;
                fightStarted = false;
                Singleton<BaseGameManager>.Instance.LoadNextLevel();
                ClearEffects();
                ec.StopAllCoroutines();
            }
        }
    }
}