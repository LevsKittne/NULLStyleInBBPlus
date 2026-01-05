using UnityEngine;
using static NULL.Manager.ModManager;

namespace NULL.Manager {
    internal static partial class ModManager {
        static void LoadCaptions() {
            var sound = m.Get<SoundObject>("Null_NPC_Bored");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Bored";
            sound.subtitle = true;

            sound = m.Get<SoundObject>("Null_NPC_Enough");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Enough";
            sound.subtitle = true;

            sound = m.Get<SoundObject>("Null_NPC_Haha");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Haha";
            sound.subtitle = true;

            sound = m.Get<SoundObject>("Null_NPC_Hide");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Hide";
            sound.subtitle = true;

            sound = m.Get<SoundObject>("NullHit");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Hit";
            sound.subtitle = true;

            sound = m.Get<SoundObject>("Null_NPC_Nothing");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Nothing1";
            sound.subtitle = true;
            sound.additionalKeys = new[] {
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 3f },
            new SubtitleTimedKey() { key = "Vfx_Null_Nothing2", time = 3.4f },
            new SubtitleTimedKey() { key = "Vfx_Null_Nothing3", time = 5.7f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 7.5f },
            new SubtitleTimedKey() { key = "Vfx_Null_Nothing4", time = 7.95f }
            };
            sound = m.Get<SoundObject>("Null_NPC_Scary");
            sound.soundKey = "Vfx_Null_Scary1";
            sound.additionalKeys = new[] {
            new SubtitleTimedKey() { key = "Vfx_Null_Scary2", time = 3.25f },
            new SubtitleTimedKey() { key = "Vfx_Null_Scary3", time = 4.25f },
            new SubtitleTimedKey() { key = "Vfx_Null_Scary4", time = 5.2f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 5.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Scary5", time = 6.7f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 10f },
            new SubtitleTimedKey() { key = "Vfx_Null_Scary6", time = 10.7f },
            };
            sound.subtitle = true;

            sound = m.Get<SoundObject>("Null_NPC_Stop");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Stop1";
            sound.subtitle = true;
            sound.additionalKeys = new[] {
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 2.45f },
            new SubtitleTimedKey() { key = "Vfx_Null_Stop2", time = 3.55f },
            new SubtitleTimedKey() { key = "Vfx_Null_Stop3", time = 6.1f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 7.9f },
            new SubtitleTimedKey() { key = "Vfx_Null_Stop4", time = 8.55f },
            };
            sound = m.Get<SoundObject>("Null_NPC_Wherever");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Wherever1";
            sound.subtitle = true;
            sound.additionalKeys = new[] {
            new SubtitleTimedKey() { key = "Vfx_Null_Wherever2", time = 4f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 7.2f }
            };
            sound = m.Get<SoundObject>("Null_PreBoss_Intro");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Exit1";
            sound.subtitle = true;
            sound.additionalKeys = new[] {
            new SubtitleTimedKey() { key = "Vfx_Null_Exit2", time = 2.7f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit3", time = 5.8f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 6.7f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit4", time = 8.7f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 13.1f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit5", time = 14.3f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 16.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit6", time = 17.4f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit7", time = 20.6f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 23.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit8", time = 24.8f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 28.2f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit9", time = 29f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit10", time = 31.4f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit11", time = 33.7f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit12", time = 35.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit13", time = 37.5f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 39f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit14", time = 39.3f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 42.5f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit15", time = 43.3f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit16", time = 45.4f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit17", time = 47.8f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 48.6f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit18", time = 49.2f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit19", time = 50.4f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit20", time = 54.1f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 57.2f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit21", time = 57.6f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 61.3f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit22", time = 62.6f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit23", time = 64.5f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit24", time = 65.3f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 66.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit25", time = 67.9f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit26", time = 73.3f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 76.5f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit27", time = 77.5f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit28", time = 80.4f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 84.4f }
            };
            sound = m.Get<SoundObject>("Null_PreBoss_Loop");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_Exit29";
            sound.subtitle = true;
            sound.additionalKeys = new[] {
            new SubtitleTimedKey() { key = "Vfx_Null_Exit30", time = 2f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 3.5f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit31", time = 4.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit32", time = 7.1f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit33", time = 8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit34", time = 11f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 12.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit35", time = 13.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit37", time = 22.8f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit38", time = 30.7f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 33.5f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit39", time = 35.2f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit42", time = 40.7f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit40", time = 43.9f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 46.6f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit41", time = 47f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 52.4f },
            new SubtitleTimedKey() { key = "Vfx_Null_Exit42", time = 52.7f },
            new SubtitleTimedKey() { key = "Sfx_Glitch", time = 55f }
            };
            sound = m.Get<SoundObject>("Null_PreBoss_Start");
            sound.color = Color.white;
            sound.soundKey = "Vfx_Null_ExitStart1";
            sound.subtitle = true;
            sound.additionalKeys = new[] {
            new SubtitleTimedKey() { key = "Vfx_Null_ExitStart2", time = 4.3f },
            new SubtitleTimedKey() { key = "Vfx_Null_ExitStart3", time = 7.8f }
            };

            sound = m.Get<SoundObject>("GlitchBossStart");
            sound.color = Color.white;
            sound.soundKey = "Sfx_BadGlitch";
            sound.subtitle = true;

            sound = m.Get<SoundObject>("ThanksForPlaying");
            sound.soundKey = "Vfx_ThanksForPlaying";
            sound.subtitle = true;
        }
    }
}