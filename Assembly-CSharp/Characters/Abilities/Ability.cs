using System;
using FX;
using FX.SpriteEffects;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public abstract class Ability : IAbility
	{
		[Serializable]
		public class SpriteEffect
		{
			[SerializeField]
			private int _priority;

			[SerializeField]
			private GenericSpriteEffect.ColorOverlay _colorOverlay;

			[SerializeField]
			private GenericSpriteEffect.ColorBlend _colorBlend;

			[SerializeField]
			private GenericSpriteEffect.Outline _outline;

			public bool enabled
			{
				get
				{
					if (!_colorOverlay.enabled && !_colorBlend.enabled)
					{
						return _outline.enabled;
					}
					return true;
				}
			}

			public GenericSpriteEffect CreateInstance()
			{
				return new GenericSpriteEffect(_priority, float.MaxValue, 1f, _colorOverlay, _colorBlend, _outline);
			}
		}

		[SerializeField]
		protected float _duration;

		[SerializeField]
		protected Sprite _defaultIcon;

		[SerializeField]
		protected int _iconPriority;

		[SerializeField]
		[Tooltip("아이콘 채우기 색깔 반전")]
		protected bool _iconFillInversed;

		[SerializeField]
		[Tooltip("아이콘 채우기 방향 반전")]
		protected bool _iconFillFlipped;

		[SerializeField]
		protected bool _removeOnSwapWeapon;

		[Header("Effects")]
		[SerializeField]
		private EffectInfo _loopEffect = new EffectInfo
		{
			subordinated = true
		};

		[SerializeField]
		private SpriteEffect _spriteEffect;

		[SerializeField]
		private EffectInfo _effectOnAttach;

		[SerializeField]
		private EffectInfo _effectOnDetach;

		public float duration
		{
			get
			{
				return _duration;
			}
			set
			{
				_duration = value;
			}
		}

		public Sprite defaultIcon
		{
			get
			{
				return _defaultIcon;
			}
			set
			{
				_defaultIcon = value;
			}
		}

		public int iconPriority => _iconPriority;

		public bool iconFillInversed => _iconFillInversed;

		public bool iconFillFlipped => _iconFillFlipped;

		public bool removeOnSwapWeapon => _removeOnSwapWeapon;

		public EffectInfo loopEffect => _loopEffect;

		public SpriteEffect spriteEffect => _spriteEffect;

		public EffectInfo effectOnAttach => _effectOnAttach;

		public EffectInfo effectOnDetach => _effectOnDetach;

		public virtual void Initialize()
		{
			if (_duration == 0f)
			{
				_duration = float.PositiveInfinity;
			}
		}

		public abstract IAbilityInstance CreateInstance(Character owner);

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
