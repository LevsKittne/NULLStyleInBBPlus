using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using NULL.Content;
using NULL.CustomComponents;
using NULL.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DevTools.ExtraVariables;
using static NULL.Manager.CompatibilityModule.Plugins;
using static NULL.Manager.ModManager;

namespace NULL.NPCs {
    public class NullNPC : Baldi {
        public bool slideMode;
        internal static new AnimationCurve slapCurve, speedCurve;
        public static int attempts;
        public static float timeSinceExcitingThing;
        public Cell currentCell, previousCell;
        
        float flickerDelay, flickerCheckTimer;
        float _distance;
        
        List<Cell> lightsToChange = new List<Cell>();
        SoundObject hitSound, endSound;
        public SpeechCheck Speaker { get; private set; }
        public bool Hidden { get => !spriteBase; set => spriteBase.SetActive(!value); }
        public bool isGlitch;
        
        public const float ANGER_PER_HIT = 3.5f, ANGER_PER_HIT_TIMES = 3.1f, PAUSE_TIME = 1f, FLASH_TIME = 1.5f;

        void SetupPrefab() {
            baseAnger = 0.1f;
            baseSpeed = 5f;

            ModCache.NullAudio = GetComponent<AudioManager>();
            ModCache.NullAudio.ReflectionSetVariable("subtitleColor", Color.white);
            ModCache.NullAudio.ReflectionSetVariable("overrideSubtitleColor", false);
            ModCache.NullAudio.ignoreListenerPause = false;

            navigator.Initialize(ec);
            navigator.passableObstacles.Add(PassableObstacle.Window);
            spriteRenderer[0].transform.localScale += new Vector3(0.5f, 0.5f, 0.5f);

            if (isGlitch) {
                spriteRenderer[0].transform.localScale *= 2;
                var col = GetComponent<CapsuleCollider>();
                col.radius *= 1.4f;
                col.height *= 2f;
            }

            Speaker = new SpeechCheck(this);
            gameObject.AddComponent<Flash>();
            
            gameObject.AddComponent<NullRadiance>();

            hitSound = !isGlitch ? m.Get<SoundObject>("NullHit") : PixelInternalAPI.Extensions.GenericExtensions.FindResourceObjectByName<SoundObject>("Lose_Buzz");
            endSound = m.Get<SoundObject>("NullEnd");

            if (endSound == null) endSound = hitSound;

            loseSounds = new WeightedSoundObject[] {
                new WeightedSoundObject() {
                    selection = endSound,
                    weight = 100
                }
            };

            if (!isGlitch) {
                NullPlusManager.instance.nullNpc = this;
                ModCache.NullNPC = this;
            }
            else {
                if (NullPlusManager.instance.nullNpc == null) {
                    NullPlusManager.instance.nullNpc = this;
                    ModCache.NullNPC = this;
                }
            }
        }

        public override void Initialize() {
            SetupPrefab();
            behaviorStateMachine.ChangeState(new NullNPC_Chase(this, this));
            behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0));

            foreach (var cell in ec.lights)
                if (cell.lightStrength > 0.5)
                    lightsToChange.Add(cell);

