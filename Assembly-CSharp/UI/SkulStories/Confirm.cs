using Scenes;
using UnityEngine;

namespace UI.SkulStories
{
	public class Confirm : MonoBehaviour
	{
		private const string _labelName = "label/skulstory/skipConfirm";

		public void Open()
		{
			UIManager uiManager = Scene<GameBase>.instance.uiManager;
			uiManager.confirm.Open(Lingua.GetLocalizedString("label/skulstory/skipConfirm"), delegate
			{
				uiManager.narration.sceneVisible = false;
			});
		}
	}
}
