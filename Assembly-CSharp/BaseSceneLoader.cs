using Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseSceneLoader : MonoBehaviour
{
	private void Awake()
	{
		if (Scene<Base>.instance == null)
		{
			SceneManager.LoadScene("Base", LoadSceneMode.Additive);
		}
	}
}
