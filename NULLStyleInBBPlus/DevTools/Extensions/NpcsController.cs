using System.Collections;
using System.Linq;
using UnityEngine;
using static NULL.Manager.ModManager;

namespace DevTools
{
    public static class NpcsController
    {
        static NpcState prevState;
        static NavigationState prevNavState;

        public static NPC AddSprite<T>(this NPC npc, string sprite) where T : NPC {
            npc.spriteRenderer[0].sprite = m.Get<Sprite>(sprite);
            return npc;
        }

        public static void SetRandomPosition(this NPC npc) {
            var list = ExtraVariables.ec.AllTilesNoGarbage(false, false);
            npc.transform.position = list[UnityEngine.Random.Range(0, list.Count)].TileTransform.position + new Vector3(0f, 5f, 0f);
        }

        public static void Spawn(this NPC npc, Vector3 pos) {
            var fixedPos = pos + new Vector3(0f, 5f);
            ExtraVariables.ec.SpawnNPC(npc, new IntVector2((int)(fixedPos.x / 10f), (int)(fixedPos.z / 10f)));
        }

        public static void Spawn(this NPC npc, Cell cell) => npc.Spawn(cell.TileTransform.position);

        public static void DespawnAllNpcs(params Character[] toSave) {
            foreach (NPC npc in Singleton<BaseGameManager>.Instance.Ec.Npcs.ToList())
                if (!toSave.Contains(npc.Character)) npc.Despawn();
        }

        public static void SavePreviousState(this NPC npc) {
            if (!npc.behaviorStateMachine.currentState.ToString().Contains("Pause"))
                prevState = npc.behaviorStateMachine.currentState;
        }

        public static void SavePreviousNavState(this NPC npc) {
            if (!npc.behaviorStateMachine.CurrentNavigationState.ToString().Contains("Nothing"))
                prevNavState = npc.behaviorStateMachine.CurrentNavigationState;
        }

        public static void Pause(this NPC npc, float time = float.MaxValue) {
            if (npc.behaviorStateMachine.currentState is NPC_Pause state)
            {
                try { checked { state.time += time; } }
                catch { state.time = float.MaxValue; }
                return;
            }
            npc.SavePreviousState();
            npc.SavePreviousNavState();
            npc.behaviorStateMachine.ChangeState(new NPC_Pause(npc, time));

        }

        public static void Unpause(this NPC npc) => npc.behaviorStateMachine.ChangeState(new NPC_Pause(npc, 0f));


        public static bool IsPaused(this NPC npc) => npc.behaviorStateMachine.CurrentState.ToString().Contains("NPC_Pause");


        public static IEnumerator Rage(this Baldi b, float increaser = .1f) {
            if (increaser <= 0f)
                yield break;

            while (true)
            {
                b.GetAngry(increaser * b.TimeScale * Time.deltaTime);
                yield return null;
            }
        }

        public class NPC_Pause : NpcState
        {
            public float time;
            public NPC_Pause(NPC npc, float time = float.MaxValue) : base(npc) {
                this.time = time;
            }

            public override void Enter() {
                base.Enter();
                npc.behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(npc, 0));
            }
            public override void OnStateTriggerStay(Collider other, bool validCollision) { }
            public override void OnStateTriggerEnter(Collider other, bool validCollision) { }
            public override void OnStateTriggerExit(Collider other, bool validCollision) { }
            public override void Update() {
                base.Update();
                time -= Time.deltaTime;
                if (time <= 0f)
                {
                    npc.behaviorStateMachine.ChangeState(prevState);
                    npc.behaviorStateMachine.ChangeNavigationState(prevNavState);
                }
            }
        }
    }
}