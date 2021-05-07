using System.Collections;
using System.Collections.Generic;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Abilities
{
	public class ColliderAbilityAttacher : AbilityAttacher
	{
		private const float _checkInterval = 0.33f;

		private static readonly NonAllocOverlapper _sharedOverlapper = new NonAllocOverlapper(99);

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
		[SerializeField]
		private bool _optimizedCollider = true;

		[Header("Abilities")]
		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent.Subcomponents _abilityComponents;

		private DoubleBuffered<List<Character>> _charactersWithinCollider;

		private CoroutineReference _cCheckReference;

		private void Awake()
		{
			if (_optimizedCollider)
			{
				_collider.enabled = false;
			}
			_charactersWithinCollider = new DoubleBuffered<List<Character>>(new List<Character>(_sharedOverlapper.capacity), new List<Character>(_sharedOverlapper.capacity));
		}

		public override void OnIntialize()
		{
			_abilityComponents.Initialize();
		}

		public override void StartAttach()
		{
			_cCheckReference.Stop();
			_cCheckReference = this.StartCoroutineWithReference(CCheck());
		}

		public override void StopAttach()
		{
			_cCheckReference.Stop();
			foreach (Character item in _charactersWithinCollider.Current)
			{
				AbilityComponent[] components = _abilityComponents.components;
				foreach (AbilityComponent abilityComponent in components)
				{
					item.ability.Remove(abilityComponent.ability);
				}
			}
		}

		private IEnumerator CCheck()
		{
			while (true)
			{
				_collider.enabled = true;
				_sharedOverlapper.contactFilter.SetLayerMask(_layer.Evaluate(base.owner.gameObject));
				_sharedOverlapper.OverlapCollider(_collider);
				if (_optimizedCollider)
				{
					_collider.enabled = false;
				}
				for (int i = 0; i < _sharedOverlapper.results.Count; i++)
				{
					Target component = _sharedOverlapper.results[i].GetComponent<Target>();
					if (component == null || component.character == null)
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
					for (int j = 0; j < _abilityComponents.components.Length; j++)
					{
						component.character.ability.Add(_abilityComponents.components[j].ability);
					}
				}
				for (int k = 0; k < _charactersWithinCollider.Current.Count; k++)
				{
					Character character = _charactersWithinCollider.Current[k];
					for (int l = 0; l < _abilityComponents.components.Length; l++)
					{
						character.ability.Remove(_abilityComponents.components[l].ability);
					}
				}
				_charactersWithinCollider.Current.Clear();
				_charactersWithinCollider.Swap();
				yield return new WaitForSecondsRealtime(0.33f);
			}
		}
	}
}
