using FX.SpriteEffects;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public sealed class ShaderEffect : CharacterHitOperation
	{
		[SerializeField]
		private int _priority;

		[SerializeField]
		private bool _proportionalToTenacity;

		[SerializeField]
		private GenericSpriteEffect.ColorOverlay _colorOverlay;

		[SerializeField]
		private GenericSpriteEffect.ColorBlend _colorBlend;

		[SerializeField]
		private GenericSpriteEffect.Outline _outline;

		private float _duration;

		private GenericSpriteEffect _effect;

		private void Awake()
		{
			_duration = Mathf.Max(_colorOverlay.duration, _colorBlend.duration, _outline.duration);
		}

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character character)
		{
			if (character.spriteEffectStack != null)
			{
				_effect = new GenericSpriteEffect(_priority, _duration, _proportionalToTenacity ? ((float)character.stat.GetFinal(Stat.Kind.StoppingResistance)) : 1f, _colorOverlay, _colorBlend, _outline);
				character.spriteEffectStack.Add(_effect);
			}
		}
	}
}
