using UnityEngine;

namespace Scenes
{
	public class Scene : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		protected string _scenePath;
	}
	public abstract class Scene<T> : Scene where T : Scene<T>
	{
		public static T instance { get; private set; }

		public Scene()
		{
			instance = this as T;
		}

		protected virtual void OnDestroy()
		{
			instance = null;
		}
	}
}
