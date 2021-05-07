using System;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.LookAtTargets
{
	public abstract class Target : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Target.types)
			{
			}
		}

		public static readonly Type[] types = new Type[9]
		{
			typeof(AITarget),
			typeof(BTTarget),
			typeof(ClosestSideOnPlatform),
			typeof(Chance),
			typeof(Inverter),
			typeof(Player),
			typeof(PlatformPoint),
			typeof(TargetObject),
			typeof(TurnAround)
		};

		public abstract Character.LookingDirection GetDirectionFrom(Character character);
	}
}
