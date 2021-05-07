using System;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Level
{
	public class EnemyWaveContainer : MonoBehaviour
	{
		public enum State
		{
			Empty,
			Remain
		}

		public State state { get; private set; }

		public Wave[] waves { get; private set; }

		public EnemyWave[] enemyWaves { get; private set; }

		public SummonWave summonWave { get; private set; }

		public event Action<State> onStateChanged;

		public void Initialize()
		{
			waves = GetComponentsInChildren<Wave>(true);
			enemyWaves = GetComponentsInChildren<EnemyWave>(true);
			GameObject gameObject = new GameObject("SummonWave");
			gameObject.transform.SetParent(base.transform);
			summonWave = gameObject.AddComponent<SummonWave>();
			Wave[] array = waves;
			foreach (Wave obj in array)
			{
				obj.Initialize();
				obj.onClear += CheckWaveState;
				obj.onSpawn += CheckWaveState;
			}
			state = GetState();
			this.onStateChanged?.Invoke(state);
		}

		public List<Character> GetAllEnemies()
		{
			List<Character> list = new List<Character>();
			EnemyWave[] array = enemyWaves;
			foreach (EnemyWave enemyWave in array)
			{
				list.AddRange(enemyWave.characters);
			}
			if (summonWave != null)
			{
				list.AddRange(summonWave.characters);
			}
			return list;
		}

		public void Stop()
		{
			Wave[] array = waves;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop();
			}
		}

		public void Attach(Character character)
		{
			character.transform.parent = summonWave.transform;
			summonWave.Attach(character);
		}

		private State GetState()
		{
			State result = State.Empty;
			Wave[] array = waves;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].state == Wave.State.Spawned)
				{
					result = State.Remain;
					break;
				}
			}
			return result;
		}

		private void CheckWaveState()
		{
			State state = GetState();
			if (this.state != state)
			{
				this.state = state;
				this.onStateChanged?.Invoke(this.state);
			}
		}
	}
}
