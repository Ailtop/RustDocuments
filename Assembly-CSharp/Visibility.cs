using System;
using UnityEngine;

public class Visibility : MonoBehaviour
{
	private const float outOfCameraZPosition = -100f;

	public bool visible { get; private set; } = true;


	public event Action<bool> onChanged;

	public void SetVisible(bool visible)
	{
		if (this.visible != visible)
		{
			this.visible = visible;
			Vector3 position = base.transform.position;
			position.z = (visible ? 0f : (-100f));
			base.transform.position = position;
			this.onChanged?.Invoke(visible);
		}
	}
}
