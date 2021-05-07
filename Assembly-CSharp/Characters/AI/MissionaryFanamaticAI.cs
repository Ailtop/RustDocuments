using System.Collections;
using Characters.AI.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class MissionaryFanamaticAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(FanaticAssemble))]
		private FanaticAssemble _fanaticAssemble;

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
				if (base.target == null)
				{
					yield return null;
				}
				else if (base.stuned)
				{
					yield return null;
				}
				else
				{
					yield return _fanaticAssemble.CRun(this);
				}
			}
		}
	}
}
