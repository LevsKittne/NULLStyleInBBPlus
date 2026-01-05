using static NULL.Manager.ModManager;

namespace NULL.Manager.CompatibilityModule {
    public static class Plugins {
        public static bool IsTimes => ModInstalled("pixelguy.pixelmodding.baldiplus.bbextracontent");
        public static bool IsEditor => ModInstalled("mtm101.rulerp.baldiplus.levelstudio");
        public static bool IsLevelTyped => ModInstalled("mtm101.rulerp.baldiplus.leveltyped");
    }
}