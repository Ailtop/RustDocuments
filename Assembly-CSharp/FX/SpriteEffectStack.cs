using FX.SpriteEffects;
using UnityEngine;

namespace FX
{
	public class SpriteEffectStack : MonoBehaviour, ISpriteEffectStack
	{
		private readonly PriorityList<SpriteEffect> _effects = new PriorityList<SpriteEffect>();

		protected Chronometer _chronometer;

		[SerializeField]
		protected SpriteRenderer _spriteRenderer;

		private MaterialPropertyBlock props;

		public SpriteRenderer mainRenderer
		{
			get
			{
				return _spriteRenderer;
			}
			set
			{
				_spriteRenderer = value;
			}
		}

		Renderer ISpriteEffectStack.mainRenderer => _spriteRenderer;

		protected virtual void Awake()
		{
			if (_spriteRenderer == null)
			{
				_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			props = new MaterialPropertyBlock();
		}

		protected virtual void LateUpdate()
		{
			for (int num = _effects.Count - 1; num >= 0; num--)
			{
				if (!_effects[num].Update(_chronometer.DeltaTime()))
				{
					_effects.RemoveAt(num);
				}
			}
			SpriteEffect.@default.Apply(_spriteRenderer);
			for (int i = 0; i < _effects.Count; i++)
			{
				_effects[i].Apply(_spriteRenderer);
			}
		}

		public void Add(SpriteEffect effect)
		{
			_effects.Add(effect.priority, effect);
		}

		public bool Contains(SpriteEffect effect)
		{
			return _effects.Contains(effect);
		}

		public bool Remove(SpriteEffect effect)
		{
			return _effects.Remove(effect);
		}
	}
}
