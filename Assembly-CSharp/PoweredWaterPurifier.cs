using UnityEngine;

public class PoweredWaterPurifier : WaterPurifier
{
	public float ConvertInterval = 5f;

	public int PowerDrain = 5;

	public Material PoweredMaterial;

	public Material UnpoweredMaterial;

	public MeshRenderer TargetRenderer;

	public override void ResetState()
	{
		base.ResetState();
	}

	protected override void SpawnStorageEnt(bool load)
	{
		if (load)
		{
			foreach (BaseEntity child in children)
			{
				LiquidContainer waterStorage;
				if ((object)(waterStorage = (child as LiquidContainer)) != null)
				{
					base.waterStorage = waterStorage;
				}
			}
		}
		if (base.waterStorage != null)
		{
			base.waterStorage.SetConnectedTo(this);
			return;
		}
		base.waterStorage = (GameManager.server.CreateEntity(storagePrefab.resourcePath, storagePrefabAnchor.position, storagePrefabAnchor.rotation) as LiquidContainer);
		base.waterStorage.SetParent(this, true);
		base.waterStorage.Spawn();
		base.waterStorage.SetConnectedTo(this);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (HasLiquidItem())
		{
			if (HasFlag(Flags.Reserved8) && !IsInvoking(ConvertWater))
			{
				InvokeRandomized(ConvertWater, ConvertInterval, ConvertInterval, ConvertInterval * 0.1f);
			}
		}
		else if (IsInvoking(ConvertWater))
		{
			CancelInvoke(ConvertWater);
		}
	}

	private void ConvertWater()
	{
		if (HasDirtyWater())
		{
			ConvertWater(ConvertInterval);
		}
	}

	public override int ConsumptionAmount()
	{
		return PowerDrain;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!base.isServer)
		{
			return;
		}
		if (old.HasFlag(Flags.Reserved8) != next.HasFlag(Flags.Reserved8))
		{
			if (next.HasFlag(Flags.Reserved8))
			{
				if (!IsInvoking(ConvertWater))
				{
					InvokeRandomized(ConvertWater, ConvertInterval, ConvertInterval, ConvertInterval * 0.1f);
				}
			}
			else if (IsInvoking(ConvertWater))
			{
				CancelInvoke(ConvertWater);
			}
		}
		if (waterStorage != null)
		{
			waterStorage.SetFlag(Flags.Reserved8, HasFlag(Flags.Reserved8));
		}
	}
}
