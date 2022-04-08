using System;
using UnityEngine;

namespace Rust.Modular;

[Serializable]
public class VehicleModuleSlidingComponent
{
	[Serializable]
	public class SlidingPart
	{
		public Transform transform;

		public Vector3 openPosition;

		public Vector3 closedPosition;
	}

	public string interactionColliderName = "MyCollider";

	public BaseEntity.Flags flag_SliderOpen = BaseEntity.Flags.Reserved3;

	public float moveTime = 1f;

	public SlidingPart[] slidingParts;

	public SoundDefinition openSoundDef;

	public SoundDefinition closeSoundDef;

	private float positionPercent;

	public bool WantsOpenPos(BaseEntity parentEntity)
	{
		return parentEntity.HasFlag(flag_SliderOpen);
	}

	public void Use(BaseVehicleModule parentModule)
	{
		parentModule.SetFlag(flag_SliderOpen, !WantsOpenPos(parentModule));
	}

	public void ServerUpdateTick(BaseVehicleModule parentModule)
	{
		CheckPosition(parentModule, Time.fixedDeltaTime);
	}

	private void CheckPosition(BaseEntity parentEntity, float dt)
	{
		bool flag = WantsOpenPos(parentEntity);
		if ((flag && positionPercent == 1f) || (!flag && positionPercent == 0f))
		{
			return;
		}
		float num = (flag ? (dt / moveTime) : (0f - dt / moveTime));
		positionPercent = Mathf.Clamp01(positionPercent + num);
		SlidingPart[] array = slidingParts;
		foreach (SlidingPart slidingPart in array)
		{
			if (!(slidingPart.transform == null))
			{
				slidingPart.transform.localPosition = Vector3.Lerp(slidingPart.closedPosition, slidingPart.openPosition, positionPercent);
			}
		}
	}
}
