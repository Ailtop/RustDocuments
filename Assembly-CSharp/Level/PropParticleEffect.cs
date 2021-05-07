using UnityEngine;

namespace Level
{
	public class PropParticleEffect : MonoBehaviour
	{
		[GetComponent]
		[SerializeField]
		private Prop _prop;

		[SerializeField]
		private PoolObject _effect;

		[SerializeField]
		private bool _relativeScaleToTargetSize = true;

		[SerializeField]
		private ParticleEffectInfo _particleInfo;

		public void Spawn(Vector2 spawnPoint, Vector2 force)
		{
			float num = 1f;
			if (_relativeScaleToTargetSize)
			{
				Vector3 size = _prop.collider.bounds.size;
				num = Mathf.Min(size.x, size.y);
			}
			if (_effect != null)
			{
				_effect.Spawn(spawnPoint).transform.localScale = Vector3.one * num;
			}
			_particleInfo?.Emit(_prop.transform.position, _prop.collider.bounds, force);
		}
	}
}
