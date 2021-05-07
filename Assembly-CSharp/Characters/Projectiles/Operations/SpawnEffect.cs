using FX;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class SpawnEffect : Operation
	{
		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private bool _attachToSpawnPosition;

		[SerializeField]
		private bool _scaleBySpawnPositionScale;

		[SerializeField]
		private EffectInfo _info;

		private void Awake()
		{
			if (_spawnPosition == null)
			{
				_spawnPosition = base.transform;
			}
		}

		public override void Run(Projectile projectile)
		{
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = _info.Spawn(_spawnPosition.position, _spawnPosition.rotation.eulerAngles.z);
			if (_attachToSpawnPosition)
			{
				reusableChronoSpriteEffect.transform.parent = _spawnPosition;
			}
			if (_scaleBySpawnPositionScale)
			{
				Vector3 lossyScale = _spawnPosition.lossyScale;
				lossyScale.x = Mathf.Abs(lossyScale.x);
				if (Mathf.Abs(_spawnPosition.rotation.eulerAngles.y) == 180f)
				{
					lossyScale.x *= -1f;
				}
				lossyScale.y = Mathf.Abs(lossyScale.y);
				reusableChronoSpriteEffect.transform.localScale = Vector3.Scale(reusableChronoSpriteEffect.transform.localScale, lossyScale);
			}
		}
	}
}
