using Oxide.Core;
using UnityEngine;

public class SolarPanel : IOEntity
{
	public Transform sunSampler;

	private const int tickrateSeconds = 60;

	public int maximalPowerOutput = 10;

	public float dot_minimum = 0.1f;

	public float dot_maximum = 0.6f;

	public override bool IsRootEntity()
	{
		return true;
	}

	public override int MaximalPowerOutput()
	{
		return maximalPowerOutput;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRandomized(SunUpdate, 1f, 5f, 2f);
	}

	public void SunUpdate()
	{
		int currentEnergy = base.currentEnergy;
		if (TOD_Sky.Instance.IsNight)
		{
			currentEnergy = 0;
		}
		else
		{
			Vector3 normalized = (TOD_Sky.Instance.Components.Sun.transform.position - sunSampler.transform.position).normalized;
			float value = Vector3.Dot(sunSampler.transform.forward, normalized);
			float num = Mathf.InverseLerp(dot_minimum, dot_maximum, value);
			if (num > 0f && !IsVisible(sunSampler.transform.position + normalized * 100f, 101f))
			{
				num = 0f;
			}
			currentEnergy = Mathf.FloorToInt((float)maximalPowerOutput * num * base.healthFraction);
		}
		if (Interface.CallHook("OnSolarPanelSunUpdate", this, currentEnergy) == null)
		{
			bool num2 = base.currentEnergy != currentEnergy;
			base.currentEnergy = currentEnergy;
			if (num2)
			{
				MarkDirty();
			}
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (outputSlot != 0)
		{
			return 0;
		}
		return currentEnergy;
	}
}
