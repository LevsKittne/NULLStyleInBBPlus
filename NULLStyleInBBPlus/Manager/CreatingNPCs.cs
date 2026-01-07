using DevTools.DevAPI.Extensions;
using MTM101BaldAPI.ObjectCreation;
using NULL.NPCs;

namespace NULL.Manager {
    internal static partial class ModManager {
        static void CreateNPCs() {
            NPC npc;
            npc = new NPCBuilder<NullNPC>(plug.Info).AddLooker().AddSpawnableRoomCategories(RoomCategory.Hall).
                AddTrigger().SetMinMaxAudioDistance(250, 450).SetEnum(Character.Baldi).SetName("NULL").SetSprite("ull").BuildNPC();
            m.Add("NULL", npc);

            npc = new NPCBuilder<NullNPC>(plug.Info).AddLooker().AddSpawnableRoomCategories(RoomCategory.Hall).
                AddTrigger().SetMinMaxAudioDistance(250, 450).SetEnum(Character.Baldi).SetName("NULLGLITCH").SetSprite("BaldloonRed").BuildNPC();
            npc.GetComponent<NullNPC>().isGlitch = true;
            m.Add("NULLGLITCH", npc);
        }
    }
}