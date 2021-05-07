using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class Vibration : CharacterOperation
	{
		[SerializeField]
		private Curve _curve;

		public override void Run(Character owner)
		{
			Singleton<Service>.Instance.controllerVibation.vibration.Attach(this, _curve);
		}
	}
}
