using UnityEngine;

namespace ValheimOffload.Extensions
{
    public static class Vector2iExtensions
    {
        public static bool IsValidForInventoryGrid(this Vector2i value)
        {
            return value.x >= 0 && value.y >= 0;
        }

        public static Vector2i Clamp(this Vector2i value, int minX, int maxX, int minY, int maxY)
        {
            return new Vector2i(Mathf.Clamp(value.x, minX, maxX), Mathf.Clamp(value.y, minY, maxY));
        }
    }
}
