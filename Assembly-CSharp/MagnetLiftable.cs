using UnityEngine;

public class MagnetLiftable : EntityComponent<BaseEntity>
{
	public ItemAmount[] shredResources;

	public Vector3 shredDirection = Vector3.forward;

	public virtual void SetMagnetized(bool wantsOn, BaseMagnet magnetSource)
	{
	}
}
