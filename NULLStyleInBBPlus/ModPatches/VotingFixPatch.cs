namespace NULLStyleInBBPlus.ModPatches {
    public static class VotingFixPatch {
        public static bool Prefix() {
            if (BaseGameManager.Instance != null && BaseGameManager.Instance.Ec != null) {
                var em = BaseGameManager.Instance.Ec.ElevatorManager;
                if (em != null && em.ExitAvailable) {
                    return false;
                }
            }
            return true;
        }
    }
}