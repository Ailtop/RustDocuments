using UnityEngine.SceneManagement;

namespace Rust;

public static class Generic
{
	private static Scene _batchingScene;

	public static Scene BatchingScene
	{
		get
		{
			if (!_batchingScene.IsValid())
			{
				_batchingScene = SceneManager.CreateScene("Batching");
			}
			return _batchingScene;
		}
	}
}
