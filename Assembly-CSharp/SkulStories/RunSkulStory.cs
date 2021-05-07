using Scenes;
using UnityEngine;

namespace SkulStories
{
	public class RunSkulStory : MonoBehaviour
	{
		[SerializeField]
		private bool _visible;

		private Narration _narration;

		public bool visible
		{
			get
			{
				return _visible;
			}
			set
			{
				_visible = value;
				_narration.sceneVisible = _visible;
			}
		}

		private void Start()
		{
			_narration = Scene<GameBase>.instance.uiManager.narration;
		}

		public void OnDisable()
		{
			_narration.sceneVisible = false;
		}
	}
}
