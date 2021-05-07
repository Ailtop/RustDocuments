using System.Collections;
using Characters.Actions;
using FX;
using Level;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class FanaticAssemble : Behaviour
	{
		[SerializeField]
		private EffectInfo _spawnEffect;

		[SerializeField]
		private float _spawnDelay = 0.5f;

		[SerializeField]
		private EnemyWave _waveToSpawn;

		[SerializeField]
		private Action _action;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Sacrifice))]
		private Sacrifice _sacrifice;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (_waveToSpawn == null)
			{
				Debug.LogError("_waveToSpawn of " + controller.character.name + " is not assigned!");
				yield break;
			}
			if (!_action.TryStart())
			{
				base.result = Result.Fail;
				yield break;
			}
			while (_action.running)
			{
				yield return null;
			}
			base.result = Result.Success;
			SpawnSpawnEffect();
			float delay = 0f;
			while (delay < _spawnDelay)
			{
				delay += controller.character.chronometer.master.deltaTime;
				yield return null;
			}
			_waveToSpawn.Spawn(false);
			yield return _sacrifice.CRun(controller);
		}

		private void SpawnSpawnEffect()
		{
			foreach (Character character in _waveToSpawn.characters)
			{
				_spawnEffect.Spawn(character.transform.position);
			}
		}
	}
}
