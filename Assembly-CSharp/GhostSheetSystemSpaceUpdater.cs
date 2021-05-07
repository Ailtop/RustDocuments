using EZhex1991.EZSoftBone;
using UnityEngine;

[RequireComponent(typeof(EZSoftBone))]
public class GhostSheetSystemSpaceUpdater : MonoBehaviour, IClientComponent
{
	private EZSoftBone[] ezSoftBones;

	private BasePlayer player;

	public void Awake()
	{
		ezSoftBones = GetComponents<EZSoftBone>();
		player = base.gameObject.ToBaseEntity() as BasePlayer;
	}

	public void Update()
	{
		if (ezSoftBones == null || ezSoftBones.Length == 0 || player == null)
		{
			return;
		}
		BaseMountable mounted = player.GetMounted();
		if (mounted != null)
		{
			SetSimulateSpace(mounted.transform, false);
			return;
		}
		BaseEntity parentEntity = player.GetParentEntity();
		if (parentEntity != null)
		{
			SetSimulateSpace(parentEntity.transform, true);
		}
		else
		{
			SetSimulateSpace(null, true);
		}
	}

	private void SetSimulateSpace(Transform transform, bool collisionEnabled)
	{
		for (int i = 0; i < ezSoftBones.Length; i++)
		{
			EZSoftBone obj = ezSoftBones[i];
			obj.simulateSpace = transform;
			obj.collisionEnabled = collisionEnabled;
		}
	}
}
