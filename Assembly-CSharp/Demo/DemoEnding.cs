using Scenes;
using UnityEngine;

namespace Demo
{
	public class DemoEnding : MonoBehaviour
	{
		public void Activate()
		{
			Scene<GameBase>.instance.uiManager.ending.gameObject.SetActive(true);
		}

		public void Deactivate()
		{
			Scene<GameBase>.instance.uiManager.ending.gameObject.SetActive(false);
		}
	}
}
