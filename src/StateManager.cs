using HarmonyLib;
using MGSC;

namespace GatherEntireFloorLoot
{
    // Global access point for your mod
    public static class StateManager
    {
        // Corrected line: standard getter and private setter
        public static MGSC.State ActiveState { get; private set; }

        [HarmonyPatch(typeof(State), nameof(State.Resolve))]
        public static class StateResolvePatch
        {
            static void Postfix(State __instance)
            {
                // Capture the live instance
                ActiveState = __instance;
            }
        }
    }
}