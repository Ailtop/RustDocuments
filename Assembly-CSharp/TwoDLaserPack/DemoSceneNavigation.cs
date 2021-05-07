using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TwoDLaserPack
{
	public class DemoSceneNavigation : MonoBehaviour
	{
		public Button buttonNextDemo;

		private void Start()
		{
			buttonNextDemo.onClick.AddListener(OnButtonNextDemoClick);
		}

		private void OnButtonNextDemoClick()
		{
			int buildIndex = SceneManager.GetActiveScene().buildIndex;
			if (buildIndex < SceneManager.sceneCount - 1)
			{
				SceneManager.LoadScene(buildIndex + 1);
			}
			else
			{
				SceneManager.LoadScene(0);
			}
		}

		private void Update()
		{
		}
	}
}
