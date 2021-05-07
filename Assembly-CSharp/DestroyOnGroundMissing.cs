using Oxide.Core;
using UnityEngine;

public class DestroyOnGroundMissing : MonoBehaviour, IServerComponent
{
	private void OnGroundMissing()
	{
		BaseEntity baseEntity = base.gameObject.ToBaseEntity();
		if (baseEntity != null && Interface.CallHook("OnEntityGroundMissing", baseEntity) == null)
		{
			BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
			if (baseCombatEntity != null)
			{
				baseCombatEntity.Die();
			}
			else
			{
				baseEntity.Kill(BaseNetworkable.DestroyMode.Gib);
			}
		}
	}
}
