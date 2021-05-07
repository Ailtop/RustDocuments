using System;
using System.Linq;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatByCountsWithinRange : Ability
	{
		public class Instance : AbilityInstance<StatByCountsWithinRange>
		{
			private Stat.Values _stat;

			private int _count;

			private float _remainCheckTime;

			public override int iconStacks => _count;

			public override Sprite icon
			{
				get
				{
					if (_count <= 0)
					{
						return null;
					}
					return base.icon;
				}
			}

			public Instance(Character owner, StatByCountsWithinRange ability)
				: base(owner, ability)
			{
				_stat = ability._statPerCount.Clone();
			}

			protected override void OnAttach()
			{
				UpdateStat();
				owner.stat.AttachValues(_stat);
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCheckTime -= deltaTime;
				if (_remainCheckTime < 0f)
				{
					_remainCheckTime += 0.25f;
					UpdateStat();
				}
			}

			private void UpdateStat()
			{
				_count = ability.GetCountWithinRange(owner.gameObject);
				for (int i = 0; i < _stat.values.Length; i++)
				{
					double num = (double)_count * ability._statPerCount.values[i].value;
					if (ability._statPerCount.values[i].categoryIndex == Stat.Category.Percent.index)
					{
						num += 1.0;
					}
					_stat.values[i].value = num;
				}
				owner.stat.SetNeedUpdate();
			}
		}

		public const float _overlapInterval = 0.25f;

		private NonAllocOverlapper _overlapper;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private Stat.Values _statPerCount;

		[SerializeField]
		private TargetLayer _layer;

		[SerializeField]
		private CharacterTypeBoolArray _characterTypes;

		[Information("스탯 배수에 적용할 기본 배수", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private int _base;

		[Information("효과가 적용되기 시작하는 최소 개수", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private int _min;

		[SerializeField]
		private int _max;

		public override void Initialize()
		{
			base.Initialize();
			if (_overlapper == null)
			{
				_overlapper = new NonAllocOverlapper(_max);
			}
		}

		private int GetCountWithinRange(GameObject gameObject)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(gameObject));
			_range.enabled = true;
			_overlapper.OverlapCollider(_range);
			int num = _overlapper.results.Where(delegate(Collider2D result)
			{
				Character component = result.GetComponent<Character>();
				return !(component == null) && _characterTypes[component.type];
			}).Count();
			if (num > 0)
			{
				num += _base;
			}
			_range.enabled = false;
			if (num < _min)
			{
				return 0;
			}
			if (num > _max)
			{
				return _max;
			}
			return num;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
