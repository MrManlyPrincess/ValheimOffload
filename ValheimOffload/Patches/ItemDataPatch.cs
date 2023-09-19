using System.Text;
using HarmonyLib;

namespace ValheimOffload.Patches
{
    [HarmonyPatch(typeof(ItemDrop.ItemData))]
    internal static class ItemDataPatch
    {
        [HarmonyPatch(nameof(ItemDrop.ItemData.GetTooltip))]
        [HarmonyPostfix]
        internal static void GetTooltip(
            ItemDrop.ItemData item,
            int qualityLevel,
            bool crafting,
            ref string __result)
        {
            if (crafting || (!ValheimOffload.IgnoredItems.Contains(item.m_shared.m_name) &&
                             !ValheimOffload.IgnoredItemSlots.Contains(item.m_gridPos))) return;
            
            __result += "\n<color=grey>Will not be offloaded</color>";
            
            Jotunn.Logger.LogDebug("Tooltip hit");
        }
    }
}
