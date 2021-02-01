using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class Sprinkler : IOEntity
{
	public float SplashFrequency = 1f;

	public Transform Eyes;

	public int WaterPerSplash = 1;

	public float DecayPerSplash = 0.8f;

	private ItemDefinition currentFuelType;

	private IOEntity currentFuelSource;

	private HashSet<ISplashable> cachedSplashables = new HashSet<ISplashable>();

	private TimeSince updateSplashableCache;

	private bool forceUpdateSplashables;

	public override bool BlockFluidDraining => currentFuelSource != null;

	public override int ConsumptionAmount()
	{
		return 2;
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		SetSprinklerState(inputAmount > 0);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return inputAmount;
	}

	private void DoSplash()
	{
		using (TimeWarning.New("SprinklerSplash"))
		{
			int num = WaterPerSplash;
			if ((float)updateSplashableCache > SplashFrequency * 4f || forceUpdateSplashables)
			{
				cachedSplashables.Clear();
				forceUpdateSplashables = false;
				updateSplashableCache = 0f;
				Vector3 position = Eyes.position;
				Vector3 up = base.transform.up;
				float sprinklerEyeHeightOffset = Server.sprinklerEyeHeightOffset;
				float value = Vector3.Angle(up, Vector3.up) / 180f;
				value = Mathf.Clamp(value, 0.2f, 1f);
				sprinklerEyeHeightOffset *= value;
				Vector3 startPosition = position + up * (Server.sprinklerRadius * 0.5f);
				Vector3 endPosition = position + up * sprinklerEyeHeightOffset;
				List<BaseEntity> obj = Facepunch.Pool.GetList<BaseEntity>();
				Vis.Entities(startPosition, endPosition, Server.sprinklerRadius, obj, 1236478737);
				if (obj.Count > 0)
				{
					foreach (BaseEntity item in obj)
					{
						ISplashable splashable;
						IOEntity entity;
						if (!item.isClient && (splashable = item as ISplashable) != null && !cachedSplashables.Contains(splashable) && splashable.WantsSplash(currentFuelType, num) && item.IsVisible(position) && ((object)(entity = item as IOEntity) == null || !IsConnectedTo(entity, IOEntity.backtracking)))
						{
							cachedSplashables.Add(splashable);
						}
					}
				}
				Facepunch.Pool.FreeList(ref obj);
			}
			if (cachedSplashables.Count > 0)
			{
				int amount = num / cachedSplashables.Count;
				foreach (ISplashable cachedSplashable in cachedSplashables)
				{
					if (!ObjectEx.IsUnityNull(cachedSplashable) && cachedSplashable.WantsSplash(currentFuelType, amount))
					{
						int num2 = cachedSplashable.DoSplash(currentFuelType, amount);
						num -= num2;
						if (num <= 0)
						{
							break;
						}
					}
				}
			}
			if (DecayPerSplash > 0f)
			{
				Hurt(DecayPerSplash);
			}
		}
	}

	public void SetSprinklerState(bool wantsOn)
	{
		if (wantsOn)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	public void TurnOn()
	{
		if (!IsOn())
		{
			SetFlag(Flags.On, true);
			forceUpdateSplashables = true;
			if (!IsInvoking(DoSplash))
			{
				InvokeRandomized(DoSplash, SplashFrequency * 0.5f, SplashFrequency, SplashFrequency * 0.2f);
			}
		}
	}

	public void TurnOff()
	{
		if (IsOn())
		{
			SetFlag(Flags.On, false);
			if (IsInvoking(DoSplash))
			{
				CancelInvoke(DoSplash);
			}
			currentFuelSource = null;
			currentFuelType = null;
		}
	}

	public override void SetFuelType(ItemDefinition def, IOEntity source)
	{
		base.SetFuelType(def, source);
		currentFuelType = def;
		currentFuelSource = source;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			SetFlag(Flags.On, false, false, false);
		}
	}
}
