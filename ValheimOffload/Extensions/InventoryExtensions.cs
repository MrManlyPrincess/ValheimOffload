using System.Linq;

namespace ValheimOffload.Extensions
{
    public static class InventoryExtensions
    {
        public static bool IsFull(this Inventory inventory)
        {
            return !inventory.HaveEmptySlot() && inventory.GetAllItems().All(item => item.IsFullStack());
        }
    }
}
