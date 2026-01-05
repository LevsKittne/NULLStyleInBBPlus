using MTM101BaldAPI;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static MTM101BaldAPI.AssetTools.AssetLoader;
using static NULL.BasePlugin;
using static PixelInternalAPI.Extensions.GenericExtensions;
using static UnityEngine.Object;

namespace DevTools {
    public partial class ContentManager { 
        public static Sprite CreateSprite(string name, float pixelsPerUnits = 100f) => SpriteFromTexture2D(TextureFromFile(Path.Combine(ModPath, "Texture2D", name + ".png")), pixelsPerUnits);
        public static AudioClip CreateAudio(string name) => AudioClipFromFile(Path.Combine(ModPath, "AudioClip", name + ".wav"));
        public static SoundObject CreateSoundObject(AudioClip clip, string subtitle = "", SoundType type = SoundType.Effect, Color? color = null, float sublength = -1f) {
            var soundObject = ScriptableObject.CreateInstance<SoundObject>();
            soundObject.name = clip.name;
            soundObject.soundClip = clip;
            soundObject.subDuration = ((sublength == -1f) ? (clip.length + 1f) : sublength);
            soundObject.soundType = type;
            soundObject.soundKey = subtitle;
            soundObject.color = (color ?? Color.white);
            soundObject.subtitle = (subtitle != "");
            return soundObject;
        }

        public static SoundObject CreateSoundObject(string name, string subtitle = "", SoundType type = SoundType.Effect, Color? color = null, float sublength = -1f) =>
            CreateSoundObject(CreateAudio(name), subtitle, type, color, sublength);

        public static LoopingSoundObject CreateLoopingSoundObject(params string[] clips) {
            var loop = ScriptableObject.CreateInstance<LoopingSoundObject>();
            var list = new List<AudioClip>();
            foreach (var clip in clips) list.Add(CreateAudio(clip));
            loop.clips = list.ToArray();
            loop.mixer = FindResourceObjectByName<AudioMixerGroup>("Master");
            return loop;
        }

        public static Canvas CreateGameCanvas(string name = "BaseCanvas") {
            var canvas = Instantiate(FindResourceObjectByName<GameObject>("GumOverlay"));
            Destroy(canvas.GetComponentInChildren<Image>());
            canvas.SetActive(false);
            canvas.name = name;
            canvas.GetComponent<Canvas>().worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(0).canvasCam;
            canvas.ConvertToPrefab(true);
            return canvas.GetComponent<Canvas>();
        }

        public class MenuUI : MonoBehaviour
        {
            public string localizedText = "Text";
            void Update() => gameObject.GetComponent<TextLocalizer>()?.GetLocalizedText(localizedText);
        }
    }
}