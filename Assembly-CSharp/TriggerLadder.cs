using UnityEngine;

public class TriggerLadder : TriggerBase
{
	public enum LadderType
	{
		Rungs = 0,
		Rope = 1
	}

	public LadderType Type;

	public override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (baseEntity as BasePlayer == null)
		{
			return null;
		}
		return baseEntity.gameObject;
	}
}
