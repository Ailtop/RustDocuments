using FX;
using UnityEngine;

namespace Characters.Operations
{
	public sealed class SpawnEffectInContainer : CharacterOperation
	{
		[SerializeField]
		private Transform _container;

		[SerializeField]
		private bool _attachToSpawnPosition;

		[SerializeField]
		private bool _scaleBySpawnPositionScale;

		[SerializeField]
		private EffectInfo _info;

		public override void Run(Character owner)
		{
			foreach (Transform item in _container)
			{
				ReusableChronoSpriteEffect reusableChronoSpriteEffect = _info.Spawn(item.position, owner, item.rotation.eulerAngles.z);
				if (_attachToSpawnPosition)
				{
					reusableChronoSpriteEffect.transform.parent = item;
				}
				if (_scaleBySpawnPositionScale)
				{
					Vector3 lossyScale = item.lossyScale;
					lossyScale.x = Mathf.Abs(lossyScale.x);
					lossyScale.y = Mathf.Abs(lossyScale.y);
					reusableChronoSpriteEffect.transform.localScale = Vector3.Scale(reusableChronoSpriteEffect.transform.localScale, lossyScale);
				}
			}
		}

		public override void Stop()
		{
			_info.DespawnChildren();
		}
	}
}
