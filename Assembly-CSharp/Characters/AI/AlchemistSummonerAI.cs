using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class AlchemistSummonerAI : AIController
	{
		[SerializeField]
		private AlchemistSummonerAI _anotherSummonerAI;

		[SerializeField]
		private Collider2D _sightRange;

		[SerializeField]
		private Collider2D _anotherSummonerSightRange;

		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[Header("PrepareSummon")]
		[SerializeField]
		private ChainAction _prepareSummon;

		[SerializeField]
		private RepeatPlaySound _takeNotesAudioSource;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight };
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			while (!base.dead)
			{
				bool num = (bool)FindClosestPlayerBody(_sightRange) || (bool)FindClosestPlayerBody(_anotherSummonerSightRange);
				bool flag = base.lastAttacker != null;
				if (num || flag)
				{
					StartSummon();
					_anotherSummonerAI.StartSummon();
					break;
				}
				yield return null;
			}
		}

		public void StartSummon()
		{
			_takeNotesAudioSource.Stop();
			if (base.gameObject.activeSelf)
			{
				_prepareSummon.TryStart();
			}
		}
	}
}
