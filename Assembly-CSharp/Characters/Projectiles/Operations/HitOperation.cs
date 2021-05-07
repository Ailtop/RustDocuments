using System;
using UnityEditor;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public abstract class HitOperation : CharacterHitOperation
	{
		public new class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, HitOperation.types)
			{
			}
		}

		[Serializable]
		internal new class Subcomponents : SubcomponentArray<HitOperation>
		{
		}

		public new static readonly Type[] types = new Type[15]
		{
			typeof(Despawn),
			typeof(PlaySound),
			typeof(DropSkulHead),
			typeof(Stuck),
			typeof(SummonOperationRunner),
			typeof(InstantAttack),
			typeof(MoveOwnerToProjectile),
			typeof(SummonOperationRunner),
			typeof(SummonOperationRunnerOnHitPoint),
			typeof(SpreadOperationRunner),
			typeof(ActivateObject),
			typeof(Bounce),
			typeof(SpawnObject),
			typeof(CameraShake),
			typeof(DropParts)
		};

		public abstract void Run(Projectile projectile, RaycastHit2D raycastHit);

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character character)
		{
			Run(projectile, raycastHit);
		}
	}
}
