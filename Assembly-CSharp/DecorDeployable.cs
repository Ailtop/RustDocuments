using UnityEngine;

public class DecorDeployable : DecayEntity, TimedExplosive.IPreventSticking
{
	public bool CanStickTo(Collider collider)
	{
		return false;
	}
}
