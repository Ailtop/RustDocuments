using UnityEngine;

namespace CutScenes.Shots
{
	public class OnStartEnd : MonoBehaviour
	{
		[SerializeField]
		[Shot.Subcomponent]
		private Shot _onStart;

		[SerializeField]
		[Shot.Subcomponent]
		private Shot _onEnd;

		public Shot onStart => _onStart;

		public Shot onEnd => _onEnd;
	}
}
