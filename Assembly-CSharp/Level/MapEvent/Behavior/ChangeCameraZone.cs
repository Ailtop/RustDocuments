using System.Collections;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Level.MapEvent.Behavior
{
	public class ChangeCameraZone : Behavior
	{
		[SerializeField]
		private CameraZone _cameraZone;

		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private float _duration;

		public override void Run()
		{
			if (_duration <= 0f)
			{
				ChangeCameraZoneComplete();
			}
			else
			{
				StartCoroutine(CRun(_curve, _duration));
			}
		}

		private IEnumerator CRun(Curve curve, float duration)
		{
			ChangeCameraZoneComplete();
			Map.Instance.cameraZone = null;
			Map.Instance.SetCameraZoneOrDefault();
			Camera camera = Scene<GameBase>.instance.cameraController.camera;
			Vector3 from = camera.transform.position;
			Vector3 vector = from;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Chronometer.global.deltaTime;
				vector = _cameraZone.GetClampedPosition(camera, Singleton<Service>.Instance.levelManager.player.transform.position);
				vector.z = from.z;
				camera.transform.position = from + (vector - from) * curve.Evaluate(elapsed / duration);
				yield return null;
			}
			vector = _cameraZone.GetClampedPosition(camera, Singleton<Service>.Instance.levelManager.player.transform.position);
			vector.z = from.z;
			camera.transform.position = vector;
			ChangeCameraZoneComplete();
		}

		private void ChangeCameraZoneComplete()
		{
			Map.Instance.cameraZone = _cameraZone;
			Map.Instance.SetCameraZoneOrDefault();
		}
	}
}
