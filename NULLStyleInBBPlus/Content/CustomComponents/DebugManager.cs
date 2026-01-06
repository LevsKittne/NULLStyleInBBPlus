using NULL.Content;
using UnityEngine;

namespace NULL.CustomComponents {
    public class DebugManager : MonoBehaviour {
        private void Update() {
            if (Input.GetKeyDown(KeyCode.V)) {
                SkipFloor();
            }
            if (Input.GetKeyDown(KeyCode.B)) {
                GiveNotebook();
            }
            if (Input.GetKeyDown(KeyCode.N)) {
                GiveProjectile();
            }
        }

        private void SkipFloor() {
            if (Singleton<BaseGameManager>.Instance != null) {
                Singleton<BaseGameManager>.Instance.LoadNextLevel();
            }
        }

        private void GiveNotebook() {
            if (Singleton<BaseGameManager>.Instance != null) {
                Singleton<BaseGameManager>.Instance.CollectNotebooks(1);
            }
        }

        private void GiveProjectile() {
            if (BossManager.Instance != null) {
                BossManager.Instance.SpawnProjectiles(1);
            }
            else {
                Debug.LogWarning("BossManager instance is null. Cannot spawn projectile.");
            }
        }
    }
}