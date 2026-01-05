using BepInEx.Bootstrap;
using DevTools;
using DevTools.Extensions;
using DevTools.Patches;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using NULL.Content;
using NULL.CustomComponents;
using NULL.Manager.CompatibilityModule;
using NULL.NPCs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using static MTM101BaldAPI.AssetTools.AssetLoader;
using static NULL.BasePlugin;
using static PixelInternalAPI.Extensions.GenericExtensions;
using static UnityEngine.Object;

namespace NULL.Manager
{
    internal static partial class ModManager
    {
        public static BasePlugin plug;

        static bool _nullStyle;
        static bool _glitchStyle;
        static bool _doubleTrouble;
        public static AssetManager m = new AssetManager();
        public static bool NullStyle
        {
            get => _nullStyle;
            set
            {
                _nullStyle = value;

                if (!_glitchStyle)
                    RePatch();

                if (!value)
                    GlitchStyle = false;
            }
        }
        public static bool GlitchStyle
        {
            get => _nullStyle && _glitchStyle;
            set
            {
                _glitchStyle = value;
                if (!NullStyle && value)
                    NullStyle = value;

                RePatch();
            }
        }
        public static bool DoubleTrouble
        {
            get => _doubleTrouble;
            set
            {
                _doubleTrouble = value;
                if (value)
                {
                    _nullStyle = true;
                }
                RePatch();
            }
        }

        internal static List<SceneObject> nullScenes = new List<SceneObject>();
        internal static Dictionary<string, CustomLevelObject> nullLevels = new Dictionary<string, CustomLevelObject>();

        internal static IEnumerator LoadContent() {
            bool e = Plugins.IsEditor;
            yield return e ? 4 : 3;
            yield return "Loading assets...";
            TryRunMethod(LoadAssets);
            yield return "Creating NPCs...";
            TryRunMethod(CreateNPCs);
            yield return "Loading captions...";
            TryRunMethod(LoadCaptions);
            if (e)
            {
                yield return "Registering NULL & GLITCH for Level Studio...";
                EditorCompat.Register();
            }
        }

