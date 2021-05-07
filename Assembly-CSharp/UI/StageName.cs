using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public sealed class StageName : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _chapterName;

		[SerializeField]
		private TextMeshProUGUI _stageNumber;

		[SerializeField]
		private TextMeshProUGUI _stageName;

		[SerializeField]
		private Image _background;

		[SerializeField]
		private HangingPanelAnimator _animator;

		public void Show(string chapterName, string stageNumber, string stageName)
		{
			base.gameObject.SetActive(true);
			_chapterName.text = chapterName;
			_stageNumber.text = stageNumber;
			_stageName.text = stageName;
			StartCoroutine(CShow());
		}

		private IEnumerator CShow()
		{
			_animator.Appear();
			yield return new WaitForSecondsRealtime(4f);
			_animator.Disappear();
		}
	}
}
