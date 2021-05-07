using System;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public abstract class Policy : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[8]
			{
				typeof(ToBTTarget),
				typeof(ToObject),
				typeof(ToPlatformPoint),
				typeof(ToPlayer),
				typeof(ToPlayerBased),
				typeof(ToRandomPoint),
				typeof(ToTargetOpposition),
				typeof(ToSavedPosition)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		public abstract Vector2 GetPosition();
	}
}
