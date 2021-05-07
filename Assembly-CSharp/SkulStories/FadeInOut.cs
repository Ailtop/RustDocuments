using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SkulStories
{
	public class FadeInOut : Sequence
	{
		[SerializeField]
		private Color _target;

		[SerializeField]
		private float _duration;

		private Color _originColor;

		private Image _image;

		private void Start()
		{
			_image = _narration.blackScreen;
			_originColor = _image.color;
		}

		public override IEnumerator CRun()
		{
			Color startColor = _image.color;
			Color different = _target - _image.color;
			float elapsed = 0f;
			while (elapsed < _duration)
			{
				elapsed += Chronometer.global.deltaTime;
				_image.color = startColor + different * (elapsed / _duration);
				yield return null;
			}
			_image.color = _target;
		}

		public void OnDisable()
		{
			_image.color = _originColor;
		}
	}
}
