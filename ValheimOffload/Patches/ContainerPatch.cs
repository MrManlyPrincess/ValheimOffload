using HarmonyLib;

namespace ValheimOffload.Patches
{
    [HarmonyPatch(typeof(Container))]
    public static class PatchContainer
    {
        [HarmonyPatch(nameof(Container.Awake))]
        [HarmonyPostfix]
        static void Awake(Container __instance)
        {
            if (!__instance.m_piece) return;
            if (__instance.name.Contains("TreasureChest_")) return;

            Jotunn.Logger.LogInfo($"Added {__instance.name} to AllContainers");
            ValheimOffload.AllContainers.Add(__instance);
        }

        [HarmonyPatch(nameof(Container.OnDestroyed))]
        [HarmonyPostfix]
        static void OnDestroyed(Container __instance)
        {
            Jotunn.Logger.LogInfo($"Removed {__instance.name} from AllContainers");
            ValheimOffload.AllContainers.Remove(__instance);
        }
    }
}
