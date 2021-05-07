using System;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Pope.Summon
{
	public abstract class CountPolicy : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[2]
			{
				typeof(ConstantCountPolicy),
				typeof(RadnomCountPolicy)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		public abstract int GetCount();
	}
}
