using UnityEngine;

public interface IItemContainerEntity
{
	ItemContainer inventory { get; }

	Transform Transform { get; }

	bool DropsLoot { get; }

	bool DropFloats { get; }

	void DropItems(BaseEntity initiator = null);

	bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true);

	bool ShouldDropItemsIndividually();

	void DropBonusItems(BaseEntity initiator, ItemContainer container);

	Vector3 GetDropPosition();
}
