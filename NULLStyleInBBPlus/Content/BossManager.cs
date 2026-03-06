using DevTools;
using DevTools.Extensions;
using MTM101BaldAPI.Reflection;
using NULL.CustomComponents;
using NULL.Manager;
using NULL.NPCs;
using UnityEngine;
using static DevTools.ExtraVariables;
using static NULL.Manager.CompatibilityModule.Plugins;

namespace NULL.Content {
    public class BossManager : MonoBehaviour {
        public static BossManager Instance { get; private set; }
        public bool BossActive { get; set; } = false;
        public bool PlayerHasProjectile { get; set; } = false;
        public int health = BasePlugin.nullHealth.Value;
        public int activeProjectiles = 0;
        public bool bossTransitionWaiting = false, holdBeat = true;
        
        readonly float initMusSpeed = 0.8f;
        MusicManager MusMan => Singleton<MusicManager>.Instance;
        internal NullNPC nullNpc => ModCache.NullNPC;

        public float currentBossSpeed = 24f;
        public float currentPlayerSpeed = 24f;

        void Awake() { Instance = this; ModCache.BossManager = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void StartBossIntro() {
            var allNulls = FindObjectsOfType<NullNPC>();
            if (allNulls.Length == 0) return;

            if (!allNulls[0].isGlitch) {
                ModCache.NullAudio.QueueAudio("Null_PreBoss_Intro");
                ModCache.NullAudio.QueueAudio("Null_PreBoss_Loop", true);
            }

            foreach (var n in allNulls) {
                if (!IsTimes) n.GetAngry(-168);
                n.Pause();
            }

            Singleton<MusicManager>.Instance.KillMidi();
            MusMan.PlayMidi("custom_BossIntro", true);
            MusMan.SetSpeed(initMusSpeed);
            holdBeat = true;

            if (pm != null && pm.itm != null) pm.itm.enabled = false;

            RemoveAllProjectiles();
            ClearEffects();
            ec.StopAllCoroutines();
            HideHuds(true);
            freezeElevators = false;
            ForceCloseAllElevators();
            StopAllEvents();
            RemoveAllItems();

            SpawnProjectiles(health);
        }

        public void SpawnProjectiles(int count) {
            for (int i = 0; i < count; i++) {
                if (activeProjectiles >= health) break;

                Cell spawnCell = RandomCellFromHallway ?? ec.RandomCell(false, false, false);
                if (spawnCell == null) continue;

                var prList = ModManager.m.GetAll<NullProjectile>();
                if (prList.Length > 0) {
                    var projectile = Instantiate(prList[Random.Range(0, prList.Length)]);
                    projectile.transform.position = spawnCell.FloorWorldPosition;
                    projectile.spawnPoint = projectile.transform.position;
                    
                    activeProjectiles++; 
                }
            }
        }

        public void RemoveAllProjectiles() {
            activeProjectiles = 0;
            var found = FindObjectsOfType<NullProjectile>();
            foreach (var p in found) if (p != null) Destroy(p.gameObject);
            PlayerHasProjectile = false;
        }

        public void NullHit(int val) {
            health -= val;
            activeProjectiles--;

            if (health == 1) {
                MusMan.HangMidi(true, true);
            }

            if (!BossActive) {
                BossActive = true;
                currentBossSpeed = 25f;
                currentPlayerSpeed = 24f;
                bossTransitionWaiting = true;
                Singleton<BaseGameManager>.Instance.StartCoroutine(NullPlusManager.AngerGlitch(8.5f));
            }
            else {
                currentBossSpeed += 2f;
                currentPlayerSpeed += 2f;
            }

            UpdateBossSpeed();

            if (health > 0)
                MusMan.SetSpeed(initMusSpeed + (BasePlugin.nullHealth.Value - health) * 0.1f);

            if (health <= 0) {
                BossActive = false;
                RemoveAllProjectiles();
                Singleton<BaseGameManager>.Instance.LoadNextLevel();
            }
        }

        public void StartBossFight() {
            holdBeat = false;
            foreach (var n in FindObjectsOfType<NullNPC>()) {
                n.slideMode = true;
                n.behaviorStateMachine.ChangeState(new NullNPC_Chase(n, n));
            }
            MusMan.PlayMidi("custom_BossLoop", true);
            MusMan.SetSpeed(initMusSpeed + (BasePlugin.nullHealth.Value - health) / 10f);
            UpdateBossSpeed();
        }

        public void UpdateBossSpeed() {
            foreach (var n in FindObjectsOfType<NullNPC>()) {
                n.baseSpeed = currentBossSpeed;
                n.ReflectionSetVariable("baseSpeed", currentBossSpeed);
            }
        }
    }
}