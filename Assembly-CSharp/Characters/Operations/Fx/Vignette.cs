using FX;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class Vignette : CharacterOperation
	{
		[SerializeField]
		private Color _startColor;

		[SerializeField]
		private Color _endColor;

		[SerializeField]
		private Curve _curve;

		public override void Run(Character owner)
		{
			Singleton<VignetteSpawner>.Instance.Spawn(_startColor, _endColor, _curve);
		}
	}
}
