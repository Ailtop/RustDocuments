using Scenes;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class CameraShakeCurve : CharacterOperation
	{
		[SerializeField]
		private Curve _curve;

		public override void Run(Character owner)
		{
			Scene<GameBase>.instance.cameraController.shake.Attach(this, _curve);
		}
	}
}
