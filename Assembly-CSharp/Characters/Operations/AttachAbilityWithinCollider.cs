using System.Collections;
using System.Collections.Generic;
using Characters.Abilities;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations
{
	public class AttachAbilityWithinCollider : CharacterOperation
	{
		private static readonly NonAllocOverlapper _sharedOverlapper = new NonAllocOverlapper(99);

		[SerializeField]
		private float _duration;

		[SerializeField]
		[Tooltip("이 주기(초)마다 콜라이더 내에 있는 캐릭터들을 검사합니다. 낮을수록 정밀도가 올라가지만 연산량이 많아집니다.")]
		[Range(0.1f, 1f)]
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

		private DoubleBuffered<List<Character>> _charactersWithinCollider;

		private CoroutineReference _cCheckReference;

		private void Awake()
		{
			if (_optimizedCollider)
			{
				_collider.enabled = false;
			}
			_charactersWithinCollider = new DoubleBuffered<List<Character>>(new List<Character>(_sharedOverlapper.capacity), new List<Character>(_sharedOverlapper.capacity));
			_abilityComponents.Initialize();
			if (_duration <= 0f)
			{
				_duration = float.PositiveInfinity;
			}
		}

		public override void Run(Character owner)
		{
			StartCoroutine(CRun(owner));
		}

		public override void Stop()
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
			_charactersWithinCollider.Current.Clear();
		}

		private IEnumerator CRun(Character owner)
		{
			_cCheckReference.Stop();
			_cCheckReference = this.StartCoroutineWithReference(CCheck(owner));
			yield return new WaitForSeconds(_duration);
			_cCheckReference.Stop();
		}

		private IEnumerator CCheck(Character owner)
		{
			while (true)
			{
				_collider.enabled = true;
				_sharedOverlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
				_sharedOverlapper.OverlapCollider(_collider);
				if (_optimizedCollider)
				{
					_collider.enabled = false;
				}
				for (int i = 0; i < _sharedOverlapper.results.Count; i++)
				{
					Target component = _sharedOverlapper.results[i].GetComponent<Target>();
					if (component == null || component.character == null || !_characterTypeFilter[component.character.type])
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
				yield return new WaitForSecondsRealtime(_checkInterval);
			}
		}
	}
}
