using System.Collections;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Abilities
{
	public class EnemyWithinRangeAttacher : AbilityAttacher
	{
		private enum Type
		{
			GreaterThanOrEqual,
			LessThan
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private int _numberOfEnemy;

		[SerializeField]
		private Collider2D _range;

		[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
		[SerializeField]
		private bool _optimizeRange = true;

		[SerializeField]
		private float _checkInterval = 0.25f;

		private NonAllocOverlapper _overlapper;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		private bool _attached;

		private void Awake()
		{
			_overlapper = new NonAllocOverlapper(_numberOfEnemy);
			_overlapper.contactFilter.SetLayerMask(1024);
			if (_optimizeRange)
			{
				_range.enabled = false;
			}
		}

		public override void OnIntialize()
		{
			_abilityComponent.Initialize();
		}

		public override void StartAttach()
		{
			StartCoroutine("CCheck");
		}

		public override void StopAttach()
		{
			StopCoroutine("CCheck");
			if (!(base.owner == null))
			{
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		private IEnumerator CCheck()
		{
			while (true)
			{
				using (new UsingCollider(_range, _optimizeRange))
				{
					_overlapper.OverlapCollider(_range);
				}
				if ((_type == Type.GreaterThanOrEqual && _overlapper.results.Count >= _numberOfEnemy) || (_type == Type.LessThan && _overlapper.results.Count < _numberOfEnemy))
				{
					Attach();
				}
				else
				{
					Detach();
				}
				yield return Chronometer.global.WaitForSeconds(_checkInterval);
			}
		}

		private void Attach()
		{
			if (!_attached)
			{
				_attached = true;
				base.owner.ability.Add(_abilityComponent.ability);
			}
		}

		private void Detach()
		{
			if (_attached)
			{
				_attached = false;
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
