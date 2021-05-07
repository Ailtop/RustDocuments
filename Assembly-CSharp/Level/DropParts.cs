using System.Collections;
using UnityEngine;

namespace Level
{
	public class DropParts : MonoBehaviour
	{
		[SerializeField]
		[FrameTime]
		private float _delay;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private ParticleEffectInfo _particleEffectInfo;

		public IEnumerator CSpawn(Vector2 position, Bounds bounds, Vector2 force)
		{
			yield return Chronometer.global.WaitForSeconds(_delay);
			_particleEffectInfo.Emit(position, bounds, force);
		}

		public IEnumerator CSpawn()
		{
			yield return Chronometer.global.WaitForSeconds(_delay);
			_particleEffectInfo.Emit(base.transform.position, _range.bounds, Vector2.zero);
		}
	}
}
