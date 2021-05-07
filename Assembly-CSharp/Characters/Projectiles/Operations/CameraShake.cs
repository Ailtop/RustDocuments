using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class CameraShake : Operation
	{
		[SerializeField]
		private float _amount;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private bool _vibrateController = true;

		public override void Run(Projectile projectile)
		{
			Scene<GameBase>.instance.cameraController.shake.Attach(this, _amount, _duration);
			if (_vibrateController)
			{
				Singleton<Service>.Instance.controllerVibation.vibration.Attach(this, _amount, _duration);
			}
		}
	}
}
