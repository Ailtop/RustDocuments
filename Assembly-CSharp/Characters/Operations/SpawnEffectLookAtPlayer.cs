using System.Collections.Generic;
using Characters.AI;
using FX;
using UnityEngine;

namespace Characters.Operations
{
	public class SpawnEffectLookAtPlayer : CharacterOperation
	{
		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private bool _attachToSpawnPosition;

		[SerializeField]
		private AIController _aIController;

		[SerializeField]
		private EffectInfo _info;

		private readonly List<ReusableChronoSpriteEffect> _effects = new List<ReusableChronoSpriteEffect>();

		private void Awake()
		{
			if (_spawnPosition == null)
			{
				_spawnPosition = base.transform;
			}
		}

		public override void Run(Character owner)
		{
			float x = _spawnPosition.position.x;
			float x2 = owner.transform.position.x;
			float x3 = _aIController.target.transform.position.x;
			_info.flipXByOwnerDirection = ((Mathf.Abs(x2 - x) < Mathf.Abs(x2 - x3)) ? true : false);
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = _info.Spawn(_spawnPosition.position, owner, _spawnPosition.rotation.eulerAngles.z);
			if (_attachToSpawnPosition)
			{
				reusableChronoSpriteEffect.transform.parent = _spawnPosition;
			}
		}

		public override void Stop()
		{
			_info.DespawnChildren();
		}
	}
}
