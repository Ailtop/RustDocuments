using Scenes;
using SkulStories;
using UnityEngine;

namespace Runnables.Triggers
{
	public class ActivatedSkulStory : Trigger
	{
		[SerializeField]
		private bool _activated;

		private Narration _narration;

		private void Start()
		{
			_narration = Scene<GameBase>.instance.uiManager.narration;
		}

		protected override bool Check()
		{
			return _narration.sceneVisible == _activated;
		}
	}
}
