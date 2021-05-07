using System.Collections;
using Runnables;
using UnityEditor;
using UnityEngine;

namespace SkulStories
{
	public class SkulStory : Runnable
	{
		[SerializeField]
		[Event.Subcomponent]
		private Event.Subcomponents _onStart;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(SequenceInfo))]
		private SequenceInfo.Subcomponents _sequence;

		[SerializeField]
		[Event.Subcomponent]
		private Event.Subcomponents _onEnd;

		public override void Run()
		{
			StartCoroutine(CRun());
		}

		private IEnumerator CRun()
		{
			_onStart.Run();
			yield return _sequence.CRun();
			_onEnd.Run();
		}
	}
}
