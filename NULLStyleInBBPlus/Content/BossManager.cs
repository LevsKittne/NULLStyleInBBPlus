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

namespace NULL.Content
{
    public class BossManager : MonoBehaviour
    {
        public static BossManager Instance { get; private set; }
        public bool BossActive { get; set; } = false;
        public bool PlayerHasProjectile { get; set; } = false;
        public int health = BasePlugin.nullHealth.Value;
        public bool bossTransitionWaiting = false, holdBeat = true;
        readonly float initMusSpeed = 0.8f;
        MusicManager MusMan { get => Singleton<MusicManager>.Instance; }
        internal static List<NullProjectile> projectiles = new List<NullProjectile>();
        internal NullNPC nullNpc => NullPlusManager.instance.nullNpc;

        public float currentBossSpeed = 6f;
        public float currentPlayerSpeed = 19f;

        void Start() => Instance = this;

        void Update() {
            if (BossActive && !bossTransitionWaiting)
            {
                if (nullNpc != null)
                {
                    nullNpc.baseSpeed = currentBossSpeed;
                    nullNpc.ReflectionSetVariable("baseSpeed", currentBossSpeed);
                    if (!nullNpc.slideMode) nullNpc.slideMode = true;
                }
            }
        }

        public void StartBossIntro() {
            if (!nullNpc.isGlitch)
            {
                nullNpc.AudMan.QueueAudio("Null_PreBoss_Intro");
                nullNpc.AudMan.QueueAudio("Null_PreBoss_Loop", true);
            }
            if (!IsTimes)
                nullNpc.GetAngry(-168);

            nullNpc.Pause();
            MusMan.PlayMidi("custom_BossIntro", true);
            MusMan.SetSpeed(initMusSpeed);
            holdBeat = true;
            pm.itm.enabled = false;
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
            holdBeat = false;
            nullNpc.slideMode = true;
            nullNpc.behaviorStateMachine.ChangeState(new NullNPC_Chase(nullNpc, nullNpc));
            MusMan.PlayMidi("custom_BossLoop", true);
            MusMan.SetSpeed(initMusSpeed + (10 - health) / 10f);
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
                SpawnProjectiles();
        }
        public void RemoveAllProjectiles() {
            try
            {
                foreach (var projectile in FindObjectsOfType<NullProjectile>())
                    Destroy(projectile.gameObject);

                PlayerHasProjectile = false;
            }
            catch { }
        }

        public void NullHit(int val, bool pause = true) {
            health -= val;

            if (!BossActive)
            {
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
                currentPlayerSpeed += 2f;
            }

            if (nullNpc != null)
            {
                nullNpc.baseSpeed = currentBossSpeed;
                nullNpc.ReflectionSetVariable("baseSpeed", currentBossSpeed);
            }

            if (health >= 0)
            {
                MusMan.SetSpeed(initMusSpeed + (10 - health) * 0.1f);
                if (health > 1 && health < 10) MusMan.HangMidi(true, true);
                if (health < 10) SpawnProjectiles(Mathf.FloorToInt((health - 1) / 3));

                if (health >= 10)
                    SpawnProjectiles(Mathf.FloorToInt((health - 1) / (IsTimes ? 1.25f : 2.5f)));
            }

            if (health == 1)
            {
                MusMan.HangMidi(true, true);
                StartCoroutine(nullNpc.Rage());
            }

            if (health <= 0)
            {
                BossActive = false;
                Singleton<BaseGameManager>.Instance.LoadNextLevel();
                ClearEffects();
                ec.StopAllCoroutines();
            }
        }
    }
}