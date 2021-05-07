using System;
using Characters.Projectiles;
using UnityEngine;

namespace Characters.Movements
{
	[Serializable]
	public class PushInfo
	{
		[SerializeField]
		internal bool ignoreOtherForce;

		[SerializeField]
		internal bool expireOnGround;

		[SerializeField]
		internal PushForce force1;

		[SerializeField]
		internal Curve curve1;

		[SerializeField]
		internal PushForce force2;

		[SerializeField]
		internal Curve curve2;

		internal ValueTuple<Vector2, Vector2> Evaluate(Transform from, ITarget to)
		{
			return new ValueTuple<Vector2, Vector2>(force1.Evaluate(from, to), force2.Evaluate(from, to));
		}

		internal ValueTuple<Vector2, Vector2> Evaluate(Projectile from, ITarget to)
		{
			return new ValueTuple<Vector2, Vector2>(force1.Evaluate(from, to), force2.Evaluate(from, to));
		}

		internal ValueTuple<Vector2, Vector2> Evaluate(Character from, ITarget to)
		{
			return new ValueTuple<Vector2, Vector2>(force1.Evaluate(from, to), force2.Evaluate(from, to));
		}

		internal ValueTuple<Vector2, Vector2> EvaluateTimeIndependent(Character from, ITarget to)
		{
			Vector2 item = force1.Evaluate(from, to);
			if (curve1.duration > 0f)
			{
				item /= curve1.duration;
			}
			Vector2 item2 = force2.Evaluate(from, to);
			if (curve2.duration > 0f)
			{
				item2 /= curve2.duration;
			}
			return new ValueTuple<Vector2, Vector2>(item, item2);
		}

		public PushInfo()
		{
			ignoreOtherForce = false;
			expireOnGround = false;
		}

		public PushInfo(bool ignoreOtherForce = false, bool expireOnGround = false)
		{
			this.ignoreOtherForce = ignoreOtherForce;
			this.expireOnGround = expireOnGround;
		}

		public PushInfo(PushForce pushForce, Curve curve, bool ignoreOtherForce = false, bool expireOnGround = false)
		{
			this.ignoreOtherForce = ignoreOtherForce;
			this.expireOnGround = expireOnGround;
			force1 = pushForce;
			curve1 = curve;
			force2 = new PushForce();
			curve2 = Curve.empty;
		}

		public PushInfo(PushForce force1, Curve curve1, PushForce force2, Curve curve2, bool ignoreOtherForce = false, bool expireOnGround = false)
		{
			this.ignoreOtherForce = ignoreOtherForce;
			this.expireOnGround = expireOnGround;
			this.force1 = force1;
			this.curve1 = curve1;
			this.force2 = force2;
			this.curve2 = curve2;
		}
	}
}
