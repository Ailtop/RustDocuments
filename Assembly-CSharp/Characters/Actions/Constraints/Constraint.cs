using System;
using Characters.Actions.Constraints.Customs;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions.Constraints
{
	public abstract class Constraint : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Constraint.types)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<Constraint>
		{
		}

		public static readonly Type[] types = new Type[15]
		{
			typeof(ActionConstraint),
			typeof(IdleConstraint),
			typeof(AirAndGroundConstraint),
			typeof(DirectionConstraint),
			typeof(GoldConstraint),
			typeof(ReferenceConstraint),
			typeof(TimingConstraint),
			typeof(MotionConstraint),
			typeof(NeedAirJumpCountConstraint),
			typeof(CooldownConstraint),
			typeof(LimitedTimesOnAirConstraint),
			typeof(EnemyWithinRangeConstraint),
			typeof(GaugeConstraint),
			typeof(HealthConstraint),
			typeof(FighterRageReadyConstraint)
		};

		protected Action _action;

		public virtual void Initilaize(Action action)
		{
			_action = action;
		}

		public abstract bool Pass();

		public virtual void Consume()
		{
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
