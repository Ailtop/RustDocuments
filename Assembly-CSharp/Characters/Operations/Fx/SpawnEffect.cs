using FX;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class SpawnEffect : CharacterOperation
	{
		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private bool _extraAngleBySpawnPositionRotation = true;

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

		public override void Run(Character owner)
		{
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = ((!_extraAngleBySpawnPositionRotation) ? _info.Spawn(_spawnPosition.position, owner) : _info.Spawn(_spawnPosition.position, owner, _spawnPosition.rotation.eulerAngles.z));
			if (_attachToSpawnPosition)
			{
				reusableChronoSpriteEffect.transform.parent = _spawnPosition;
			}
			if (_scaleBySpawnPositionScale)
			{
				Vector3 lossyScale = _spawnPosition.lossyScale;
				lossyScale.x = Mathf.Abs(lossyScale.x);
				lossyScale.y = Mathf.Abs(lossyScale.y);
				reusableChronoSpriteEffect.transform.localScale = Vector3.Scale(reusableChronoSpriteEffect.transform.localScale, lossyScale);
			}
		}

		public override void Stop()
		{
			_info.DespawnChildren();
		}
	}
}
