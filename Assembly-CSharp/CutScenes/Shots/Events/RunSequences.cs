using UnityEngine;

namespace CutScenes.Shots.Events
{
	public sealed class RunSequences : Event
	{
		[SerializeField]
		[Sequence.Subcomponent]
		private Sequence.Subcomponents _sequences;

		public override void Run()
		{
			StartCoroutine(_sequences.CRun());
		}
	}
}
