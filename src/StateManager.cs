using HarmonyLib;
using MGSC;

namespace GatherEntireFloorLoot
{
    // Global access point for your mod
    public static class StateManager
    {
        // Corrected line: standard getter and private setter
        public static MGSC.State ActiveState { get; private set; }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadGame))]
        public static class StateResolvePatch
        {
            static void Postfix(SaveManager __instance)
            {
                // Capture the live instance, slightly later.
                //Plugin.Logger.Log("State Manager working.");
                ActiveState = __instance._state;
            }
        }
    }
}