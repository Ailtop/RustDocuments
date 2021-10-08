using UnityEngine;

public interface IMissionProvider
{
	uint ProviderID();

	Vector3 ProviderPosition();

	BaseEntity Entity();
}
