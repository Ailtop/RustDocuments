using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FX
{
	public class Vignette : MonoBehaviour
	{
		[SerializeField]
		private PoolObject _poolObject;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private Image _image;

		public void Initialize(Color startColor, Color endColor, Curve curve)
		{
			_rectTransform.localScale = Vector3.one;
			_rectTransform.localPosition = Vector3.zero;
			_rectTransform.offsetMax = Vector2.zero;
			_rectTransform.offsetMin = Vector2.zero;
			StartCoroutine(CFade(startColor, endColor, curve));
		}

		private IEnumerator CFade(Color startColor, Color endColor, Curve curve)
		{
			float duration = curve.duration;
			for (float time = 0f; time < duration; time += Chronometer.global.deltaTime)
			{
				_image.color = Color.Lerp(startColor, endColor, curve.Evaluate(time));
				yield return null;
			}
			_poolObject.Despawn();
		}
	}
}
