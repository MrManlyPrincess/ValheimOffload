using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ValheimOffload.Extensions;

namespace ValheimOffload
{
    public static class Utils
    {
        public static void ShowCenterMessage(string message)
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, message);
        }

        public static List<Container> GetContainersInRadius(double radius)
        {
            var playerPosition = Player.m_localPlayer.transform.position;

            return ValheimOffload.AllContainers
                .Where(container => container != null &&
                                    playerPosition.WithinDistance(container.transform.position, radius))
                .ToList();
        }

        public static List<string> GetItemsFromConfig(string combinedItems)
        {
            if (string.IsNullOrWhiteSpace(combinedItems)) return new List<string>();
            return combinedItems.Split(',')
                .Select(item => item.Trim())
                .ToList();
        }

        public static List<Vector2i> GetItemSlotsFromConfig(string combinedItemSlots, int maxWidth, int maxHeight)
        {
            var ignoredItemSlots = new List<Vector2i>();

            if (string.IsNullOrWhiteSpace(combinedItemSlots)) return ignoredItemSlots;
            var itemSlotGroups = combinedItemSlots.Split('|')
                .Select(item => item.Trim())
                .ToList();

            foreach (var coordGroup in itemSlotGroups)
            {
                var coords = coordGroup.Split(',');
                if (coords.Length != 2) continue;

                var x = ParseAndClampInt(coords[0], 0, maxWidth);
                var y = ParseAndClampInt(coords[1], 0, maxHeight);

                if (!x.HasValue && !y.HasValue) continue;

                var minX = x ?? 0;
                var maxX = x ?? maxWidth;

                var minY = y ?? 0;
                var maxY = y ?? maxHeight;

                for (var width = minX; width <= maxX; width++)
                {
                    for (var height = minY; height <= maxY; height++)
                    {
                        ignoredItemSlots.Add(new Vector2i(width, height));
                    }
                }
            }

            return ignoredItemSlots
                .Distinct()
                .ToList();
        }

        private static int? ParseAndClampInt(string value, int min, int max)
        {
            if (int.TryParse(value, out var outValue)) return Mathf.Clamp(outValue, min, max);
            return null;
        }
    }
}
