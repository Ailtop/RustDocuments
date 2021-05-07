using Services;
using Singletons;
using UnityEngine;

namespace Level.Objects
{
	public class WeepingStatue : MonoBehaviour
	{
		[SerializeField]
		private GameObject _forEditor;

		[SerializeField]
		private GameObject _stage1;

		[SerializeField]
		private GameObject _stage2;

		private void OnEnable()
		{
			_forEditor.gameObject.SetActive(false);
			if (Singleton<Service>.Instance.levelManager.currentChapter.stageIndex == 0)
			{
				_stage1.gameObject.SetActive(true);
				_stage2.gameObject.SetActive(false);
			}
			else
			{
				_stage2.gameObject.SetActive(true);
				_stage1.gameObject.SetActive(false);
			}
		}
	}
}
