using UnityEngine;

public class IndustrialStorageAdaptor : IndustrialEntity, IIndustrialStorage
{
	public GameObject GreenLight;

	public GameObject RedLight;

	public ItemContainer Container => (GetParentEntity() as StorageContainer)?.inventory;

	public BaseEntity IndustrialEntity => this;

	public Vector2i InputSlotRange(int slotIndex)
	{
		if (GetParentEntity() is IIndustrialStorage industrialStorage)
		{
			return industrialStorage.InputSlotRange(slotIndex);
		}
		if (GetParentEntity() is Locker locker)
		{
			Vector3 localPosition = base.transform.localPosition;
			return locker.GetIndustrialSlotRange(localPosition);
		}
		return new Vector2i(0, Container.capacity - 1);
	}

	public Vector2i OutputSlotRange(int slotIndex)
	{
		if (GetParentEntity() is DropBox)
		{
			return new Vector2i(0, Container.capacity - 2);
		}
		if (GetParentEntity() is IIndustrialStorage industrialStorage)
		{
			return industrialStorage.OutputSlotRange(slotIndex);
		}
		if (GetParentEntity() is Locker locker)
		{
			Vector3 localPosition = base.transform.localPosition;
			return locker.GetIndustrialSlotRange(localPosition);
		}
		return new Vector2i(0, Container.capacity - 1);
	}

	public void OnStorageItemTransferBegin()
	{
		if (GetParentEntity() is VendingMachine vendingMachine)
		{
			vendingMachine.OnIndustrialItemTransferBegins();
		}
	}

	public void OnStorageItemTransferEnd()
	{
		if (GetParentEntity() is VendingMachine vendingMachine)
		{
			vendingMachine.OnIndustrialItemTransferEnds();
		}
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public void ClientNotifyItemAddRemoved(bool add)
	{
		if (add)
		{
			GreenLight.SetActive(value: false);
			GreenLight.SetActive(value: true);
		}
		else
		{
			RedLight.SetActive(value: false);
			RedLight.SetActive(value: true);
		}
	}
}
