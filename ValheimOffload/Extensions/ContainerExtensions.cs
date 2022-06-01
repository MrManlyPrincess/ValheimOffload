namespace ValheimOffload.Extensions
{
    public static class ContainerExtensions
    {
        public static bool IsValidForOffloading(this Container container)
        {
            if (container.IsInUse() || container.m_open.activeSelf)
            {
                Jotunn.Logger.LogInfo("Skipping container that is 'in-use'");
                return false;
            }

            if (!container.CheckAccess(Player.m_localPlayer.GetPlayerID()))
            {
                Jotunn.Logger.LogDebug("Skipping container that local player does not have access to");
                return false;
            }

            if (container.GetInventory().IsFull())
            {
                Jotunn.Logger.LogDebug("Skipping container that is completely full");
                return false;
            }

            return true;
        }
    }
}
