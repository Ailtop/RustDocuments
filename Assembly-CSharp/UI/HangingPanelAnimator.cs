using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class HangingPanelAnimator : MonoBehaviour
	{
		[SerializeField]
		private Image _backgroundImage;

		[SerializeField]
		private GameObject _container;

		[SerializeField]
		private bool _startOnEnable;

		private Vector2 _appearedPosition;

		private Vector2 _disappearedPosition;

		private void Awake()
		{
			_appearedPosition = _container.transform.localPosition;
			_disappearedPosition = _appearedPosition;
			_disappearedPosition.y += _backgroundImage.rectTransform.sizeDelta.y * _backgroundImage.rectTransform.localScale.y;
		}

		private void OnEnable()
		{
			if (_startOnEnable)
			{
				Appear();
			}
		}

		private IEnumerator CEasePosition(EasingFunction.Method method, Transform transform, Vector2 from, Vector2 to, float speed = 1f)
		{
			float t = 0f;
			EasingFunction.Function easingFunction = EasingFunction.GetEasingFunction(method);
			_container.transform.localPosition = from;
			Vector2 vector = default(Vector2);
			for (; t < 1f; t += Time.unscaledDeltaTime * speed)
			{
				vector.x = easingFunction(from.x, to.x, t);
				vector.y = easingFunction(from.y, to.y, t);
				_container.transform.localPosition = vector;
				yield return null;
			}
			_container.transform.localPosition = to;
		}

		public void Appear()
		{
			base.gameObject.SetActive(true);
			StartCoroutine(CAppear());
		}

		public void Disappear()
		{
			StartCoroutine(CDisappear());
		}

		private IEnumerator CAppear()
		{
			_container.transform.localPosition = _disappearedPosition;
			while (LetterBox.instance.visible)
			{
				yield return null;
			}
			yield return CEasePosition(EasingFunction.Method.EaseOutBounce, _container.transform, _disappearedPosition, _appearedPosition, 0.5f);
		}

		private IEnumerator CDisappear()
		{
			_container.transform.localPosition = _appearedPosition;
			while (LetterBox.instance.visible)
			{
				yield return null;
			}
			yield return CEasePosition(EasingFunction.Method.EaseOutQuad, _container.transform, _appearedPosition, _disappearedPosition);
			base.gameObject.SetActive(false);
		}
	}
}
