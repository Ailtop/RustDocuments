namespace Oxide.Plugins
{
    [Info("Healthy Guns", "Wulf/lukespragg/Arainrr", "3.0.3", ResourceId = 2262)]
    [Description("Makes weapons in barrels/crates spawn in good condition")]
    public class HealthyGuns : RustPlugin
    {
        private void Init() => Unsubscribe(nameof(OnLootSpawn));

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnLootSpawn));
            foreach (var baseNetworkable in BaseNetworkable.serverEntities)
            {
                if (baseNetworkable is LootContainer)
                    OnLootSpawn(baseNetworkable as LootContainer);
            }
        }

        private void OnLootSpawn(LootContainer lootContainer)
        {
            if (lootContainer == null || lootContainer.OwnerID != 0) return;
            if (lootContainer.SpawnType == LootContainer.spawnType.ROADSIDE || lootContainer.SpawnType == LootContainer.spawnType.TOWN)
                NextTick(() => RepairContainerContents(lootContainer));
        }

        private void RepairContainerContents(LootContainer lootContainer)
        {
            if (lootContainer?.inventory?.itemList?.Count > 0)
            {
                foreach (var item in lootContainer.inventory.itemList)
                {
                    if (item.hasCondition && !item.condition.Equals(item.info.condition.max) && ItemManager.FindItemDefinition(item.info.itemid)?.category == ItemCategory.Weapon)
                        item.condition = item.info.condition.max;
                }
            }
        }
    }
}