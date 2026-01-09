using NULL.Content;
using NULL.NPCs;

namespace NULL.Manager {
    public static class ModCache {
        public static NullNPC NullNPC;
        public static AudioManager NullAudio;
        public static BossManager BossManager;

        public static void Clear() {
            NullNPC = null;
            NullAudio = null;
            BossManager = null;
        }
    }
}