        static void LoadAssets() {
            m.LoadAll();

            foreach (var midi in Utils.GetAllFilesFromFolder("MidiDB"))
                MidiFromMod(midi, plug, ModPath, "MidiDB", midi + ".mid");

            var ambience = ScriptableObject.CreateInstance<LoopingSoundObject>();
            ambience.clips = new[] { m.Get<SoundObject>("unknown_ambience").soundClip };
            ambience.mixer = FindResourceObjectByName<AudioMixerGroup>("Master");
            NullPlusManager.darkAmbience = ambience;

            string prefix = "Projectile_";
            var pr = new GameObject(prefix + "Banana");
            var spriteBase = Instantiate(Utils.FindResourceObjectWithName<GameObject>("Decor_Banana"));
            spriteBase.transform.SetParent(pr.transform, false);
            spriteBase.transform.localScale += new Vector3(1.4f, 1.4f, 1.4f);

            var collider = pr.AddComponent<SphereCollider>();
            collider.radius = 4f;
            collider.isTrigger = true;
            pr.AddComponent<NullProjectile>();

            pr.ConvertToPrefab(true);
            m.Add(pr.name, pr.GetComponent<NullProjectile>());
            BossManager.projectiles.Add(pr.GetComponent<NullProjectile>());

            pr = Instantiate(Utils.FindResourceObjectWithName<GameObject>("Plant"));

            pr.name = prefix + "Plant";
            collider = pr.AddComponent<SphereCollider>();
            collider.radius = 4f;
            collider.isTrigger = true;
            pr.transform.localScale -= Vector3.one * .125f;
            pr.AddComponent<NullProjectile>();
            pr.ConvertToPrefab(true);
            m.Add(pr.name, pr.GetComponent<NullProjectile>());
            BossManager.projectiles.Add(pr.GetComponent<NullProjectile>());

            pr = new GameObject(prefix + "Chair");
            spriteBase = Instantiate(Utils.FindResourceObjectWithName<GameObject>("Chair_Test"));

            spriteBase.transform.SetParent(pr.transform, false);
            spriteBase.transform.localScale += Vector3.one * .5f;
            spriteBase.transform.localPosition = pr.transform.localPosition;
            spriteBase.transform.localRotation = pr.transform.localRotation;

            Destroy(spriteBase.GetComponent<BoxCollider>());
            collider = pr.AddComponent<SphereCollider>();
            collider.radius = 4;
            collider.isTrigger = true;
            pr.AddComponent<NullProjectile>();
            pr.ConvertToPrefab(true);
            m.Add(pr.name, pr.GetComponent<NullProjectile>());
            BossManager.projectiles.Add(pr.GetComponent<NullProjectile>());

            GameObject obj = new GameObject();
            obj.SetActive(false);
            var mainGameManager = obj.AddComponent<NullPlusManager>();

            obj.AddComponent<BossManager>();

            GameObject ambient = Instantiate(FindResourceObject<MainGameManager>().transform.Find("Ambience").gameObject, mainGameManager.transform);
            mainGameManager.ReflectionSetVariable("elevatorScreenPre", FindResourceObject<ElevatorScreen>());
            mainGameManager.ReflectionSetVariable("pitstop", FindResourceObject<MainGameManager>().ReflectionGetVariable("pitstop"));
            mainGameManager.ReflectionSetVariable("happyBaldiPre", PixelInternalAPI.Extensions.GenericExtensions.FindResourceObject<HappyBaldi>());
            mainGameManager.ReflectionSetVariable("ambience", ambient.GetComponent<Ambience>());
            mainGameManager.spawnNpcsOnInit = false;
            mainGameManager.spawnImmediately = false;
            mainGameManager.beginPlayImmediately = false;
            mainGameManager.ReflectionSetVariable("destroyOnLoad", true);
            mainGameManager.gameObject.name = "NullPlusManager";
            mainGameManager.gameObject.ConvertToPrefab(true);
            m.Add("NullPlusMan", mainGameManager);

            var pick = Instantiate(FindResourceObjectByName<GameObject>("PickChallenge"));
            pick.name = "PickPre";
            pick.transform.RemoveChildsContainingNames(new[] { "Speedy", "Stealthy", "Grapple", "ModeText" });
            pick.gameObject.ConvertToPrefab(true);
            m.Add("PickPre", pick);

            var b = Utils.FindResourceObjectContainingName<Baldi>("Baldi");
            NullNPC.slapCurve = (AnimationCurve)b.ReflectionGetVariable("slapCurve");
            NullNPC.speedCurve = (AnimationCurve)b.ReflectionGetVariable("speedCurve");
        }

