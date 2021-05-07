using System.Collections;
using Characters.AI.Behaviours;
using Runnables;
using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class YggdrasillElderEntAI : AIController
	{
		[SerializeField]
		[Characters.AI.Behaviours.Behaviour.Subcomponent(true)]
		private Characters.AI.Behaviours.Behaviour _behaviours;

		[SerializeField]
		[Characters.AI.Behaviours.Behaviour.Subcomponent(true)]
		private Characters.AI.Behaviours.Behaviour _phase2Sequence;

		[SerializeField]
		private Runnable _onPhase2;

		private void Awake()
		{
			StartCoroutine(CProcess());
			character.onDie += OnDie;
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return _behaviours.CRun(this);
		}

		private void OnDie()
		{
			character.onDie -= OnDie;
			character.health.Heal(0.0099999997764825821);
			_onPhase2.Run();
			StopAllCoroutinesWithBehaviour();
			StartCoroutine(_phase2Sequence.CRun(this));
		}
	}
}
