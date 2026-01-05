using DevTools.Extensions;
using MTM101BaldAPI;
using NULL.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;
using static DevTools.ExtraVariables;

namespace NULL.Content {
    public class NullPlusFinaleManager : MainGameManager {
        GameObject thanksMan;
        public override void Initialize() {
            base.Initialize();

            GameObject me = new GameObject("ThanksMan");
            me.transform.localPosition = new Vector3(65, 0, 70);
            GameObject me_spr = new GameObject("Sprite");
            me_spr.transform.SetParent(me.transform);
            var s = me_spr.AddComponent<SpriteRenderer>();
            s.sharedMaterial = ObjectCreators.SpriteMaterial;
            s.sprite = ModManager.m.Get<Sprite>("Me");
            me_spr.transform.localPosition = Vector3.up * 3.6f;
            me_spr.transform.localScale = 3 * Vector3.one;
            me.CreateAudioManager();
            me.SetActive(false);
            thanksMan = me;

            ec.notebookTotal = 0;
            Singleton<CoreGameManager>.Instance.GetHud(0).UpdateNotebookText(0, "0/0", false);
        }
        protected override void AllNotebooks() { }
        public override void BeginSpoopMode() { }
        public override void LoadNextLevel() { Core.Quit(); }
        public override void CollectNotebook(Notebook notebook) {
            thanksMan.SetActive(true);
            thanksMan.GetComponent<AudioManager>().QueueAudio("ThanksForPlaying");
        }
    }
}