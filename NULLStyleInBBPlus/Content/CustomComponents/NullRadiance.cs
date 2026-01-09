using UnityEngine;
using HarmonyLib;

namespace NULL.CustomComponents {
    public class NullRadiance : MonoBehaviour {
        private EnvironmentController ec;
        private int radius = 10; 
        private IntVector2 lastPos;
        
        private float updateTimer = 0f;
        private float updateInterval = 0f; 

        private LightController[,] lightMapCache;

        private void Start() {
            ec = Singleton<BaseGameManager>.Instance.Ec;
            var field = AccessTools.Field(typeof(EnvironmentController), "lightMap");
            lightMapCache = (LightController[,])field.GetValue(ec);
            
            lastPos = IntVector2.GetGridPosition(transform.position);
        }

        private void Update() {
            if (!NULL.BasePlugin.lightGlitch.Value) return;

            if (ec == null || lightMapCache == null) return;

            IntVector2 currentPos = IntVector2.GetGridPosition(transform.position);

            if (currentPos != lastPos) {
                UpdateArea(lastPos);
                lastPos = currentPos;
            }

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval) {
                UpdateArea(currentPos);
                updateTimer = 0f;
            }
        }

        private void UpdateArea(IntVector2 center) {
            int minX = Mathf.Max(0, center.x - radius);
            int maxX = Mathf.Min(ec.levelSize.x - 1, center.x + radius);
            int minZ = Mathf.Max(0, center.z - radius);
            int maxZ = Mathf.Min(ec.levelSize.z - 1, center.z + radius);

            for (int x = minX; x <= maxX; x++) {
                for (int z = minZ; z <= maxZ; z++) {
                    LightController controller = lightMapCache[x, z];
                    if (controller != null) {
                        ec.QueueLightControllerForUpdate(controller);
                    }
                }
            }
        }
    }
}