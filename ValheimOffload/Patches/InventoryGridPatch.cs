using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using UnityEngine;

namespace ValheimOffload.Patches
{
    [HarmonyPatch(typeof(InventoryGrid))]
    public static class InventoryGridPatch
    {
        [HarmonyPatch(nameof(InventoryGrid.UpdateGui))]
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

            foreach (var item in ___m_inventory.GetAllItems().Where(allItem => ValheimOffload.IgnoredItemSlots.Contains(allItem.m_gridPos)))
            {
                if (!ValheimOffload.IgnoredItemSlots.Contains(item.m_gridPos)) continue;

                var index = item.m_gridPos.y * width + item.m_gridPos.x;
                ___m_elements[index].m_queued.enabled = true;
            }
        }

        [HarmonyPatch(nameof(InventoryGrid.OnLeftClick))]
        [HarmonyPrefix]
        internal static bool OnLeftClick(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            if (!ShouldPerformAction(out var pressedKey)) return true;
            Jotunn.Logger.LogInfo($"Pressed Key: {pressedKey}");

            var buttonPos = __instance.GetButtonPos(clickHandler.gameObject);

            ValheimOffload.ToggleIgnoredSlot(buttonPos);
            return false;
        }
        
        [HarmonyPatch(nameof(InventoryGrid.OnRightClick))]
        [HarmonyPrefix]
        internal static bool OnRightClick(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            if (!ShouldPerformAction(out var pressedKey)) return true;
            Jotunn.Logger.LogInfo($"Pressed Key: {pressedKey}");
            
            var buttonPos = __instance.GetButtonPos(clickHandler.gameObject);
            var item = __instance.GetInventory().GetItemAt(buttonPos.x, buttonPos.y);

            ValheimOffload.ToggleIgnoredItem(item.m_shared.m_name);
            return false;
        }

        private static bool ShouldPerformAction(out KeyCode pressedKey)
        {
            pressedKey = KeyCode.None;
            
            var localPlayer = Player.m_localPlayer;
            if (!localPlayer || localPlayer.IsTeleporting() || InventoryGui.instance.m_dragGo) return false;

            // TODO: If modifier key - add left/right
            
            // TODO: Change this to pull from config - InventoryModifierKey
            return AnyKeyPressed(new List<KeyCode> { KeyCode.LeftAlt, KeyCode.RightAlt }, out pressedKey);
        }

        private static bool AnyKeyPressed(List<KeyCode> keyCodes, out KeyCode pressedKey)
        {
            foreach(var key in keyCodes)
            {
                pressedKey = key;
                if (Input.GetKey(key)) return true;
            }

            pressedKey = KeyCode.None;
            return false;
        }
    }
}
