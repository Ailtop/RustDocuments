using System;
using Characters.Movements;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusByAirTime : Ability
	{
		public class Instance : AbilityInstance<StatBonusByAirTime>
		{
			private const float _updateInterval = 0.25f;

			private float _remainUpdateTime;

			private Stat.Values _stat;

			private float _remainBuffTime;

			private bool _wasGrounded;

			private float _airTime;

			private float _cachedMultiplier;

			public override Sprite icon
			{
				get
				{
					if (!(_remainBuffTime > 0f))
					{
						return null;
					}
					return ability.defaultIcon;
				}
			}

			public override float iconFillAmount => 1f - _airTime / ability._timeToMaxStat;

			public Instance(Character owner, StatBonusByAirTime ability)
				: base(owner, ability)
			{
				_stat = ability._maxStat.Clone();
			}

			protected override void OnAttach()
			{
				_wasGrounded = owner.movement.controller.isGrounded;
				SetStat(0f);
				owner.stat.AttachValues(_stat);
				owner.movement.onJump += OnJump;
				owner.movement.onGrounded += OnGrounded;
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
				owner.movement.onJump -= OnJump;
				owner.movement.onGrounded -= OnGrounded;
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainUpdateTime -= deltaTime;
				if (_wasGrounded)
				{
					_remainBuffTime -= deltaTime;
				}
				else
				{
					_airTime += deltaTime;
				}
				if (_airTime > ability._timeToMaxStat)
				{
					_airTime = ability._timeToMaxStat;
				}
				if (_remainUpdateTime < 0f)
				{
					_remainUpdateTime = 0.25f;
					UpdateStat();
				}
			}

			public void UpdateStat()
			{
				Stat.Value[] value = _stat.values;
				float num = 0f;
				if (_remainBuffTime > 0f)
				{
					num = _airTime / ability._timeToMaxStat;
				}
				if (num != _cachedMultiplier)
				{
					_cachedMultiplier = num;
					SetStat(num);
				}
			}

			private void SetStat(float multiplier)
			{
				Stat.Value[] values = _stat.values;
				for (int i = 0; i < values.Length; i++)
				{
					values[i].value = ability._maxStat.values[i].GetMultipliedValue(multiplier);
				}
				owner.stat.SetNeedUpdate();
			}

			private void OnJump(Movement.JumpType jumpType, float jumpHeight)
			{
				if (_wasGrounded)
				{
					_wasGrounded = false;
					_airTime = 0f;
				}
				_remainBuffTime = float.PositiveInfinity;
				UpdateStat();
			}

			private void OnGrounded()
			{
				_wasGrounded = true;
				_remainBuffTime = ability._remainTimeOnGround;
			}
		}

		[SerializeField]
		private float _timeToMaxStat;

		[Tooltip("바닥에 착지할 경우 이 시간 후에 버프가 사라짐")]
		[SerializeField]
		private float _remainTimeOnGround = 1f;

		[Space]
		[SerializeField]
		private Stat.Values _maxStat;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
