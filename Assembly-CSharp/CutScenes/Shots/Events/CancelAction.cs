using Runnables;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class CancelAction : Event
	{
		[SerializeField]
		private Target _target;

		public override void Run()
		{
			_target.character.CancelAction();
		}
	}
}
