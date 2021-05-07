using System;
using System.Collections.Generic;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class AttachAbilityWithinCollider : Ability
	{
		public enum ChronometerType
		{
			Master,
			Animation,
			Effect,
			Projectile
		}

		public class Instance : AbilityInstance<AttachAbilityWithinCollider>
		{
			private static readonly NonAllocOverlapper _sharedOverlapper = new NonAllocOverlapper(99);

			private DoubleBuffered<List<Character>> _charactersWithinCollider;

			private float _remainCheckTime;

			public Instance(Character owner, AttachAbilityWithinCollider ability)
				: base(owner, ability)
			{
				if (ability._optimizedCollider)
				{
					ability._collider.enabled = false;
				}
				_charactersWithinCollider = new DoubleBuffered<List<Character>>(new List<Character>(_sharedOverlapper.capacity), new List<Character>(_sharedOverlapper.capacity));
				ability._abilityComponents.Initialize();
			}

			protected override void OnAttach()
			{
			}

			protected override void OnDetach()
			{
				for (int i = 0; i < _charactersWithinCollider.Current.Count; i++)
				{
					Character character = _charactersWithinCollider.Current[i];
					AbilityComponent[] components = ability._abilityComponents.components;
					if (!(character == null))
					{
						for (int j = 0; j < components.Length; j++)
						{
							character.ability.Remove(components[j].ability);
						}
					}
				}
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCheckTime -= deltaTime;
				if (_remainCheckTime < 0f)
				{
					_remainCheckTime = ability._checkInterval;
					Check();
				}
			}

			private void Check()
			{
				ability._collider.enabled = true;
				_sharedOverlapper.contactFilter.SetLayerMask(ability._layer.Evaluate(owner.gameObject));
				_sharedOverlapper.OverlapCollider(ability._collider);
				if (ability._optimizedCollider)
				{
					ability._collider.enabled = false;
				}
				for (int i = 0; i < _sharedOverlapper.results.Count; i++)
				{
					Target component = _sharedOverlapper.results[i].GetComponent<Target>();
					if (component == null || component.character == null || !ability._characterTypeFilter[component.character.type])
					{
						continue;
					}
					_charactersWithinCollider.Next.Add(component.character);
					int num = _charactersWithinCollider.Current.IndexOf(component.character);
					if (num >= 0)
					{
						_charactersWithinCollider.Current.RemoveAt(num);
						continue;
					}
					for (int j = 0; j < ability._abilityComponents.components.Length; j++)
					{
						component.character.ability.Add(ability._abilityComponents.components[j].ability);
					}
				}
				for (int k = 0; k < _charactersWithinCollider.Current.Count; k++)
				{
					Character character = _charactersWithinCollider.Current[k];
					if (!(character == null))
					{
						for (int l = 0; l < ability._abilityComponents.components.Length; l++)
						{
							character.ability.Remove(ability._abilityComponents.components[l].ability);
						}
					}
				}
				_charactersWithinCollider.Current.Clear();
				_charactersWithinCollider.Swap();
			}
		}

		[SerializeField]
		[Range(0.1f, 1f)]
		[Tooltip("이 주기(초)마다 콜라이더 내에 있는 캐릭터들을 검사합니다. 낮을수록 정밀도가 올라가지만 연산량이 많아집니다.")]
		private float _checkInterval = 0.33f;

		[Header("Filter")]
		[SerializeField]
		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		[SerializeField]
		private CharacterTypeBoolArray _characterTypeFilter = new CharacterTypeBoolArray(true, true, true, true, true, true, true, true);

		[Header("Collider")]
		[SerializeField]
		private Collider2D _collider;

		[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
		[SerializeField]
		private bool _optimizedCollider = true;

		[Header("Abilities")]
		[Space]
		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent.Subcomponents _abilityComponents;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
