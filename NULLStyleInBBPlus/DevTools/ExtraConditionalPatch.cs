using MTM101BaldAPI;
using NULL.Manager;

namespace DevTools {
    public class ConditionalPatchNULL : ConditionalPatch {
        public override bool ShouldPatch() {
            return ModManager.NullStyle;
        }
    }
}