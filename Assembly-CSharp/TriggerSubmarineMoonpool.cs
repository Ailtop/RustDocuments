using UnityEngine;

public class TriggerSubmarineMoonpool : TriggerBase, IServerComponent
{
	internal override GameObject InterestedInObject(GameObject obj)
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
		BaseSubmarine baseSubmarine;
		if (baseEntity.isServer && (object)(baseSubmarine = baseEntity as BaseSubmarine) != null)
		{
			return baseSubmarine.gameObject;
		}
		return null;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		BaseSubmarine baseSubmarine;
		if ((object)(baseSubmarine = ent as BaseSubmarine) != null)
		{
			baseSubmarine.OnSurfacedInMoonpool();
		}
	}
}
