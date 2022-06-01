using System.Collections.Generic;
using HarmonyLib;

namespace ValheimOffload.Patches
{
    [HarmonyPatch(typeof(InventoryGrid))]
    public static class InventoryGridPatch
    {
        [HarmonyPatch(nameof(UpdateGui))]
        [HarmonyPostfix]
        internal static void UpdateGui(
            InventoryGrid __instance,
            Player player,
            ItemDrop.ItemData dragItem,
            Inventory ___m_inventory,
            List<InventoryGrid.Element> ___m_elements)
        {
            if (!player) return;
            var width = ___m_inventory.GetWidth();

            ValheimOffload.PopulateParsedConfigValues();

            foreach (var allItem in ___m_inventory.GetAllItems())
            {
                if (!ValheimOffload.IgnoredItemSlots.Contains(allItem.m_gridPos)) continue;

                var index = allItem.m_gridPos.y * width + allItem.m_gridPos.x;
                ___m_elements[index].m_queued.enabled = true;
            }
        }
    }
}
