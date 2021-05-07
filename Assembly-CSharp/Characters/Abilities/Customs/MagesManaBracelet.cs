using System;
using Characters.Actions;
using FX;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class MagesManaBracelet : Ability
	{
		public class Instance : AbilityInstance<MagesManaBracelet>
		{
			private ReusableChronoSpriteEffect _buffEffect;

			private float _remainBuffTime;

			private int _currentCount;

			private bool _buffAttached;

			public override int iconStacks => _currentCount;

			public override float iconFillAmount
			{
				get
				{
					if (!_buffAttached)
					{
						return 0f;
					}
					return 1f - _remainBuffTime / ability._buffDuration;
				}
			}

			public Instance(Character owner, MagesManaBracelet ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				_buffAttached = true;
				owner.onStartAction += OnOwnerStartAction;
				owner.stat.AttachValues(ability._stat);
			}

			protected override void OnDetach()
			{
				_buffAttached = false;
				owner.onStartAction -= OnOwnerStartAction;
				owner.stat.DetachValues(ability._stat);
				DetachBuff();
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainBuffTime -= deltaTime;
				if (_remainBuffTime < 0f)
				{
					DetachBuff();
				}
			}

			private void AttachBuff()
			{
				_remainBuffTime = ability._buffDuration;
				_currentCount = 0;
				if (!_buffAttached)
				{
					_buffAttached = true;
					owner.stat.AttachValues(ability._stat);
					_buffEffect = ((ability._buffEffect == null) ? null : ability._buffEffect.Spawn(owner.transform.position, owner));
				}
			}

			private void DetachBuff()
			{
				_buffAttached = false;
				owner.stat.DetachValues(ability._stat);
				if (_buffEffect != null)
				{
					_buffEffect.reusable.Despawn();
					_buffEffect = null;
				}
			}

			private void OnOwnerStartAction(Characters.Actions.Action action)
			{
				if (action.type == Characters.Actions.Action.Type.Skill && !action.cooldown.usedByStreak)
				{
					_currentCount++;
					if (_currentCount == ability._count)
					{
						AttachBuff();
					}
				}
			}
		}

		[SerializeField]
		private Stat.Values _stat;

		[SerializeField]
		private EffectInfo _buffEffect = new EffectInfo
		{
			subordinated = true
		};

		[SerializeField]
		private float _buffDuration;

		[SerializeField]
		private int _count;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
