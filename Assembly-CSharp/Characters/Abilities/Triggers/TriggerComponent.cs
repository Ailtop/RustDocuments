using System;
using Characters.Abilities.Constraints;
using UnityEditor;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	public abstract class TriggerComponent : MonoBehaviour, ITrigger
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[20]
			{
				typeof(OnActionComponent),
				typeof(OnChargeActionComponent),
				typeof(OnBackAttackComponent),
				typeof(OnBeginCombatComponent),
				typeof(OnEnterMapComponent),
				typeof(OnEvadeComponent),
				typeof(OnFinishCombatComponent),
				typeof(OnGaugeFullComponent),
				typeof(OnGaugeEmptyComponent),
				typeof(OnValueGaugeValueReachedComponent),
				typeof(OnGaveDamageComponent),
				typeof(OnGroundedComponent),
				typeof(OnHealthChangedComponent),
				typeof(OnKilledComponent),
				typeof(OnMarkMaxStackComponent),
				typeof(OnMoveComponent),
				typeof(OnSwapComponent),
				typeof(OnTookDamageComponent),
				typeof(OnUpdateComponent),
				typeof(OnJumpComponent)
			};

			public SubcomponentAttribute()
				: base(true, types)
			{
			}
		}

		public abstract float cooldownTime { get; }

		public abstract float remainCooldownTime { get; }

		public abstract event Action onTriggered;

		public abstract void Attach(Character character);

		public abstract void Detach();

		public abstract void UpdateTime(float deltaTime);

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
	public abstract class TriggerComponent<T> : TriggerComponent where T : Trigger
	{
		[SerializeField]
		private T _trigger;

		private Constraint[] _constraints = new Constraint[4]
		{
			new LetterBox(),
			new Dialogue(),
			new Story(),
			new Characters.Abilities.Constraints.EndingCredit()
		};

		public override float cooldownTime => _trigger.cooldownTime;

		public override float remainCooldownTime => _trigger.remainCooldownTime;

		public override event Action onTriggered
		{
			add
			{
				_trigger.onTriggered += value;
			}
			remove
			{
				_trigger.onTriggered -= value;
			}
		}

		public override void Attach(Character character)
		{
			_trigger.Attach(character);
		}

		public override void Detach()
		{
			_trigger.Detach();
		}

		public override void UpdateTime(float deltaTime)
		{
			if (_constraints.Pass())
			{
				_trigger.UpdateTime(deltaTime);
			}
		}
	}
}
