using System;
using System.Collections;
using Characters.AI.Behaviours;
using UnityEngine;

namespace Characters.AI.Pope
{
	public class Sequence : MonoBehaviour, ISequence
	{
		public interface IPhase
		{
			IEnumerator CRun(AIController controller);
		}

		[Serializable]
		private class Phase1 : IPhase
		{
			[SerializeField]
			[Characters.AI.Behaviours.Behaviour.Subcomponent(true)]
			private Characters.AI.Behaviours.Behaviour _behaviours;

			public IEnumerator CRun(AIController controller)
			{
				yield return _behaviours.CRun(controller);
			}
		}

		[Serializable]
		private class Phase2 : IPhase
		{
			[SerializeField]
			private Characters.AI.Behaviours.Behaviour _sequence;

			public IEnumerator CRun(AIController controller)
			{
				yield return _sequence.CRun(controller);
			}
		}

		[Header("Phase1")]
		[SerializeField]
		private Phase1 _phase1;

		[Space]
		[Header("Phase2")]
		[SerializeField]
		private Phase2 _phase2;

		private IPhase[] _phases;

		private int _currentPhase;

		private IEnumerator reference;

		private void Awake()
		{
			_phases = new IPhase[2] { _phase1, _phase2 };
		}

		public IEnumerator CRun(AIController controller)
		{
			reference = _phases[_currentPhase].CRun(controller);
			yield return reference;
		}

		public void Stop()
		{
			StopCoroutine(reference);
		}

		public void NextPhase()
		{
			if (_currentPhase + 1 >= _phases.Length)
			{
				Debug.LogError("Index of out of range in Pope's phase");
			}
			else
			{
				_currentPhase++;
			}
		}
	}
}
