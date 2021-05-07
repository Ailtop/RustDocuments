using Characters;
using Level;
using UnityEngine;

public class CameraZoneChanger : MonoBehaviour
{
	[SerializeField]
	private CameraZone _cameraZone;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Character component = collision.GetComponent<Character>();
		if (!(component == null) && component.type == Character.Type.Player)
		{
			Map.Instance.cameraZone = _cameraZone;
			Map.Instance.SetCameraZoneOrDefault();
		}
	}
}
