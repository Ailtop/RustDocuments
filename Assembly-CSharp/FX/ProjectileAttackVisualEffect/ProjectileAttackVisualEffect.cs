using System;
using Characters;
using Characters.Projectiles;
using UnityEditor;
using UnityEngine;

namespace FX.ProjectileAttackVisualEffect
{
	public abstract class ProjectileAttackVisualEffect : VisualEffect
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, ProjectileAttackVisualEffect.types)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<ProjectileAttackVisualEffect>
		{
			public void SpawnDespawn(Projectile projectile)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].SpawnDespawn(projectile);
				}
			}

			public void SpawnExpire(Projectile projectile)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].SpawnExpire(projectile);
				}
			}

			public void Spawn(Projectile projectile, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].Spawn(projectile, origin, direction, distance, raycastHit);
				}
			}

			public void Spawn(Projectile projectile, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Damage damage, ITarget target)
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].Spawn(projectile, origin, direction, distance, raycastHit, damage, target);
				}
			}
		}

		public static readonly Type[] types = new Type[1] { typeof(SpawnOnHitPoint) };

		public abstract void SpawnDespawn(Projectile projectile);

		public abstract void SpawnExpire(Projectile projectile);

		public abstract void Spawn(Projectile projectile, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit);

		public abstract void Spawn(Projectile projectile, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Damage damage, ITarget target);
	}
}
