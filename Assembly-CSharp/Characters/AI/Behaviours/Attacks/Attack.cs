using System;
using UnityEditor;

namespace Characters.AI.Behaviours.Attacks
{
	public abstract class Attack : Behaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public new class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[4]
			{
				typeof(ActionAttack),
				typeof(CircularProjectileAttack),
				typeof(HorizontalProjectileAttack),
				typeof(MultiCircularProjectileAttack)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		protected bool gaveDamage;
	}
}
