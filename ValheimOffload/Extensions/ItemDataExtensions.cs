﻿namespace ValheimOffload.Extensions
{
    public static class ItemDataExtensions
    {
        public static bool IsFullStack(this ItemDrop.ItemData item)
        {
            return item.m_stack >= item.m_shared.m_maxStackSize;
        }

        public static int GetAmountToFullStack(this ItemDrop.ItemData item)
        {
            return item.m_shared.m_maxStackSize - item.m_stack;
        }

        public static bool IsFood(this ItemDrop.ItemData item)
        {
            return item.m_shared.m_food > 0.0 || item.m_shared.m_foodStamina > 0.0;
        }
    }
}
