using System;
using UnityEditor;
using UnityEngine;

namespace BT.Conditions
{
	public abstract class Condition : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[4]
			{
				typeof(Chance),
				typeof(CoolDown),
				typeof(PlayerInRange),
				typeof(TargetOnOwnerPlatform)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		[SerializeField]
		private bool _inverter;

		protected abstract bool Check(Context context);

		public bool IsSatisfied(Context context)
		{
			return _inverter ^ Check(context);
		}
	}
}
