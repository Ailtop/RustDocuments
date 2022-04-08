using UnityEngine.SceneManagement;

namespace Rust;

public static class Server
{
	public const float UseDistance = 3f;

	private static Scene _entityScene;

	public static Scene EntityScene
	{
		get
		{
			if (!_entityScene.IsValid())
			{
				_entityScene = SceneManager.CreateScene("Server Entities");
			}
			return _entityScene;
		}
	}
}
