using UnityEngine;

public class EnvironmentVolume : MonoBehaviour
{
	[InspectorFlags]
	public EnvironmentType Type = EnvironmentType.Underground;

	public Vector3 Center = Vector3.zero;

	public Vector3 Size = Vector3.one;

	public Collider trigger { get; private set; }

	protected virtual void Awake()
	{
		UpdateTrigger();
	}

	public void UpdateTrigger()
	{
		if (!trigger)
		{
			trigger = base.gameObject.GetComponent<Collider>();
		}
		if (!trigger)
		{
			trigger = base.gameObject.AddComponent<BoxCollider>();
		}
		trigger.isTrigger = true;
		BoxCollider boxCollider = trigger as BoxCollider;
		if ((bool)boxCollider)
		{
			boxCollider.center = Center;
			boxCollider.size = Size;
		}
	}
}
