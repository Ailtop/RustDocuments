using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;
using UnityEngine;

namespace FX.BoundsAttackVisualEffect
{
	public class RandomWithinIntersect : BoundsAttackVisualEffect
	{
		[SerializeField]
		private EffectInfo _normal;

		[SerializeField]
		private EffectInfo _critical;

		private void Awake()
		{
			if (_critical.effect == null)
			{
				_critical = _normal;
			}
		}

		public override void Spawn(Character owner, Bounds bounds, [In][IsReadOnly] ref Damage damage, ITarget target)
		{
			EffectInfo obj = (damage.critical ? _critical : _normal);
			Bounds bounds2 = target.collider.bounds;
			Vector3 position = ((!bounds.Intersects(bounds2)) ? ((Vector3)MMMaths.RandomPointWithinBounds(bounds2)) : MMMaths.RandomVector3(MMMaths.Max(bounds.min, bounds2.min), MMMaths.Min(bounds.max, bounds2.max)));
			obj.Spawn(position, owner);
		}
	}
}
