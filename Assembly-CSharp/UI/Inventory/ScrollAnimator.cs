using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
	public class ScrollAnimator : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _scroll;

		[SerializeField]
		private RectTransform _mask;

		[SerializeField]
		private float _scrollWidth;

		[SerializeField]
		private float _foldedScrollWidth;

		[SerializeField]
		private float _maskWidth;

		[SerializeField]
		private float _foldedMaskWidth;

		private void OnEnable()
		{
			Appear();
			StartCoroutine(CBlockInput());
		}

		private void OnDisable()
		{
			EventSystem.current.currentInputModule.enabled = true;
		}

		private IEnumerator CBlockInput()
		{
			EventSystem.current.currentInputModule.enabled = false;
			yield return new WaitForSecondsRealtime(0.3f);
			EventSystem.current.currentInputModule.enabled = true;
		}

		public void Appear()
		{
			StartCoroutine(CAppear());
		}

		private IEnumerator CAppear()
		{
			float t = 0f;
			Vector2 scrollSize = _scroll.sizeDelta;
			Vector2 maskSize = _mask.sizeDelta;
			scrollSize.x = _foldedScrollWidth;
			maskSize.x = _foldedMaskWidth;
			_scroll.sizeDelta = scrollSize;
			_mask.sizeDelta = maskSize;
			yield return new WaitForSecondsRealtime(0.1f);
			for (; t < 1f; t += Time.unscaledDeltaTime * 2f)
			{
				scrollSize.x = EasingFunction.EaseOutCubic(_foldedScrollWidth, _scrollWidth, t);
				maskSize.x = EasingFunction.EaseOutCubic(_foldedMaskWidth, _maskWidth, t);
				_scroll.sizeDelta = scrollSize;
				_mask.sizeDelta = maskSize;
				yield return null;
			}
			scrollSize.x = _scrollWidth;
			maskSize.x = _maskWidth;
			_scroll.sizeDelta = scrollSize;
			_mask.sizeDelta = maskSize;
		}
	}
}
