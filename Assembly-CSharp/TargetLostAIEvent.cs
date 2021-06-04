using UnityEngine;

public class TargetLostAIEvent : BaseAIEvent
{
	public float Range { get; set; }

	public TargetLostAIEvent()
		: base(AIEventType.TargetLost)
	{
		base.Rate = ExecuteRate.Fast;
	}

	public override void Execute(AIMemory memory, AIBrainSenses senses, StateStatus stateStatus)
	{
		base.Result = base.Inverted;
		BaseEntity baseEntity = memory.Entity.Get(base.InputEntityMemorySlot);
		if (baseEntity == null)
		{
			base.Result = !base.Inverted;
		}
		else if (Vector3.Distance(baseEntity.transform.position, base.Owner.transform.position) > senses.TargetLostRange)
		{
			base.Result = !base.Inverted;
		}
		else if (baseEntity.Health() <= 0f)
		{
			base.Result = !base.Inverted;
		}
		else if (senses.ignoreSafeZonePlayers)
		{
			BasePlayer basePlayer = baseEntity as BasePlayer;
			if (basePlayer != null && basePlayer.InSafeZone())
			{
				base.Result = !base.Inverted;
			}
		}
	}
}
