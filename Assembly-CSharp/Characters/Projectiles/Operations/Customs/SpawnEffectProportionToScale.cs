using FX;
using UnityEngine;

namespace Characters.Projectiles.Operations.Customs
{
	public class SpawnEffectProportionToScale : Operation
	{
		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private bool _attachToSpawnPosition;

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
			reusableChronoSpriteEffect.transform.localScale = base.transform.lossyScale;
		}
	}
}