            foreach (var window in Object.FindObjectsOfType<Window>()) {
                if (window.colliders != null) {
                    foreach (var col in window.colliders) {
                        if (col != null && looker != null) {
                            looker.IgnoreTransform(col.transform);
                        }
                    }
                }
            }
        }

        private void OnDestroy() {
            if (ModCache.NullNPC == this) {
                ModCache.Clear();
            }
        }

        [HarmonyPatch(typeof(NPC), "WindowHit")]
        internal class NullWindowBreakPatch {
            [HarmonyPrefix]
            private static bool Prefix(NPC __instance, Window window) {
                if (__instance is NullNPC nullNpc) {
                    bool isBroken = (bool)AccessTools.Field(typeof(Window), "broken").GetValue(window);

                    if (!isBroken) {
                        window.Break(true);
                        nullNpc.Speaker.SpeechChecker("Hide", 0.04f);
                    }

                    var npcCollider = nullNpc.GetComponent<Collider>();
                    if (npcCollider != null && window.colliders != null) {
                        foreach (var winCol in window.colliders) {
                            if (winCol != null) {
                                Physics.IgnoreCollision(npcCollider, winCol, true);
                            }
                        }
                    }
                    
                    return false;
                }
                return true;
            }
        }

        public void Hit(int val, bool pause = true) {
            if (val < 0) return;
            GetComponent<Flash>()?.SetFlash(FLASH_TIME);
            if (pause) this.Pause(PAUSE_TIME);
            GetAngry(IsTimes ? ANGER_PER_HIT_TIMES : ANGER_PER_HIT * val);
            ModCache.NullAudio.FlushQueue(true);
            ModCache.NullAudio.QueueAudio(hitSound);
            if (!BossManager.Instance.BossActive)
                ModCache.NullAudio.QueueAudio(isGlitch ? "GlitchBossStart" : "Null_PreBoss_Start");
        }

        public override void Slap() {
            slapTotal = 0f;
            slapDistance = nextSlapDistance * 2f;
            nextSlapDistance = 0f;

            var speed = !slideMode ? slapDistance / (Delay * MovementPortion) : baseSpeed;

            navigator.SetSpeed(speed);
        }

        public void FixedUpdateSlapDistance() => nextSlapDistance += Speed * Time.deltaTime * TimeScale;

        protected override void VirtualUpdate() {
            base.VirtualUpdate();
            if (!Hidden && !BasePlugin.darkAtmosphere.Value) {
                flickerCheckTimer -= Time.deltaTime;
                if (flickerCheckTimer <= 0f) {
                    FlickerLights(true);
                    flickerCheckTimer = 0.15f;
                }
            }
        }

        public new float Delay => (slapCurve.Evaluate((float)this.ReflectionGetVariable("anger") + (float)this.ReflectionGetVariable("extraAnger")) + 0.4f) * 2f;
        public new float Speed => (speedCurve.Evaluate((float)this.ReflectionGetVariable("anger")) + baseSpeed + (float)this.ReflectionGetVariable("extraAnger")) * 0.5f;

        public void FlickerLights(bool enable) {
            if (!enable) return;

            flickerDelay -= Time.deltaTime * ec.EnvironmentTimeScale;
            
            for (int i = 0; i < lightsToChange.Count; i++) {
                var cell = lightsToChange[i];
                if (cell == null) continue;

                _distance = Vector3.Distance(transform.position, cell.TileTransform.position);
                
                if (_distance > 110f) {
                    if (!cell.lightOn) cell.SetLight(true);
                    continue; 
                }

                float num = (_distance - 30f) / 70f;
                if (behaviorStateMachine.currentState.ToString().Contains("Baldi_Attack")) break;

                if (!Singleton<PlayerFileManager>.Instance.reduceFlashing) {
                    if (_distance <= 30f) {
                        if (cell.lightOn)
                            cell.SetLight(false);
                    }
                    else if (_distance <= 100f) {
                        if (flickerDelay <= 0f && Random.Range(0f, 1f) <= 0.1f) {
                            if (!cell.lightOn)
                                if (Random.Range(0f, 1f) <= num)
                                    cell.SetLight(true);

                                else if (Random.Range(0f, 1f) >= num)
                                    cell.SetLight(false);
                        }
                    }
                    else if (!cell.lightOn) cell.SetLight(true);
                }
                else if (_distance <= 70f) cell.SetLight(false);
                else cell.SetLight(true);
            }
        }

        public class SpeechCheck {
            NullNPC nullNpc;
            public Dictionary<string, bool> nullPhrases = new Dictionary<string, bool>();
            float speechCheckTime = 10f;
            float doorCommentCool;
            float corneredCommentCool;
            public float gameTime;
            public float hadTargetTime;

            public SpeechCheck(NullNPC nullNpc) {
                this.nullNpc = nullNpc;
                string[] allPhrases = new[] { "Bored", "Enough", "Haha", "Hide", "Nothing", "Scary", "Stop", "Wherever" };
                allPhrases.Do(x => nullPhrases.Add(x, false));
            }

            public void SpeechChecker(string phrase, float chance) {
                if (BossManager.Instance.BossActive || BossManager.Instance.bossTransitionWaiting) return;

                List<string> genericPhrases = new List<string> { "Bored", "Scary", "Stop", "Wherever" };
                var audMan = ModCache.NullAudio;
                
                if (audMan == null) return;

                void PlayPhrase(string name, bool generic = false) {
                    if (nullNpc.isGlitch) return;

                    if (Random.Range(0f, 1f) <= chance && !audMan.AnyAudioIsPlaying && (!nullPhrases[name] || (phrase.Equals("Generic") && generic))) {
                        if (!generic) {
                            audMan.QueueAudio("Null_NPC_" + name);
                            nullPhrases[name] = true;
                            return;
                        }
                        genericPhrases.Remove(name);
                    }
                }

                if (!phrase.Equals("Generic")) {
                    PlayPhrase(phrase);
                    return;
                }
                var list = new List<string>(genericPhrases);
                while (list.Count > 0) {
                    var aud = list[Random.Range(0, list.Count)];
                    if (aud != "Bored") {
                        switch (aud) {
                            case "Scary":
                                if (gameTime >= 240f && !nullNpc.looker.PlayerInSight()) {
                                    PlayPhrase(aud, true);
                                    return;
                                }
                                break;
                            case "Stop":
                                if (attempts >= 5 && gameTime >= 60f) {
                                    PlayPhrase(aud, true);
                                    return;
                                }
                                break;
                            case "Where":
                                if (hadTargetTime >= 30f && gameTime >= 60f) {
                                    PlayPhrase(aud, true);
                                    return;
                                }
                                break;
                        }
                    }
                    else if (gameTime >= 300f && timeSinceExcitingThing >= 60f) {
                        PlayPhrase(aud, true);
                        return;
                    }
                    list.Remove(aud);
                }
            }

            public void Update() {
                if (nullPhrases.Count == 0 || nullNpc.currentCell is null || nullNpc.previousCell is null) return;

                speechCheckTime -= Time.deltaTime;
                if (nullNpc.currentCell.doorHere) {
                    foreach (Door door in nullNpc.currentCell.doors) {
                        if (door.GetComponent<LockdownDoor>() == null)
                            door.OpenTimed(0.5f, false);
                    }
                    if (doorCommentCool <= 0f) SpeechChecker("Hide", 0.01f);
                    doorCommentCool = 1f;
                }
                if (speechCheckTime <= 0f) {
                    speechCheckTime = 10f;
                    SpeechChecker("Generic", 0.25f);
                    if (!Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.HasItem() && Singleton<CoreGameManager>.Instance.GetPlayer(0).plm.stamina == 0f &&
                        Vector3.Distance(nullNpc.transform.position, Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position) <= 25f && (float)nullNpc.ReflectionGetVariable("anger") >= 4f)
                        SpeechChecker("Nothing", 0.2f);
                }
                if (corneredCommentCool <= 0f && nullNpc.currentCell != null && nullNpc.previousCell != null && Singleton<BaseGameManager>.Instance.FoundNotebooks > 2) {
                    int navBin = nullNpc.currentCell.NavBin;
                    for (int i = 0; i < 4; i++) {
                        if ((navBin & 1 << i) == 0)
                            nullNpc.currentCell.SilentBlock((Direction)i, true);
                    }

                    if (!ExtraVariables.ec.CheckPath(nullNpc.previousCell, ExtraVariables.ec.CellFromPosition(IntVector2.GetGridPosition(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position)), PathType.Nav))
                        SpeechChecker("Enough", 0.04f);

                    for (int j = 0; j < 4; j++) {
                        if ((navBin & 1 << j) == 0)
                            nullNpc.currentCell.SilentBlock((Direction)j, false);
                    }
                    corneredCommentCool = 1f;
                }
                hadTargetTime += Time.deltaTime;
                gameTime += Time.deltaTime;
                doorCommentCool -= Time.deltaTime;
                corneredCommentCool -= Time.deltaTime;
            }
        }
    }

    public class NullNPC_Chase : Baldi_Chase {
        private float delayTimer;
        protected NullNPC nullNpc;

        public NullNPC_Chase(NPC npc, NullNPC nullNpc) : base(npc, nullNpc) {
            this.nullNpc = nullNpc;
        }

        public override void Enter() {
            delayTimer = nullNpc.Delay;
            nullNpc.ResetSlapDistance();
            if (!nullNpc.Navigator.passableObstacles.Contains(PassableObstacle.Window))
                nullNpc.Navigator.passableObstacles.Add(PassableObstacle.Window);
        }

        public override void OnStateTriggerStay(Collider other, bool validCollision) {
            if (other.CompareTag("Player") && !nullNpc.Hidden) {
                bool flag;
                nullNpc.looker.Raycast(other.transform, Vector3.Magnitude(nullNpc.transform.position - other.transform.position), out flag);
                if (flag) {
                    PlayerManager component = other.GetComponent<PlayerManager>();
                    ItemManager itm = component.itm;
                    if (!component.invincible) {
                        nullNpc.Speaker.SpeechChecker("Haha", 0.04f);
                        nullNpc.CaughtPlayer(component);
                    }
                }
            }
        }

        public override void OnStateTriggerEnter(Collider other, bool validCollision) {
            base.OnStateTriggerEnter(other, validCollision);
        }

        public override void Update() {
            nullNpc.FixedUpdateSlapDistance();
            delayTimer -= Time.deltaTime * npc.TimeScale;
            if (delayTimer <= 0f || nullNpc.slideMode) {
                nullNpc.Slap();
                nullNpc.SlapRumble();
                delayTimer = nullNpc.Delay;
            }

            if (BossManager.Instance != null && BossManager.Instance.BossActive && !nullNpc.Hidden) {
                PlayerManager player = Singleton<CoreGameManager>.Instance.GetPlayer(0);
                if (player != null) {
                    PlayerInSight(player);
                }
            }

            if (!(this is NullNPC_Preboss) && !(this is NullNPC_Rushing) && !nullNpc.Hidden) {
                PlayerManager player = Singleton<CoreGameManager>.Instance.GetPlayer(0);
                if (player != null && !player.invincible) {
                    if (Vector3.Distance(nullNpc.transform.position, player.transform.position) < 5f) {
                        nullNpc.Speaker.SpeechChecker("Haha", 0.04f);
                        nullNpc.CaughtPlayer(player);
                    }
                }
            }

            if (ec.CellFromPosition(IntVector2.GetGridPosition(nullNpc.transform.position)) != nullNpc.currentCell) {
                nullNpc.previousCell = nullNpc.currentCell;
                nullNpc.currentCell = ec.CellFromPosition(IntVector2.GetGridPosition(nullNpc.transform.position));
            }

            nullNpc.Speaker.Update();
        }

        public override void DestinationEmpty() {
            base.DestinationEmpty();
        }

        public override void PlayerSighted(PlayerManager player) {
            base.PlayerSighted(player);
            if (!nullNpc.Navigator.passableObstacles.Contains(PassableObstacle.Window)) {
                nullNpc.Navigator.passableObstacles.Add(PassableObstacle.Window);
                nullNpc.Navigator.CheckPath();
            }
        }

        public override void PlayerInSight(PlayerManager player) {
            base.PlayerInSight(player);
            if (!nullNpc.Navigator.passableObstacles.Contains(PassableObstacle.Window)) {
                nullNpc.Navigator.passableObstacles.Add(PassableObstacle.Window);
                nullNpc.Navigator.CheckPath();
            }
        }
    }

    public class NullNPC_Preboss : NullNPC_Chase {
        protected Vector3 elevatorPos;
        protected EnvironmentController ec;
        public const int MIN_DISTANCE_TO_BEGIN_RUSHING = 15;

        public NullNPC_Preboss(NullNPC nullNpc, Elevator finalElevator) : base(nullNpc, nullNpc) {
            elevatorPos = finalElevator.transform.position + finalElevator.Door.direction.ToVector3() * 10f;
            ec = nullNpc.ec;
        }

        public override void Enter() {
            if (!IsTimes && ec.rooms.Any(x => x.category == RoomCategory.Office)) {
                base.Enter();
                nullNpc.slideMode = true;
                nullNpc.GetAngry(169f);
                nullNpc.baseSpeed = 100f;
                nullNpc.Navigator.passableObstacles.Clear();
                nullNpc.Navigator.passableObstacles.Add(PassableObstacle.Window);
                nullNpc.behaviorStateMachine.ChangeNavigationState(new NavigationState_TargetPosition(nullNpc, 63, elevatorPos));
            }
        }

        public override void Update() {
            base.Update();
            int dist = ec.NavigableDistance(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position.ToCell(), elevatorPos.ToCell(), PathType.Nav);
            if (dist > 0 && dist < MIN_DISTANCE_TO_BEGIN_RUSHING)
                nullNpc.behaviorStateMachine.ChangeState(new NullNPC_Rushing(npc, nullNpc, elevatorPos));
        }

        public override void DestinationEmpty() {
            if (IsTimes) return;
            if (!nullNpc.Hidden) {
                nullNpc.Hidden = true;
            }
            nullNpc.behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(nullNpc, 0));
        }

        public override void PlayerInSight(PlayerManager player) { }
        public override void PlayerSighted(PlayerManager player) { }
        public override void OnStateTriggerEnter(Collider other, bool validCollision) { }
        public override void OnStateTriggerStay(Collider other, bool validCollision) { }
    }

    public class NullNPC_Rushing : NullNPC_Chase {
        Vector3 finalElevatorPos;

        public NullNPC_Rushing(NPC npc, NullNPC nullNPC, Vector3 finalElevatorPos) : base(npc, nullNPC) {
            this.finalElevatorPos = finalElevatorPos;
        }

        public override void Enter() {
            base.Enter();
            nullNpc.Navigator.passableObstacles.Clear();
            nullNpc.Navigator.passableObstacles.Add(PassableObstacle.Window);
            nullNpc.slideMode = true;

            if ((float)nullNpc.ReflectionGetVariable("anger") < 169f) {
                nullNpc.GetAngry(169f);
                nullNpc.baseSpeed = 100f;
            }

            nullNpc.behaviorStateMachine.ChangeNavigationState(new NavigationState_TargetPosition(nullNpc, 0, finalElevatorPos + new Vector3(0f, 5f, 0f)));
            nullNpc.Hidden = false;
        }

        public override void Update() {
            base.Update();
            if (Vector3.Distance(nullNpc.transform.position, finalElevatorPos) < 22f) {
                BossManager.Instance.StartBossIntro();
            }
        }

        public override void OnStateTriggerStay(Collider other, bool validCollision) { }
        public override void PlayerInSight(PlayerManager player) { }
        public override void DestinationEmpty() { }
        public override void OnStateTriggerEnter(Collider other, bool validCollision) { }
    }
}