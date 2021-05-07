using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class RunAction : Sequence
	{
		[SerializeField]
		private Action _action;

		public override IEnumerator CRun()
		{
			_action.TryStart();
			while (_action.running)
			{
				yield return null;
			}
		}
	}
}
