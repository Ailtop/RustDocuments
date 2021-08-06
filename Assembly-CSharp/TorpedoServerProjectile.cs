using UnityEngine;

public class TorpedoServerProjectile : ServerProjectile
{
	[Tooltip("Make sure to leave some allowance for waves, which affect the true depth.")]
	[SerializeField]
	private float minWaterDepth = 0.5f;

	[SerializeField]
	private float upwardDrift = 10f;

	[SerializeField]
	private float surfaceSpeed = 18f;

	private float initialGravModifier;

	protected override int mask => 1236478721;

	protected void Awake()
	{
		initialGravModifier = gravityModifier;
	}

	public override bool DoMovement()
	{
		if (!base.DoMovement())
		{
			return false;
		}
		float num = WaterLevel.GetWaterInfo(base.transform.position).surfaceLevel - base.transform.position.y;
		if (num < -1f)
		{
			gravityModifier = 1f;
		}
		else if (num <= minWaterDepth)
		{
			float value = minWaterDepth - num;
			gravityModifier = Mathf.Clamp01(value);
		}
		else
		{
			float num2 = minWaterDepth + currentVelocity.y * currentVelocity.y / 25f;
			if ((double)currentVelocity.y > 0.0 && num < num2)
			{
				currentVelocity.y -= 25f * Time.fixedDeltaTime;
				gravityModifier = 0f;
			}
			else
			{
				gravityModifier = initialGravModifier;
				if (currentVelocity.y < upwardDrift)
				{
					currentVelocity.y += 15f * Time.fixedDeltaTime;
				}
			}
		}
		if (num < minWaterDepth + 1f && currentVelocity.magnitude < surfaceSpeed)
		{
			currentVelocity += currentVelocity.normalized * 10f * Time.fixedDeltaTime;
		}
		return true;
	}
}
