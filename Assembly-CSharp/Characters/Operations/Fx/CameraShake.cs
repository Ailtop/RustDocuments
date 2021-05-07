using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class CameraShake : CharacterOperation
	{
		[SerializeField]
		private float _amount;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private bool _vibrateController = true;

		public override void Run(Character owner)
		{
			Scene<GameBase>.instance.cameraController.shake.Attach(this, _amount, _duration);
			if (_vibrateController)
			{
				Singleton<Service>.Instance.controllerVibation.vibration.Attach(this, _amount, _duration);
			}
		}
	}
}
