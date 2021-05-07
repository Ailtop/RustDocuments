using System;
using UnityEditor;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public abstract class CharacterHitOperation : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, CharacterHitOperation.types)
			{
			}
		}

		[Serializable]
		internal class Subcomponents : SubcomponentArray<CharacterHitOperation>
		{
		}

		public static readonly Type[] types = new Type[13]
		{
			typeof(AddMarkStack),
			typeof(AttachCurseOfLight),
			typeof(Attack),
			typeof(Knockback),
			typeof(KnockbackTo),
			typeof(ShaderEffect),
			typeof(ApplyStatus),
			typeof(Smash),
			typeof(Heal),
			typeof(InstantAttack),
			typeof(PlaySound),
			typeof(Despawn),
			typeof(MoveOwnerToProjectile)
		};

		public abstract void Run(Projectile projectile, RaycastHit2D raycastHit, Character character);
	}
}
