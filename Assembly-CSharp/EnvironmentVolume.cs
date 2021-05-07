using UnityEngine;

public class EnvironmentVolume : MonoBehaviour
{
	[InspectorFlags]
	public EnvironmentType Type = EnvironmentType.Underground;

	public Vector3 Center = Vector3.zero;

	public Vector3 Size = Vector3.one;

	public BoxCollider trigger
	{
		get;
		private set;
	}

	protected virtual void Awake()
	{
		UpdateTrigger();
	}

	public void UpdateTrigger()
	{
		if (!trigger)
		{
			trigger = base.gameObject.GetComponent<BoxCollider>();
		}
		if (!trigger)
		{
			trigger = base.gameObject.AddComponent<BoxCollider>();
		}
		trigger.isTrigger = true;
		trigger.center = Center;
		trigger.size = Size;
	}
}
