using DevTools.Extensions;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.SaveSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Object;

namespace DevTools
{
    public static class ExtraVariables
    {
        public static EnvironmentController ec => Singleton<BaseGameManager>.Instance.Ec;
        public static PlayerManager pm => Singleton<CoreGameManager>.Instance.GetPlayer(0);
        public static MovementModifier stopMod;
        public static TimeScaleModifier stopNpcs;
        public static Canvas canvas;
        public static bool freezeElevators;
        public static CoreGameManager Core { get => Singleton<CoreGameManager>.Instance; }
        public static bool AllNotebooks { get => (bool)Singleton<BaseGameManager>.Instance.ReflectionGetVariable("allNotebooksFound"); }
        public static int CurrentFloor { get => Singleton<CoreGameManager>.Instance.sceneObject.levelNo; }
        public static IntVector2 LevelCenter { get => new IntVector2(ec.levelSize.x / 2, ec.levelSize.z / 2); }
        public static List<Cell> AllCellsInHall
        {
            get
            {
                var res = ec.mainHall.AllTilesNoGarbage(false, false);
                return res.Count > 0 ? res : ec.AllTilesNoGarbage(false, false);
            }
        }
        public static Cell RandomCell { get => ec.RandomCell(false, false, false); }
        public static Cell RandomCellFromHallway
        {
            get
            {
                var a = AllCellsInHall;
                return a[Random.Range(0, a.Count)];
            }
        }
        public static bool PlayerInElevator { get => FindObjectOfType<ElevatorScreen>(); }

        public static void Reset() {
            stopMod = new MovementModifier(Vector3.zero, 0f);
            stopNpcs = new TimeScaleModifier() { npcTimeScale = 0f, environmentTimeScale = 1f };
            freezeElevators = false;
        }

        public static void ClearEffects() {
            try
            {
                var fogs = (List<Fog>)ec.ReflectionGetVariable("fogs");
                foreach (var fog in fogs) ec.RemoveFog(fog);
                foreach (var gum in FindObjectsOfType<Gum>()) gum.Cut();
            }
            catch { }
            Singleton<CoreGameManager>.Instance.GetPlayer(0).Am.moveMods.RemoveAll(x => x.Equals(stopMod));
            Shader.SetGlobalFloat("_VertexGlitchIntensity", 0f);
            Shader.SetGlobalFloat("_TileVertexGlitchIntensity", 0f);
            Shader.SetGlobalInt("_ColorGlitching", 0);
            Shader.SetGlobalInt("_SpriteColorGlitching", 0);
        }

        public static void StopAllEvents() {
            for (int i = 0; i < ec.CurrentEventTypes.Count; i++)
                ec.GetEvent(ec.CurrentEventTypes[i]).ReflectionSetVariable("remainingTime", 0f);
        }

        public static void RemoveAllItems() {
            for (int k = 0; k < 5; k++) Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.RemoveItem(k);
        }

        public static void ForceCloseAllElevators() {
            var bgm = Singleton<BaseGameManager>.Instance;
            foreach (var elevator in ec.elevators)
            {
                elevator.Close();
                elevator.Door.Shut();
                bgm.ReflectionSetVariable("elevatorsClosed", (int)bgm.ReflectionGetVariable("elevatorsClosed") + 1);
                bgm.ReflectionSetVariable("elevatorsToClose", (int)bgm.ReflectionGetVariable("elevatorsToClose") - 1);
            }
        }

        public static void StartEvent(RandomEventType type, System.Random rng = null) {
            var RandomEvents = Resources.FindObjectsOfTypeAll<RandomEvent>();
            var randomEvent = Instantiate(RandomEvents.OfType<RandomEvent>().FirstOrDefault(x => x.Type == type));
            var controlledRNG = FindObjectOfType<LevelBuilder>().controlledRNG;
            randomEvent.Initialize(ec, controlledRNG);
            randomEvent.SetEventTime(controlledRNG);
            randomEvent.AfterUpdateSetup(rng ?? new System.Random());
            randomEvent.Begin();
        }

        public static void GameOver() {
            int lives = (int)Core.ReflectionGetVariable("lives");
            int extraLives = (int)Core.ReflectionGetVariable("extraLives");

            if (lives < 1 && extraLives < 1)
            {
                Singleton<GlobalCam>.Instance.SetListener(true);
                Singleton<CoreGameManager>.Instance.ReturnToMenu();
                return;
            }

            if (lives > 0)
                Core.ReflectionSetVariable("lives", lives - 1);
            else
                Core.ReflectionSetVariable("extraLives", extraLives - 1);

            Singleton<BaseGameManager>.Instance.RestartLevel();
        }
        public static void PausePlayer(bool val) {
            Singleton<CoreGameManager>.Instance.disablePause = val;
            if (val)
            {
                pm.Am.moveMods.Add(stopMod);
                pm.itm.enabled = false;
                return;
            }
            pm.Am.moveMods.RemoveAll(x => x.Equals(stopMod));
        }

        public static SceneObject LoadGame(bool setSave = true, bool ignoreSaveFile = false, SceneObject customScene = null) {
            Debug.Log("CustomScene: " + (customScene != null ? customScene.name : "null"));
            bool saveAvailable = !ignoreSaveFile && Singleton<ModdedFileManager>.Instance.saveData.saveAvailable;

            GameLoader gameLoader = Resources.FindObjectsOfTypeAll<GameLoader>()
                                             .FirstOrDefault(x => x.gameObject.scene.IsValid());

            if (gameLoader == null)
                gameLoader = Resources.FindObjectsOfTypeAll<GameLoader>().First();

            gameLoader.gameObject.SetActive(true);
            gameLoader.cgmPre = PixelInternalAPI.Extensions.GenericExtensions.FindResourceObject<CoreGameManager>();
            var scene = customScene ?? Singleton<ModdedFileManager>.Instance.saveData.level;

            if (!saveAvailable)
            {
                try
                {
                    gameLoader.CheckSeed();
                }
                catch (System.NullReferenceException)
                {
                    Debug.LogWarning("ExtraVariables: GameLoader.CheckSeed failed (seedInput is null). Defaulting to random seed.");
                    gameLoader.ReflectionSetVariable("useSeed", false);
                }

                gameLoader.Initialize(2);
                gameLoader.SetMode(0);
            }
            else
            {
                Singleton<ModdedFileManager>.Instance.CreateSavedGameCoreManager(gameLoader);
                gameLoader.SetMode(0);
                Singleton<CursorManager>.Instance.LockCursor();
            }

            ElevatorScreen elevatorScreen = (from x in SceneManager.GetActiveScene().GetRootGameObjects()
                                             where x.name == "ElevatorScreen"
                                             select x).First().GetComponent<ElevatorScreen>();

            gameLoader.AssignElevatorScreen(elevatorScreen);
            elevatorScreen.gameObject.SetActive(true);
            gameLoader.LoadLevel(scene);
            elevatorScreen.Initialize();
            gameLoader.SetSave(setSave);

            if (saveAvailable)
                Singleton<ModdedFileManager>.Instance.DeleteSavedGame();

            return scene;
        }

        public static void HideHuds(bool val) {
            var huds = (HudManager[])Singleton<CoreGameManager>.Instance.ReflectionGetVariable("huds");
            foreach (var hud in huds)
                hud?.Hide(val);
        }
    }
}