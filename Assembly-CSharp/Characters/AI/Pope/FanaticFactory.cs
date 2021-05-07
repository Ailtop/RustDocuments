using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.AI.Pope.Summon;
using Level;
using Level.Chapter4;
using UnityEngine;

namespace Characters.AI.Pope
{
	public class FanaticFactory : MonoBehaviour
	{
		public enum SummonType
		{
			Fanatic,
			AgedFanatic,
			MartyrFanatic
		}

		[Serializable]
		internal class SummonInfo
		{
			[SerializeField]
			private SummonType _tag;

			[SerializeField]
			private Character _prefab;

			internal SummonType tag => _tag;

			internal Character prefab => _prefab;
		}

		[Serializable]
		public class Config
		{
			[SerializeField]
			private float _interval;

			[SerializeField]
			private Transform _spawnedContainer;

			[SerializeField]
			private SummonInfo[] _fanaticPrefabs;

			private SummonType[] toSummonTypesCached;

			internal float interval => _interval;

			internal SummonInfo[] fanaticPrefabs => _fanaticPrefabs;

			internal SummonType[] toSummonTypes
			{
				get
				{
					if (toSummonTypesCached == null)
					{
						toSummonTypesCached = fanaticPrefabs.Select((SummonInfo x) => x.tag).ToArray();
					}
					return toSummonTypesCached;
				}
			}

			internal Transform spawnedContainer => _spawnedContainer;
		}

		[SerializeField]
		private Scenario _scenario;

		[SerializeField]
		private Config _config;

		[SerializeField]
		[CountPolicy.Subcomponent(true)]
		private CountPolicy _countPolicy;

		[SerializeField]
		[FanaticPolicy.Subcomponent(true)]
		private FanaticPolicy _fanaticPolicy;

		[SerializeField]
		[LadderPolicy.Subcomponent(true)]
		private LadderPolicy _ladderPolicy;

		private Dictionary<SummonType, Character> _fanatics;

		private List<FanaticLadder> ladders;

		private bool _running;

		private void Awake()
		{
			_fanatics = new Dictionary<SummonType, Character>(_config.toSummonTypes.Length);
			SummonInfo[] fanaticPrefabs = _config.fanaticPrefabs;
			foreach (SummonInfo summonInfo in fanaticPrefabs)
			{
				_fanatics.Add(summonInfo.tag, summonInfo.prefab);
			}
		}

		public void StartToSummon()
		{
			_running = true;
			StartCoroutine(CProcess());
			_scenario.OnPhase1End += DropFanatics;
		}

		public void StopToSummon()
		{
			_running = false;
			_scenario.OnPhase1End -= DropFanatics;
		}

		public void DropFanatics()
		{
			foreach (FanaticLadder ladder in ladders)
			{
				StartCoroutine(ladder.CFall());
			}
		}

		public IEnumerator CProcess()
		{
			if (_config.fanaticPrefabs.Length == 0)
			{
				Debug.LogError("Fanatics count is 0");
				yield break;
			}
			while (_running)
			{
				ladders = _ladderPolicy.GetLadders(_fanaticPolicy.GetToSummons(_countPolicy.GetCount()));
				foreach (FanaticLadder ladder in ladders)
				{
					StartCoroutine(CSummon(ladder));
				}
				yield return Chronometer.global.WaitForSeconds(_config.interval);
			}
		}

		private IEnumerator CSummon(FanaticLadder ladder)
		{
			yield return ladder.CClimb();
			if (_running)
			{
				Character spawned = UnityEngine.Object.Instantiate(_fanatics[ladder.fanatic], ladder.spawnPoint, Quaternion.identity, _config.spawnedContainer);
				spawned.health.onDied += delegate
				{
					UnityEngine.Object.Destroy(spawned);
				};
				Map.Instance.waveContainer.Attach(spawned);
			}
		}
	}
}
