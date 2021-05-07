using System.Collections;
using Scenes;
using Services;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class CameraZoom : CharacterOperation
	{
		[SerializeField]
		private float _percent = 1f;

		[SerializeField]
		private float _zoomSpeed = 1f;

		[SerializeField]
		private float _restoreSpeed = 1f;

		[SerializeField]
		private float _duration;

		private float originalTrackSpeed;

		public override void Run(Character owner)
		{
			StartCoroutine(CZoom(owner.chronometer.animation));
		}

		public override void Stop()
		{
			if (!Service.quitting)
			{
				Scene<GameBase>.instance.cameraController.Zoom(1f, _restoreSpeed);
			}
		}

		private IEnumerator CZoom(Chronometer chronometer)
		{
			CameraController cameraController = Scene<GameBase>.instance.cameraController;
			cameraController.Zoom(_percent, _zoomSpeed);
			yield return chronometer.WaitForSeconds(_duration);
			cameraController.Zoom(1f, _restoreSpeed);
		}
	}
}
