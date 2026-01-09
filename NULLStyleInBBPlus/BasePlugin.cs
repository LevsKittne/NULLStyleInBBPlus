using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModdedModesAPI.ModesAPI;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using NULL.CustomComponents;
using NULL.Manager.CompatibilityModule;
using System.IO;

namespace NULL {
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pixelguy.pixelmodding.baldiplus.moddedmodesapi", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pixelguy.pixelmodding.baldiplus.bbextracontent", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ModInfo.ID, ModInfo.NAME, ModInfo.VERSION)]
    public class BasePlugin : BaseUnityPlugin {
        internal static Harmony harmony = new Harmony(ModInfo.ID);
        public static string ModPath;

        public static ConfigEntry<bool> characters;
        public static ConfigEntry<bool> darkAtmosphere;
        public static ConfigEntry<bool> disableResultsTV;
        public static ConfigEntry<bool> lightGlitch;
        public static ConfigEntry<bool> gameCrash;
        public static ConfigEntry<int> nullHealth;

        private void Awake() {
            Manager.ModManager.plug = this;
            ModPath = AssetLoader.GetModPath(this);

            characters = Config.Bind("Null Style settings", "Enable another characters", false, "Setting this \"true\" will enable other characters on the floor except Null/Red Baldloon");
            darkAtmosphere = Config.Bind("Null Style settings", "Enable the dark atmosphere", true, "Setting this \"true\" will enable the dark atmosphere, which makes the level darker and more creepy");
            disableResultsTV = Config.Bind("Null Style settings", "Disable Results TV", true, "If true, the score screen in the elevator will be hidden and the animation skipped.");
            lightGlitch = Config.Bind("Null Style settings", "Dynamic Lighting", true, "If true, lights near NULL/GLITCH will flicker.");
            gameCrash = Config.Bind("Null Style settings", "Game Crash", false, "If true, the game will close itself when NULL/GLITCH catches you, simulating a crash.");
            nullHealth = Config.Bind("Null Style settings", "Health", 10, "Setting a custom amount of null's health");

            Manager.OptionsManager.Register();

            //gameObject.AddComponent<DebugManager>(); 

            harmony.PatchAllConditionals();
            
            CriminalPackCompat.Init(harmony);

            LoadingEvents.RegisterOnAssetsLoaded(Info, Manager.ModManager.LoadContent(), LoadingEventOrder.Pre);
            LoadingEvents.RegisterOnAssetsLoaded(Info, () => Manager.ModManager.TryRunMethod(Manager.ModManager.LoadScenes), LoadingEventOrder.Post);

            AssetLoader.LocalizationFromFile(Path.Combine(ModPath, "Language", "English", "SubtitlesEn.json"), Language.English);
            CustomModesHandler.OnMainMenuInitialize += ModPatches.MenuPatcher.ConstructMenu;
        }

        public static void RePatch() {
            harmony.UnpatchSelf();
            harmony.PatchAllConditionals();
            CriminalPackCompat.Init(harmony);
        }
    }

    static class ModInfo {
        public const string ID = "levs_kittne.baldiplus.null";
        public const string NAME = "NULL";
        public const string VERSION = "1.2.6";
    }
}