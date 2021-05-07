using System.Collections;
using UnityEngine;

namespace CutScenes.Shots
{
	public sealed class SequenceInfos : Shot
	{
		[SerializeField]
		[Sequence.Subcomponent]
		private Sequence.Subcomponents _sequences;

		private Shot _next;

		public override void Run()
		{
			StartCoroutine(CRun());
		}

		private IEnumerator CRun()
		{
			yield return _sequences.CRun();
			if (_next != null)
			{
				_next.Run();
			}
		}

		public override void SetNext(Shot next)
		{
			_next = next;
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
