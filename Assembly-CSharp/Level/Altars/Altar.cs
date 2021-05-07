using System;
using System.Collections.Generic;
using Characters;
using FX;
using FX.SpriteEffects;
using UnityEngine;

namespace Level.Altars
{
	public class Altar : MonoBehaviour
	{
		[Serializable]
		private class ShaderEffect
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

			private GenericSpriteEffect _effect;

			public void Initialize()
			{
				_effect = new GenericSpriteEffect(_priority, 2.14748365E+09f, 1f, _colorOverlay, _colorBlend, _outline);
			}

			public void Attach(Character target)
			{
				if (target.spriteEffectStack != null)
				{
					target.spriteEffectStack.Add(_effect);
				}
			}

			public void Detach(Character target)
			{
				if (_effect != null && target.spriteEffectStack != null)
				{
					target.spriteEffectStack.Remove(_effect);
				}
			}
		}

		[SerializeField]
		private Stat.Values _stat;

		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private Target _target;

		[SerializeField]
		private EffectInfo _effect;

		[SerializeField]
		private ShaderEffect _shaderEffect;

		[SerializeField]
		private int _offset = 1;

		private Prop _prop;

		public Collider2D collider => _collider;

		public List<Character> characters { get; private set; } = new List<Character>();


		public event Action onDestroyed;

		private void Awake()
		{
			_prop = GetComponentInParent<Prop>();
			_prop.onDestroy += Destroy;
			_shaderEffect.Initialize();
		}

		private void Destroy()
		{
			_collider.enabled = false;
			this.onDestroyed?.Invoke();
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			_003C_003Ec__DisplayClass20_0 _003C_003Ec__DisplayClass20_ = new _003C_003Ec__DisplayClass20_0();
			Target component = collision.GetComponent<Target>();
			if (!(component == null) && !(component.character == null))
			{
				Character character = component.character;
				if (character.type != Character.Type.Trap && character.type != Character.Type.Dummy)
				{
					_003C_003Ec__DisplayClass20_.spawnedEffect = _effect.Spawn(component.transform.position, component.character);
					_003C_003Ec__DisplayClass20_.spawnedEffect.GetComponent<SpriteRenderer>();
					_shaderEffect.Attach(component.character);
					component.character.stat.AttachValues(_stat, _003C_003Ec__DisplayClass20_._003COnTriggerEnter2D_003Eg__DespawnEffect_007C0);
					characters.Add(component.character);
				}
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			Target component = collision.GetComponent<Target>();
			if (!(component == null) && !(component.character == null) && !(component.character.health?.dead ?? false))
			{
				component.character.stat.DetachValues(_stat);
				_shaderEffect.Detach(component.character);
				characters.Remove(component.character);
			}
		}
	}
}
