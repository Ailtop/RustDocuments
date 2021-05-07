using Level;
using UnityEngine;

namespace Runnables
{
	public sealed class ChangeCameraZone : Runnable
	{
		[SerializeField]
		private CameraZone _cameraZone;

		public override void Run()
		{
			Map.Instance.cameraZone = _cameraZone;
			Map.Instance.SetCameraZoneOrDefault();
		}
	}
}
