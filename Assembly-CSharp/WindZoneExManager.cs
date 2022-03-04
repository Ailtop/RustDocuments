using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(WindZone))]
public class WindZoneExManager : MonoBehaviour
{
	private enum TestMode
	{
		Disabled = 0,
		Low = 1
	}

	public float maxAccumMain = 4f;

	public float maxAccumTurbulence = 4f;

	public float globalMainScale = 1f;

	public float globalTurbulenceScale = 1f;

	public Transform testPosition;
}
