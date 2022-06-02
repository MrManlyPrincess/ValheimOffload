using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.Events;
using ValheimOffload.Extensions;

namespace ValheimOffload
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.Minor)]
    internal class ValheimOffload : BaseUnityPlugin
    {
        public const string PluginGUID = "com.mrmanlyprincess.ValheimOffload";
        public const string PluginName = "Valheim Offload";
        public const string PluginVersion = "0.0.6";

        private static CustomLocalization ModLocalization = LocalizationManager.Instance.GetLocalization();

        #region Config

        private static ConfigEntry<bool> ModEnabled;

        private static ConfigEntry<float> OffloadRadius;

        #region Offload Button

        private ConfigEntry<KeyboardShortcut> OffloadButtonConfig;
        private ButtonConfig OffloadButton;

        #endregion

        private static ConfigEntry<bool> IgnoreConsumables;
        private static ConfigEntry<bool> IgnoreUnequippedAmmo;

        private static ConfigEntry<string> IgnoredItemSlotsRaw;
        private static ConfigEntry<string> IgnoredItemsRaw;

        #endregion

        public static List<Container> AllContainers = new List<Container>();

        private UnityEvent m_OnOffloadButtonPressed = new UnityEvent();

        public static List<Vector2i> IgnoredItemSlots;
        public static List<string> IgnoredItems;

        private void Awake()
        {
            CreateConfigValues();
            if (!ModEnabled.Value) return;

            m_OnOffloadButtonPressed.AddListener(OffloadIntoNearbyContainers);

            AddLocalizations();

            AddInputs();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Update()
        {
            if (!ModEnabled.Value) return;
            HandleInput();
        }

        #region Setup

        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;
            Config.ConfigReloaded += ReloadConfig;
            Config.SettingChanged += UpdateSettings;

            ModEnabled = Config.Bind("General", "Enabled", true, "Enable this mod");
            OffloadRadius = Config.Bind("General", "Offload Radius", 15f,
                "Radius in which to search for containers");

            OffloadButtonConfig = Config.Bind("Keybindings", "Offload Button (Keyboard)",
                new KeyboardShortcut(KeyCode.Tilde),
                new ConfigDescription("KeyboardShortcut for OffloadButton keybinding. Modifiers are ignored."));

            // OffloadButtonGamepadConfig = Config.Bind("Keybindings", "Offload Button (Gamepad)",
            //     InputManager.GamepadButton.LeftShoulder,
            //     new ConfigDescription("Gamepad button for OffloadButton keybinding"));

            IgnoreConsumables = Config.Bind("Ignore Options", "Ignore Consumables", true,
                "If true, consumable items will not be offloaded");

            IgnoreUnequippedAmmo = Config.Bind("Ignore Options", "Ignore Unequipped Ammo", true,
                "If true, unequipped ammo will not be offloaded");

            IgnoredItemSlotsRaw = Config.Bind("Ignore Options", "Item slots to ignore", "0,2|1,2|2,2|0,3|1,3|2,3|",
                "A pipe-delimited list of item slots to ignore. Each slot is referenced by a zero-based Vector2. " +
                "First slot is '0,0', last slot is '3,7'." +
                "If a value in the Vector2 is left blank, the entire column/row will be affected.");

            IgnoredItemsRaw = Config.Bind("Ignore Options", "Items to ignore", string.Empty,
                "A comma-delimited list of item names to ignore. Examples: '$item_hammer, $item_hoe, $item_leatherscraps'");
        }

        private void ReloadConfig(object sender, EventArgs args)
        {
            Jotunn.Logger.LogInfo($"ReloadConfig => {sender}, {args}");
            PopulateParsedConfigValues(true);
        }

        private void UpdateSettings(object sender, SettingChangedEventArgs args)
        {
            Jotunn.Logger.LogInfo($"UpdateSettings => {sender}, {args}");
            PopulateParsedConfigValues(true);
        }

        private void AddInputs()
        {
            OffloadButton = new ButtonConfig
            {
                Name = "Offload Button",
                ShortcutConfig = OffloadButtonConfig,
                // GamepadConfig = OffloadButtonGamepadConfig,
                ActiveInGUI = true,
                HintToken = "$offload_button_text"
            };

            InputManager.Instance.AddButton(PluginGUID, OffloadButton);
        }

        private void AddLocalizations()
        {
            ModLocalization.AddTranslation("English", new Dictionary<string, string>
            {
                { "offload_button_text", "Offload" }
            });
        }

        private void HandleInput()
        {
            if ((Chat.instance && Chat.instance.m_input.isFocused) || Minimap.InTextInput()) return;
            if (!Input.GetKeyDown(OffloadButton.Shortcut.MainKey)) return;

            PopulateParsedConfigValues();

            Jotunn.Logger.LogInfo("Hit OffloadButton");
            m_OnOffloadButtonPressed.Invoke();
        }

        public static void PopulateParsedConfigValues(bool force = false)
        {
            if (!Player.m_localPlayer) return;

            if (force || IgnoredItemSlots == null)
            {
                var maxWidth = Player.m_localPlayer.m_inventory.m_width;
                var maxHeight = Player.m_localPlayer.m_inventory.m_height;

                IgnoredItemSlots = Utils.GetItemSlotsFromConfig(IgnoredItemSlotsRaw.Value, maxWidth, maxHeight);
            }

            if (force || IgnoredItems == null)
            {
                IgnoredItems = Utils.GetItemsFromConfig(IgnoredItemsRaw.Value);
            }
        }

        #endregion

        #region Offload

        private void OffloadIntoNearbyContainers()
        {
            if (!Player.m_localPlayer) return;

            Jotunn.Logger.LogInfo("Entered OffloadIntoNearbyContainers");
            var items = GetOffloadableItems();

            var totalItemsStored = 0;
            var totalContainersStoredIn = 0;

            var containersInRadius = Utils.GetContainersInRadius(OffloadRadius.Value)
                .Where(container => container.IsValidForOffloading())
                .ToList();

            Jotunn.Logger.LogInfo(
                $"Doing Offload, {AllContainers.Count} total containers and {containersInRadius.Count} in radius");

            foreach (var container in containersInRadius)
            {
                container.m_nview.ClaimOwnership();

                container.SetInUse(true);
                container.OnContainerChanged();

                var includeContainerInCount = OffloadIntoContainer(items, container, ref totalItemsStored);
                if (includeContainerInCount) totalContainersStoredIn++;

                container.SetInUse(false);

                container.GetInventory().Changed();
                container.OnContainerChanged();
            }

            Player.m_localPlayer.GetInventory().Changed();

            HandleOffloadMessage(totalItemsStored, totalContainersStoredIn, containersInRadius.Count);
        }

        private List<ItemDrop.ItemData> GetOffloadableItems()
        {
            return Player.m_localPlayer.GetInventory().GetAllItems()
                .Where(item =>
                    item != null &&
                    !item.m_equiped &&
                    item.m_shared.m_maxStackSize != 1 &&
                    (!IgnoreConsumables.Value || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable) &&
                    (!IgnoreUnequippedAmmo.Value || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Ammo) &&
                    !IgnoredItems.Contains(item.m_shared.m_name) &&
                    !IgnoredItemSlots.Contains(item.m_gridPos)
                )
                .ToList();
        }

        private static void HandleOffloadMessage(int totalItemsStored, int totalContainersStoredIn,
            int validContainerCount)
        {
            if (totalItemsStored > 0)
            {
                Utils.ShowCenterMessage($"Stored {totalItemsStored} items in {totalContainersStoredIn} containers");
            }
            else if (totalItemsStored == 0 && validContainerCount > 0)
            {
                Utils.ShowCenterMessage("No items were stored");
            }
            else
            {
                Utils.ShowCenterMessage("No valid containers");
            }
        }

        private bool OffloadIntoContainer(List<ItemDrop.ItemData> items, Container container, ref int totalItemsStored)
        {
            var includeContainerInCount = false;
            foreach (var item in items)
            {
                var mutableItem = item;
                if (ProcessItem(Player.m_localPlayer.GetInventory(), container, ref mutableItem,
                        ref totalItemsStored))
                {
                    includeContainerInCount = true;
                }
            }

            return includeContainerInCount;
        }

        private bool ProcessItem(Inventory inventory, Container container, ref ItemDrop.ItemData item,
            ref int itemsMoved)
        {
            // Omit items that we always want to ignore
            if (item == null || item.m_equiped || item.m_shared.m_maxStackSize == 1) return false;

            var itemName = item.m_shared.m_name;

            var containerInventory = container.GetInventory();
            var containerItems = containerInventory.GetAllItems();

            if (containerItems.All(containerItem => containerItem.m_shared.m_name != itemName)) return false;
            Jotunn.Logger.LogDebug($"Found chest with {itemName}");

            itemsMoved += FillPartialStacks(inventory, containerInventory, ref item);
            itemsMoved += FillEmptyStack(inventory, containerInventory, ref item);

            var movedItems = itemsMoved > 0;

            if (movedItems)
            {
                Jotunn.Logger.LogInfo(
                    $"Stored {itemsMoved} {Localization.instance.Localize(item.m_shared.m_name)} in {Localization.instance.Localize(container.m_name)}");
            }

            return movedItems;
        }

        private int FillEmptyStack(Inventory inventory, Inventory containerInventory, ref ItemDrop.ItemData item)
        {
            if (item == null || item.m_stack == 0) return 0;

            var emptySlot = containerInventory.FindEmptySlot(true);
            if (!emptySlot.IsValidForInventoryGrid()) return 0;

            containerInventory.MoveItemToThis(inventory, item);
            return item.m_stack;
        }

        private int FillPartialStacks(Inventory inventory, Inventory containerInventory, ref ItemDrop.ItemData item)
        {
            var partialStack = containerInventory.FindFreeStackItem(item.m_shared.m_name, item.m_quality);
            var amountMoved = 0;

            string StackSizePrintout(ItemDrop.ItemData itemData) =>
                $"{itemData.m_stack}/{itemData.m_shared.m_maxStackSize}";

            while (item.m_stack > 0 && partialStack != null && !partialStack.IsFullStack())
            {
                Jotunn.Logger.LogDebug($"Found partial stack of {item.m_shared.m_name}," +
                                       $" {StackSizePrintout(partialStack)}");

                var amountToMove = Mathf.Min(item.m_stack, partialStack.GetAmountToFullStack());

                containerInventory.MoveItemToThis(inventory, item, amountToMove, partialStack.m_gridPos.x,
                    partialStack.m_gridPos.y);

                Jotunn.Logger.LogDebug($"Added {amountToMove} to partial stack of {item.m_shared.m_name}," +
                                       $" {StackSizePrintout(partialStack)}");

                amountMoved += amountToMove;
                partialStack = containerInventory.FindFreeStackItem(item.m_shared.m_name, item.m_quality);
            }

            return amountMoved;
        }

        #endregion
    }
}