        internal static void LoadScenes() {
            ItemObject chalkEraser = ItemMetaStorage.Instance.FindByEnum(Items.ChalkEraser).value;

            void Register_Internal(CustomLevelObject ld, bool withNpcs = true) {
                ld.potentialItems = new WeightedItemObject[] {
                    new WeightedItemObject() { selection = chalkEraser, weight = 100 }
                };

                if (ld.randomEvents != null)
                {
                    ld.randomEvents.RemoveAll(x => x != null && x.selection.Type == RandomEventType.Snap);
                }

                if (!withNpcs)
                {
                    ld.additionalNPCs = 0;
                    ld.forcedNpcs = new NPC[0];
                }
                ld.potentialBaldis = new WeightedNPC[] { new WeightedNPC() { selection = m.Get<NullNPC>(!ld.name.Contains("GLITCH") ? "NULL" : "NULLGLITCH"), weight = 100 } };
            }

            SceneObject[] objs = Resources.FindObjectsOfTypeAll<SceneObject>();

            Dictionary<SceneObject, SceneObject> oldToNewMapping_Scenes = new Dictionary<SceneObject, SceneObject>();

            foreach (SceneObject obj in objs)
            {
                if (!(obj.levelObject is CustomLevelObject))
                    continue;

                var scene = ScriptableObject.CreateInstance<SceneObject>();
                Dictionary<string, CustomLevelObject> m_nullLevels = new Dictionary<string, CustomLevelObject>();
                if (obj.manager.GetType() == typeof(MainGameManager))
                {
                    CustomLevelObject nullMain = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(obj.levelObject);
                    nullMain.name = "NULL_" + obj.levelObject.name;
                    Register_Internal(nullMain);
                    nullMain.MarkAsNeverUnload();
                    m_nullLevels.Add(nullMain.name, nullMain);

                    CustomLevelObject nullMain_NoNpcs = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(nullMain);
                    nullMain_NoNpcs.name = "NULL_" + obj.levelObject.name + "_NoNpcs";
                    Register_Internal(nullMain_NoNpcs, false);
                    nullMain_NoNpcs.MarkAsNeverUnload();
                    m_nullLevels.Add(nullMain_NoNpcs.name, nullMain_NoNpcs);

                    CustomLevelObject glitchMain = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(nullMain);
                    glitchMain.name = "GLITCH_" + obj.levelObject.name;
                    Register_Internal(glitchMain);
                    glitchMain.MarkAsNeverUnload();
                    m_nullLevels.Add(glitchMain.name, glitchMain);

                    CustomLevelObject glitchMain_NoNpcs = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(nullMain_NoNpcs);
                    glitchMain_NoNpcs.name = "GLITCH_" + obj.levelObject.name + "_NoNpcs";
                    Register_Internal(glitchMain_NoNpcs, false);
                    glitchMain_NoNpcs.MarkAsNeverUnload();
                    m_nullLevels.Add(glitchMain_NoNpcs.name, glitchMain_NoNpcs);

                    scene.manager = m.Get<NullPlusManager>("NullPlusMan");
                    scene.levelNo = obj.levelNo;
                    scene.levelObject = nullMain;

                    if (System.Text.RegularExpressions.Regex.IsMatch(obj.levelTitle, @"^F\d$"))
                    {
                        scene.name = "NULL_" + obj.levelTitle;
                        scene.levelTitle = "N" + obj.levelTitle.Substring(1);
                    }

                    scene.shopItems = new WeightedItemObject[] {
                        new WeightedItemObject() { selection = chalkEraser, weight = 100 }
                    };

                    scene.totalShopItems = obj.totalShopItems;
                    scene.mapPrice = obj.mapPrice;

                    scene.skybox = obj.skybox;
                    scene.usesMap = obj.usesMap;

                    if (scene.levelObject.name.Contains("_NoNpcs"))
                    {
                        scene.potentialNPCs = new List<WeightedNPC>();
                    }
                    else
                    {
                        scene.potentialNPCs = new List<WeightedNPC> { new WeightedNPC() { selection = m.Get<NullNPC>(!scene.levelObject.name.Contains("GLITCH") ? "NULL" : "NULLGLITCH"), weight = 100 } };
                    }

                    scene.MarkAsNeverUnload();
                    nullLevels.AddRange(m_nullLevels);
                    nullScenes.Add(scene);

                    oldToNewMapping_Scenes.Add(obj, scene);
                }
            }

            GameObject newObject = new GameObject();
            newObject.SetActive(false);
            var finaleMan = newObject.AddComponent<NullPlusFinaleManager>();
            finaleMan.spawnNpcsOnInit = false;
            finaleMan.spawnImmediately = false;
            finaleMan.ReflectionSetVariable("destroyOnLoad", true);
            finaleMan.gameObject.name = "NullPlusFinaleManager";
            finaleMan.gameObject.ConvertToPrefab(true);

            LevelAsset levelAsset = ScriptableObject.CreateInstance<LevelAsset>();
            levelAsset.spawnPoint = new Vector3(65, 5, 25);
            levelAsset.spawnDirection = Direction.North;
            levelAsset.levelSize = new IntVector2(10, 10);

            var types = new int[,]
            {
            { 12, 8, 10, 10, 10, 10, 8, 9 },
            { 4, 1, 14, 8, 8, 9, 4, 1 },
            { 4, 0, 9, 6, 0, 1, 4, 1 },
            { 14, 10, 10, 10, 2, 3, 4, 1 },
            { 4, 0, 0, 0, 8, 8, 0, 1 },
            { 4, 0, 0, 0, 0, 0, 0, 1 },
            { 6, 2, 2, 2, 2, 2, 2, 3 }
            };

            var tilesList = new List<CellData>();

            for (int x = 3; x <= 9; x++)
            {
                for (int z = 2; z <= 9; z++)
                {
                    int type = 0;
                    int roomId = 1;
                    switch (x)
                    {
                        case 3:
                            type = types[x - 3, z - 2]; break;
                        case 4:
                            type = types[x - 3, z - 2];
                            roomId = (z >= 5 && z <= 7) ? 0 : 1; break;
                        case 5:
                            type = types[x - 3, z - 2];
                            roomId = (z >= 5 && z <= 7) ? 0 : 1; break;
                        case 6:
                            type = types[x - 3, z - 2];
                            roomId = (z == 6 || z == 7) ? 0 : 1; break;
                        case 7:
                            type = types[x - 3, z - 2]; break;
                        case 8:
                            type = types[x - 3, z - 2]; break;
                        case 9:
                            type = types[x - 3, z - 2]; break;

                    }
                    tilesList.Add(new CellData() { pos = new IntVector2(x, z), type = type, roomId = roomId });
                }
            }
            levelAsset.tile = tilesList.ToArray();

            static T Find<T>(string name) where T : UnityEngine.Object => PixelInternalAPI.Extensions.GenericExtensions.FindResourceObjectByName<T>(name);

            Transform lightPrefab = null;
            RoomFunctionContainer defaultContainer = null;
            var reflexOffice = Resources.FindObjectsOfTypeAll<RoomAsset>().FirstOrDefault(x => x.name == "Room_ReflexOffice_0");

            if (reflexOffice != null)
            {
                lightPrefab = reflexOffice.lightPre;
                defaultContainer = reflexOffice.roomFunctionContainer;
            }
            else
            {
                var anyOffice = Resources.FindObjectsOfTypeAll<RoomAsset>().FirstOrDefault(x => x.name.Contains("Office"));
                if (anyOffice != null)
                {
                    lightPrefab = anyOffice.lightPre;
                    defaultContainer = anyOffice.roomFunctionContainer;
                }
                else
                {
                    lightPrefab = Find<Transform>("StandardLight");
                    defaultContainer = Resources.FindObjectsOfTypeAll<RoomFunctionContainer>().FirstOrDefault();
                }
            }

            var doorMats = Find<StandardDoorMats>("ClassDoorSet");
            RoomData data = new RoomData()
            {
                name = "Office",
                category = RoomCategory.Office,
                type = RoomType.Room,
                doorMats = doorMats,
                florTex = m.Get<Sprite>("BasicRealCarpet").texture,
                wallTex = m.Get<Sprite>("BasicRealWall").texture,
                ceilTex = m.Get<Sprite>("BasicRealCeiling").texture,
                roomFunctionContainer = defaultContainer,
                activity = new ActivityData()
                {
                    prefab = PixelInternalAPI.Extensions.GenericExtensions.FindResourceObject<NoActivity>(),
                    position = new Vector3(41.5f, 5, 77.5f)
                },
                hasActivity = true
            };

            data.basicObjects.Add(new BasicObjectData()
            {
                position = new Vector3(45, 0, 77.5f),
                prefab = Find<Transform>("BigDesk")
            });

            data.basicObjects.Add(new BasicObjectData()
            {
                position = new Vector3(53, 0, 73.5f),
                prefab = Find<Transform>("BigDesk"),
                rotation = Quaternion.Euler(0, 90, 0)
            });
            data.basicObjects.Add(new BasicObjectData()
            {
                position = new Vector3(55, 0, 65),
                prefab = Find<Transform>("CeilingFan")

            });
            data.basicObjects.Add(new BasicObjectData()
            {
                position = new Vector3(58.09f, 4.05f, 52.01f),
                prefab = Find<Transform>("Decor_Papers")
            });
            data.basicObjects.Add(new BasicObjectData()
            {
                position = new Vector3(55, 0, 52),
                prefab = Find<Transform>("FilingCabinet_Short"),
                rotation = Quaternion.Euler(0, 270, 0)
            });
            data.basicObjects.Add(new BasicObjectData()
            {
                position = new Vector3(45, 3.75f, 77.5f),
                prefab = Find<Transform>("MyComputer"),
                rotation = Quaternion.Euler(0, 270, 0)
            });
            data.basicObjects.Add(new BasicObjectData()
            {
                position = new Vector3(44.28f, 0, 74.05f),
                prefab = Find<Transform>("Chair_Test"),
            });

            levelAsset.rooms.Add(data);

            var black = Find<Texture2D>("BlackTexture");
            data = new RoomData()
            {
                name = "Empty",
                category = RoomCategory.Null,
                type = RoomType.Hall,
                doorMats = doorMats,
                florTex = black,
                wallTex = black,
                ceilTex = black,
                roomFunctionContainer = defaultContainer
            };
            levelAsset.rooms.Add(data);

            data = new RoomData()
            {
                name = "Supplies",
                category = RoomCategory.Special,
                type = RoomType.Room,
                doorMats = doorMats,
                florTex = Find<Texture2D>("Carpet"),
                wallTex = m.Get<Sprite>("BasicRealWall").texture,
                ceilTex = Find<Texture2D>("PlasticTable"),
                lightPre = lightPrefab,
                roomFunctionContainer = defaultContainer
            };
            levelAsset.rooms.Add(data);

            var door = Find<StandardDoor>("ClassDoor_Standard");
            levelAsset.doors.Add(new DoorData(1, door, new IntVector2(6, 5), Direction.North));
            levelAsset.doors.Add(new DoorData(2, door, new IntVector2(4, 4), Direction.North));

            levelAsset.lights.Add(new LightSourceData()
            {
                prefab = lightPrefab,
                position = new IntVector2(5, 6),
                color = Color.white,
                strength = 8
            });

            var officeWindow = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficeWindow").texture, new PosterTextData[0]);
            levelAsset.posters.Add(new PosterData()
            {
                poster = officeWindow,
                position = new IntVector2(4, 5),
                direction = Direction.West
            });
            levelAsset.posters.Add(new PosterData()
            {
                poster = officeWindow,
                position = new IntVector2(6, 7),
                direction = Direction.North
            });
            levelAsset.posters.Add(new PosterData()
            {
                poster = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficeWhiteboard").texture, new PosterTextData[0]),
                position = new IntVector2(4, 7),
                direction = Direction.West
            });
            levelAsset.posters.Add(new PosterData()
            {
                poster = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficeRulesPoster").texture, new PosterTextData[0]),
                position = new IntVector2(6, 7),
                direction = Direction.East
            });
            levelAsset.posters.Add(new PosterData()
            {
                poster = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficePlusPoster").texture, new PosterTextData[0]),
                position = new IntVector2(6, 6),
                direction = Direction.East
            });
            levelAsset.posters.Add(new PosterData()
            {
                poster = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficeToys").texture, new PosterTextData[0]),
                position = new IntVector2(5, 5),
                direction = Direction.South
            });

            levelAsset.name = "NULL";

            var endingScene = ScriptableObject.CreateInstance<SceneObject>();
            endingScene.levelAsset = levelAsset;
            endingScene.levelTitle = endingScene.name = "NULL";
            endingScene.manager = finaleMan;
            endingScene.potentialNPCs = new List<WeightedNPC>();
            endingScene.shopItems = new WeightedItemObject[0];
            endingScene.MarkAsNeverUnload();

            nullScenes.Add(endingScene);

            for (int i = 0; i < nullScenes.Count - 1; i++)
                nullScenes[i].nextLevel = nullScenes[i + 1];

            foreach (var kvp in oldToNewMapping_Scenes)
            {
                if (kvp.Key.previousLevels != null && kvp.Key.previousLevels.Length > 0)
                {
                    kvp.Value.previousLevels = new SceneObject[kvp.Key.previousLevels.Length];
                    for (int i = 0; i < kvp.Key.previousLevels.Length; i++)
                    {
                        if (oldToNewMapping_Scenes.ContainsKey(kvp.Key.previousLevels[i]))
                            kvp.Value.previousLevels[i] = oldToNewMapping_Scenes[kvp.Key.previousLevels[i]];
                    }
                }
            }
        }

        public static void DisableSounds(bool val, params string[] sounds) {
            foreach (var sound in sounds)
            {
                var s = sound.ToString();
                if (val) AudioManagerPatcher.disabledSounds.Add(s);
                else if (AudioManagerPatcher.disabledSounds.Contains(s))
                    AudioManagerPatcher.disabledSounds.Remove(s);
            }
        }
        public static bool ModInstalled(string mod) => Chainloader.PluginInfos.ContainsKey(mod);

        public static void TryRunMethod(Action actionToRun, bool causeCrashIfFail = true) {
            try
            {
                actionToRun();
            }
            catch (Exception e)
            {
                Debug.LogWarning("------ Error caught during an action ------");
                Debug.LogException(e);

                if (causeCrashIfFail)
                    MTM101BaldiDevAPI.CauseCrash(plug.Info, e);

            }
        }
        public static bool TryRunMethodIfModInstalled(string guid, Action act) {
            if (ModInstalled(guid))
            {
                TryRunMethod(act);
                return true;
            }
            return false;
        }
    }
}