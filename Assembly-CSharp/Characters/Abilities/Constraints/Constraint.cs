using System;
using UnityEditor;
using UnityEngine;

namespace Characters.Abilities.Constraints
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
			public bool Pass()
			{
				return base.components.Pass();
			}
		}

		public static readonly Type[] types = new Type[4]
		{
			typeof(LetterBox),
			typeof(Dialogue),
			typeof(Story),
			typeof(EndingCredit)
		};

		public abstract bool Pass();

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
