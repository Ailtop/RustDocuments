using UnityEngine;

public class TriggerRagdollRelocate : TriggerBase
{
	public Transform targetLocation;

	internal override void OnObjectAdded(GameObject obj, Collider col)
	{
		base.OnObjectAdded(obj, col);
		BaseEntity baseEntity = obj.transform.ToBaseEntity();
		if (baseEntity != null && baseEntity.isServer)
		{
			RepositionTransform(baseEntity.transform);
		}
		Ragdoll componentInParent = obj.GetComponentInParent<Ragdoll>();
		if (!(componentInParent != null))
		{
			return;
		}
		RepositionTransform(componentInParent.transform);
		foreach (Rigidbody rigidbody in componentInParent.rigidbodies)
		{
			if (rigidbody.transform.position.y < base.transform.position.y)
			{
				RepositionTransform(rigidbody.transform);
			}
		}
	}

	private void RepositionTransform(Transform t)
	{
		Vector3 position = targetLocation.InverseTransformPoint(t.position);
		position.y = 0f;
		position = (t.position = targetLocation.TransformPoint(position));
	}
}
