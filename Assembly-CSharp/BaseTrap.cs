using UnityEngine;

public class BaseTrap : DecayEntity
{
	public virtual void ObjectEntered(GameObject obj)
	{
	}

	public virtual void Arm()
	{
		SetFlag(Flags.On, true);
		SendNetworkUpdate();
	}

	public virtual void OnEmpty()
	{
	}
}
