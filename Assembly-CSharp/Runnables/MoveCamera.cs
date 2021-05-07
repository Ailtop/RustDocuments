using System.Collections;
using Scenes;
using UnityEngine;

namespace Runnables
{
	public class MoveCamera : CRunnable
	{
		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Curve _curve;

		public override IEnumerator CRun()
		{
			CameraController cameraController = Scene<GameBase>.instance.cameraController;
			cameraController.StopTrack();
			cameraController.StartTrack(_target);
			float elapsed = 0f;
			Vector3 startPosition = Camera.main.transform.position;
			while (elapsed < _curve.duration)
			{
				elapsed += Chronometer.global.deltaTime;
				Vector3 position = Vector3.Lerp(startPosition, _target.position, _curve.Evaluate(elapsed));
				position.z = Camera.main.transform.position.z;
				Camera.main.transform.position = position;
				yield return null;
			}
			Vector3 position2 = _target.position;
			position2.z = Camera.main.transform.position.z;
			Camera.main.transform.position = position2;
		}
	}
}
