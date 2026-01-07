using PlusLevelStudio;
using NULL.NPCs;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;
using UnityEngine;

namespace NULL.Manager.CompatibilityModule {
    public static class EditorCompat {
        public static void Register() {
            NullNPC nullPrefab = ModManager.m.Get<NullNPC>("NULL");
            NullNPC glitchPrefab = ModManager.m.Get<NullNPC>("NULLGLITCH");

                if (LevelStudioPlugin.Instance != null) {
                if (LevelStudioPlugin.Instance.npcDisplays.ContainsKey("NULL")) {
                    LevelStudioPlugin.Instance.npcDisplays.Remove("NULL");
                }
                if (LevelStudioPlugin.Instance.npcDisplays.ContainsKey("NULLGLITCH")) {
                    LevelStudioPlugin.Instance.npcDisplays.Remove("NULLGLITCH");
                }

                GameObject nullVisual = EditorInterface.AddNPCVisual("NULL", nullPrefab);
                GameObject glitchVisual = EditorInterface.AddNPCVisual("NULLGLITCH", glitchPrefab);

                nullVisual.transform.localScale += new Vector3(0.5f, 0.5f, 0.5f);
                nullVisual.GetComponentInChildren<SpriteRenderer>().transform.localPosition += new Vector3(0f, 0.35f, 0f);

                glitchVisual.transform.localScale *= 2;
                glitchVisual.GetComponentInChildren<SpriteRenderer>().transform.localPosition += new Vector3(0f, 0.25f, 0f);
            }

            if (LevelLoaderPlugin.Instance != null) {
                if (!LevelLoaderPlugin.Instance.npcAliases.ContainsKey("NULL")) {
                    LevelLoaderPlugin.Instance.npcAliases.Add("NULL", nullPrefab);
                }

                if (!LevelLoaderPlugin.Instance.npcAliases.ContainsKey("NULLGLITCH")) {
                    LevelLoaderPlugin.Instance.npcAliases.Add("NULLGLITCH", glitchPrefab);
                }
            }

            EditorInterfaceModes.AddModeCallback((mode, isVanilla) => {
                if (mode.availableTools.ContainsKey("npcs")) {
                    Sprite nullIcon = ModManager.m.Get<Sprite>("EditorNpc_NULL");
                    Sprite glitchIcon = ModManager.m.Get<Sprite>("EditorNpc_NULLGLITCH");

                    if (!mode.availableTools["npcs"].Exists(x => x.id == "npc_NULL")) {
                        EditorInterfaceModes.AddToolToCategory(mode, "npcs", new NullTool(nullIcon));
                    }
                    if (!mode.availableTools["npcs"].Exists(x => x.id == "npc_NULLGLITCH")) {
                        EditorInterfaceModes.AddToolToCategory(mode, "npcs", new NullGlitchTool(glitchIcon));
                    }
                }
            });

            Shader.SetGlobalFloat("_VertexGlitchIntensity", 0f);
            Shader.SetGlobalFloat("_TileVertexGlitchIntensity", 0f);
            Shader.SetGlobalInt("_ColorGlitching", 0);
            Shader.SetGlobalInt("_SpriteColorGlitching", 0);
        }
    }

    public class NullTool : NPCTool {
        public NullTool(Sprite sprite) : base("NULL", sprite) { }
        public override string titleKey => "NULL";
        public override string descKey => "The main antagonist.";
    }

    public class NullGlitchTool : NPCTool {
        public NullGlitchTool(Sprite sprite) : base("NULLGLITCH", sprite) { }
        public override string titleKey => "NULL (Glitch)";
        public override string descKey => "A glitchy variant.";
    }
}