using System;
using Characters;
using Characters.Movements;
using UnityEditor;
using UnityEngine;

namespace FX.SmashAttackVisualEffect
{
	public abstract class SmashAttackVisualEffect : VisualEffect
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, SmashAttackVisualEffect.types)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<SmashAttackVisualEffect>
		{
			public void Spawn(Character owner, Push push, RaycastHit2D raycastHit, Movement.CollisionDirection direction, Damage damage, ITarget target)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].Spawn(owner, push, raycastHit, direction, damage, target);
				}
			}
		}

		public static readonly Type[] types = new Type[1] { typeof(SpawnOnHitPoint) };

		public abstract void Spawn(Character owner, Push push, RaycastHit2D raycastHit, Movement.CollisionDirection direction, Damage damage, ITarget target);
	}
}
