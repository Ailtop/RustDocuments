using System;
using System.Collections;
using Characters;
using PhysicsUtils;
using UnityEngine;

namespace Level.Traps
{
	public class TrapController : MonoBehaviour
	{
		[Serializable]
		public class Config
		{
			internal enum Condition
			{
				None,
				OnCharacterDie,
				OnTargetWaveSpawn,
				OnTargetWaveClear,
				PlayerOnTriggerEnter,
				PlayerOnTriggerExit,
				OnPropDestory
			}

			[SerializeField]
			internal Condition condition;

			[SerializeField]
			internal Character character;

			[SerializeField]
			internal EnemyWave targetWave;

			[SerializeField]
			internal Collider2D range;

			[SerializeField]
			internal Prop prop;

			[SerializeField]
			internal bool once;
		}

		[SerializeField]
		private ControlableTrap _targetTrap;

		[SerializeField]
		private Config _activate;

		[SerializeField]
		private Config _deactivate;

		private Action OnColliderEnter;

		private Action OnColliderExit;

		private static readonly NonAllocOverlapper _lapper;

		static TrapController()
		{
			_lapper = new NonAllocOverlapper(15);
			_lapper.contactFilter.SetLayerMask(512);
		}

		private void Start()
		{
			Initialize(_activate, _targetTrap.Activate);
			Initialize(_deactivate, _targetTrap.Deactivate);
		}

		private void Initialize(Config config, Action run)
		{
			switch (config.condition)
			{
			case Config.Condition.None:
				if (config == _activate)
				{
					_targetTrap.Activate();
				}
				break;
			case Config.Condition.OnCharacterDie:
				config.character.health.onDied += run;
				break;
			case Config.Condition.OnTargetWaveSpawn:
				if (config.targetWave.state == Wave.State.Spawned)
				{
					run?.Invoke();
				}
				else
				{
					config.targetWave.onSpawn += run;
				}
				break;
			case Config.Condition.OnTargetWaveClear:
				config.targetWave.onClear += run;
				break;
			case Config.Condition.PlayerOnTriggerEnter:
				StartCoroutine(CTriggerEnter(config));
				OnColliderEnter = (Action)Delegate.Combine(OnColliderEnter, run);
				break;
			case Config.Condition.PlayerOnTriggerExit:
				StartCoroutine(CTriggerExit(config));
				OnColliderExit = (Action)Delegate.Combine(OnColliderExit, run);
				break;
			case Config.Condition.OnPropDestory:
				config.prop.onDestroy += run;
				break;
			}
		}

		private IEnumerator CTriggerEnter(Config config)
		{
			while (true)
			{
				if (_lapper.OverlapCollider(config.range).GetComponent<Character>() == null)
				{
					yield return null;
					continue;
				}
				OnColliderEnter?.Invoke();
				if (config.once)
				{
					break;
				}
				while (_lapper.OverlapCollider(config.range).GetComponent<Character>() != null)
				{
					yield return null;
				}
			}
		}

		private IEnumerator CTriggerExit(Config config)
		{
			while (true)
			{
				if (_lapper.OverlapCollider(config.range).GetComponent<Character>() == null)
				{
					yield return null;
					continue;
				}
				while (_lapper.OverlapCollider(config.range).GetComponent<Character>() != null)
				{
					yield return null;
				}
				OnColliderExit?.Invoke();
				if (config.once)
				{
					break;
				}
			}
		}
	}
}
