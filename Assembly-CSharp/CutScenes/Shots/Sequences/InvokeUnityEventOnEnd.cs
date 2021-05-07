using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CutScenes.Shots.Sequences
{
	public sealed class InvokeUnityEventOnEnd : Sequence
	{
		[SerializeField]
		[Subcomponent]
		private Subcomponents _sequences;

		[SerializeField]
		private UnityEvent _event;

		public override IEnumerator CRun()
		{
			yield return _sequences.CRun();
			_event?.Invoke();
		}
	}
}
