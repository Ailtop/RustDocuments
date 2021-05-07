using System;
using Characters;
using UnityEditor;
using UnityEngine;

namespace FX.CastAttackVisualEffect
{
	public abstract class CastAttackVisualEffect : VisualEffect
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, CastAttackVisualEffect.types)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<CastAttackVisualEffect>
		{
			public void Spawn(Vector3 position)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].Spawn(position);
				}
			}

			public void Spawn(Character owner, Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].Spawn(owner, collider, origin, direction, distance, raycastHit);
				}
			}

			public void Spawn(Character owner, Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Damage damage, ITarget target)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].Spawn(owner, collider, origin, direction, distance, raycastHit, damage, target);
				}
			}
		}

		public static readonly Type[] types = new Type[1] { typeof(SpawnOnHitPoint) };

		public abstract void Spawn(Vector3 position);

		public abstract void Spawn(Character owner, Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit);

		public abstract void Spawn(Character owner, Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Damage damage, ITarget target);
	}
}
