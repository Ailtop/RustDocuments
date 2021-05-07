using System;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusByMoving : Ability
	{
		public class Instance : AbilityInstance<StatBonusByMoving>
		{
			private const float _updateInterval = 0.2f;

			private float _remainUpdateTime;

			private Stat.Values _stat;

			private float _movedDistance;

			private float _movedDistanceBefore;

			public override Sprite icon => ability.defaultIcon;

			public override float iconFillAmount => _movedDistance / ability._distanceToMax;

			public Instance(Character owner, StatBonusByMoving ability)
				: base(owner, ability)
			{
				_stat = ability._maxStat.Clone();
			}

			protected override void OnAttach()
			{
				owner.stat.AttachValues(_stat);
				owner.movement.onMoved += OnMoved;
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
				owner.movement.onMoved -= OnMoved;
			}

			private void OnMoved(Vector2 amount)
			{
				_movedDistance += Mathf.Abs(amount.x);
				if (_movedDistance > ability._distanceToMax)
				{
					_movedDistance = ability._distanceToMax;
				}
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_movedDistance -= ability._lossPerSecond * deltaTime;
				if (_movedDistance < 0f)
				{
					_movedDistance = 0f;
				}
				_remainUpdateTime -= deltaTime;
				if (_remainUpdateTime < 0f)
				{
					_remainUpdateTime = 0.2f;
					UpdateStat();
				}
			}

			public void UpdateStat()
			{
				if (_movedDistance != _movedDistanceBefore)
				{
					Stat.Value[] values = _stat.values;
					float num = _movedDistance / ability._distanceToMax;
					if (ability._gauge != null)
					{
						ability._gauge.Set(num * 100f);
					}
					for (int i = 0; i < values.Length; i++)
					{
						values[i].value = ability._maxStat.values[i].GetMultipliedValue(num);
					}
					owner.stat.SetNeedUpdate();
					_movedDistanceBefore = _movedDistance;
				}
			}
		}

		[SerializeField]
		[Tooltip("손실이 없다고 가정할 때, 이 거리만큼 이동하면 스탯이 최대치에 도달함 (반드시 0보다 커야함)")]
		private float _distanceToMax = 1f;

		[SerializeField]
		[Tooltip("매 초 이 수치만큼 이동 거리를 잃음")]
		private float _lossPerSecond;

		[Space]
		[SerializeField]
		private ValueGauge _gauge;

		[Space]
		[SerializeField]
		private Stat.Values _maxStat;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
