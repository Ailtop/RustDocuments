using Characters;
using UnityEngine;

namespace FX.EffectProperties
{
	public class SimpleScale : EffectProperty
	{
		[SerializeField]
		private Vector3 _scale;

		[SerializeField]
		private float _angle;

		public override void Apply(PoolObject spawned, Character owner, Target target)
		{
			spawned.transform.localScale = _scale;
			spawned.transform.localEulerAngles = new Vector3(0f, 0f, _angle);
		}
	}
}
