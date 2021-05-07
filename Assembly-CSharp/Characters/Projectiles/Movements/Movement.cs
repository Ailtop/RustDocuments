using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public abstract class Movement : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, Movement.types)
			{
			}
		}

		public static readonly Type[] types = new Type[8]
		{
			typeof(Simple),
			typeof(Ease2),
			typeof(Trajectory),
			typeof(TrajectoryToPoint),
			typeof(Homing),
			typeof(Missile),
			typeof(Ground),
			typeof(Spiral)
		};

		public Projectile projectile { get; private set; }

		public float direction { get; set; }

		public Vector2 directionVector { get; set; }

		public virtual void Initialize(Projectile projectile, float direction)
		{
			this.projectile = projectile;
			this.direction = direction;
			float f = direction * ((float)Math.PI / 180f);
			directionVector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public abstract ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime);
	}
}
