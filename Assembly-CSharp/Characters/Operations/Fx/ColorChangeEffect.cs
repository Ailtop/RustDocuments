using FX.SpriteEffects;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class ColorChangeEffect : CharacterOperation
	{
		private const int priority = 0;

		[SerializeField]
		private Color _startColor = Color.white;

		[SerializeField]
		private Color _endColor = new Color(1f, 1f, 1f, 0f);

		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private bool _proportionalToTenacity;

		private float _originalDuration;

		private void Awake()
		{
			_originalDuration = _curve.duration;
		}

		public override void Run(Character owner)
		{
			if (_proportionalToTenacity)
			{
				_curve.duration = _originalDuration * (float)owner.stat.GetFinal(Stat.Kind.StoppingResistance);
			}
			owner.spriteEffectStack?.Add(new EasedColorOverlay(0, _startColor, _endColor, _curve));
		}
	}
}
