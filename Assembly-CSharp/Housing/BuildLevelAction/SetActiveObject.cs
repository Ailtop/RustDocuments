using UnityEngine;

namespace Housing.BuildLevelAction
{
	[RequireComponent(typeof(BuildLevel))]
	public class SetActiveObject : BuildLevelAction
	{
		[SerializeField]
		private GameObject _target;

		[SerializeField]
		private bool _active = true;

		protected override void Run()
		{
			_target.SetActive(_active);
		}
	}
}
