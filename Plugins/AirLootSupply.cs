namespace Oxide.Plugins
{
    [Info("Air Loot Supply", "Arainrr", "1.0.0")]
    [Description("Allow supply to be loot when it's dropping")]
    public class AirLootSupply : RustPlugin
    {
        private void OnServerInitialized()
        {
            foreach (var supplyDrop in UnityEngine.Object.FindObjectsOfType<SupplyDrop>())
                OnEntitySpawned(supplyDrop);
        }

        private void OnEntitySpawned(SupplyDrop supplyDrop)
        {
            if (supplyDrop == null) return;
            supplyDrop.MakeLootable();
        }
    }
}