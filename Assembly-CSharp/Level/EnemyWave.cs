using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using FX;
using PhysicsUtils;
using UnityEngine;

namespace Level
{
	public class EnemyWave : Wave
	{
		[Serializable]
		private class GameObjectRandomizer
		{
			[Serializable]
			public class Reorderable : ReorderableArray<GameObjectRandomizer>
			{
				public void Randomize()
				{
					if (values.Length == 0)
					{
						return;
					}
					int max = values.Sum((GameObjectRandomizer v) => v._weight);
					int num = UnityEngine.Random.Range(0, max) + 1;
					int num2 = 0;
					for (int i = 0; i < values.Length; i++)
					{
						num -= values[i]._weight;
						if (num <= 0)
						{
							num2 = i;
							break;
						}
					}
					for (int j = 0; j < values.Length; j++)
					{
						if (j != num2)
						{
							GameObject gameObject = values[j]._gameObject;
							gameObject.transform.parent = null;
							UnityEngine.Object.Destroy(gameObject);
						}
					}
				}
			}

			[SerializeField]
			private GameObject _gameObject;

			[SerializeField]
			private int _weight = 1;
		}

		private class Assets
		{
			internal static EffectInfo enemyAppearance = new EffectInfo(Resource.instance.enemyAppearanceEffect);
		}

		private enum SpawnCondition
		{
			Always,
			TimeOutAfterSpawn,
			RemainMonsters,
			EnterZone,
			TimeOut,
			Manually
		}

		private static readonly NonAllocOverlapper _overlapper;

		private const float _enemySpawnDelay = 0.4f;

		[SerializeField]
		[Range(0f, 100f)]
		private int _possibility = 100;

		[SerializeField]
		private GameObjectRandomizer.Reorderable _randomizer = new GameObjectRandomizer.Reorderable();

		[SerializeField]
		private SpawnCondition _condition;

		[SerializeField]
		private EnemyWave _targetWave;

		[SerializeField]
		private int _remainMosnters;

		[SerializeField]
		private Collider2D _zone;

		[SerializeField]
		private float _timeOut;

		private int _remains;

		public List<Character> characters { get; private set; }

		public List<DestructibleObject> destructibleObjects { get; private set; }

		public event Action<int> onChildrenChanged;

		static EnemyWave()
		{
			_overlapper = new NonAllocOverlapper(1);
			_overlapper.contactFilter.SetLayerMask(512);
		}

		public void Spawn(bool effect = true)
		{
			try
			{
				if (base.state != 0 || !MMMaths.PercentChance(_possibility))
				{
					return;
				}
				base.state = State.Spawned;
				_onSpawn?.Invoke();
				if (effect)
				{
					StartCoroutine(_003CSpawn_003Eg__CRun_007C25_0());
					return;
				}
				foreach (Character character in characters)
				{
					character.gameObject.SetActive(true);
				}
				foreach (DestructibleObject destructibleObject in destructibleObjects)
				{
					destructibleObject.gameObject.SetActive(true);
				}
			}
			catch (Exception ex)
			{
				Debug.Log("Error while spawn enemy wave : " + ex.Message);
				Clear();
			}
		}

		private void Clear()
		{
			if (base.state != State.Cleared)
			{
				base.state = State.Cleared;
				_onClear?.Invoke();
			}
		}

		private void DecreaseRemains()
		{
			_remains--;
			this.onChildrenChanged?.Invoke(_remains);
			if (_remains == 0)
			{
				Clear();
			}
		}

		public override void Initialize()
		{
			try
			{
				_randomizer.Randomize();
				destructibleObjects = new List<DestructibleObject>();
				characters = new List<Character>();
				_003CInitialize_003Eg__AddCharacterOrDestructibleObject_007C28_0(base.transform);
				_remains = characters.Count + destructibleObjects.Count;
				if (!base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(true);
				}
				switch (_condition)
				{
				case SpawnCondition.Always:
					Spawn(false);
					break;
				case SpawnCondition.TimeOutAfterSpawn:
					if (_targetWave.state == State.Waiting)
					{
						_targetWave.onSpawn += TimeOutAfterSpawn;
					}
					else
					{
						StartCoroutine(CTimeOut());
					}
					_remainMosnters = 0;
					_targetWave.onChildrenChanged += CheckRemainMosnters;
					break;
				case SpawnCondition.RemainMonsters:
					_targetWave.onChildrenChanged += CheckRemainMosnters;
					break;
				case SpawnCondition.EnterZone:
					StartCoroutine(CCheckZone());
					break;
				case SpawnCondition.TimeOut:
					StartCoroutine(CTimeOut());
					break;
				case SpawnCondition.Manually:
					break;
				}
			}
			catch (Exception ex)
			{
				Debug.Log("Error while initialize enemy wave : " + ex.Message);
				Clear();
			}
		}

		private void TimeOutAfterSpawn()
		{
			StartCoroutine(CTimeOutAfterSpawn());
		}

		private IEnumerator CTimeOutAfterSpawn()
		{
			yield return Chronometer.global.WaitForSeconds(_timeOut);
			_targetWave.onSpawn -= TimeOutAfterSpawn;
			if (base.state == State.Waiting)
			{
				Spawn();
			}
		}

		private void CheckRemainMosnters(int remains)
		{
			if (remains <= _remainMosnters)
			{
				_targetWave.onChildrenChanged -= CheckRemainMosnters;
				if (base.state == State.Waiting)
				{
					Spawn();
				}
			}
		}

		private IEnumerator CTimeOut()
		{
			yield return Chronometer.global.WaitForSeconds(_timeOut);
			if (base.state == State.Waiting)
			{
				Spawn();
			}
		}

		private IEnumerator CCheckZone()
		{
			yield return null;
			while (_overlapper.OverlapCollider(_zone).GetComponent<Target>() == null)
			{
				yield return null;
			}
			Spawn();
		}
	}
}
