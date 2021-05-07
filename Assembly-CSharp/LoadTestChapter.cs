using Level;
using Services;
using Singletons;
using UnityEngine;

public class LoadTestChapter : MonoBehaviour
{
	private void Start()
	{
		Singleton<Service>.Instance.levelManager.Load(Chapter.Type.Test);
	}
}
