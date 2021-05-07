using System;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public abstract class Condition : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[10]
			{
				typeof(BehaviourCoolTime),
				typeof(BehaviourResult),
				typeof(BetweenTargetAndWall),
				typeof(CanStartAction),
				typeof(CompareDistanceFromWall),
				typeof(CoolDown),
				typeof(EnterTrigger),
				typeof(HealthCondition),
				typeof(MonsterCount),
				typeof(TargetIsGrounded)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		[SerializeField]
		private bool _inverter;

		protected abstract bool Check(AIController controller);

		public bool IsSatisfied(AIController controller)
		{
			return _inverter ^ Check(controller);
		}
	}
}
