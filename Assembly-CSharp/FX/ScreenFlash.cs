using System;
using System.Collections;
using Scenes;
using UnityEngine;

namespace FX
{
	public class ScreenFlash : MonoBehaviour
	{
		[Serializable]
		public class Info
		{
			public enum SortingOrder
			{
				Frontmost,
				Rearmost
			}

			[SerializeField]
			private Color _color = Color.black;

			[SerializeField]
			[Tooltip("페이드인이 완료된 후 지속될 시간")]
			private float _duration;

			[Space]
			[SerializeField]
			[SortingLayer]
			private int _sortingLayer;

			[SerializeField]
			private SortingOrder _sortingOrder;

			[Space]
			[SerializeField]
			private AnimationCurve _fadeIn;

			[SerializeField]
			private float _fadeInDuration;

			[Space]
			[SerializeField]
			private AnimationCurve _fadeOut;

			[SerializeField]
			private float _fadeOutDuration;

			public Color color => _color;

			public float duration => _duration;

			public int sortingLayer => _sortingLayer;

			public SortingOrder sortingOrder => _sortingOrder;

			public AnimationCurve fadeIn => _fadeIn;

			public float fadeInDuration => _fadeInDuration;

			public AnimationCurve fadeOut => _fadeOut;

			public float fadeOutDuration => _fadeOutDuration;
		}

		[SerializeField]
		private PoolObject _poolObject;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		private float _pixelPerUnit;

		private float _width;

		private float _height;

		private Info _info;

		private float _fadedPercent;

		public SpriteRenderer spriteRenderer => _spriteRenderer;

		private void Awake()
		{
			_pixelPerUnit = _spriteRenderer.sprite.pixelsPerUnit;
			_width = _spriteRenderer.sprite.rect.width;
			_height = _spriteRenderer.sprite.rect.height;
		}

		private void FullScreen()
		{
			Vector3 zero = Vector3.zero;
			zero.z = 10f;
			base.transform.localPosition = zero;
			Camera camera = Scene<GameBase>.instance.camera;
			float num = camera.orthographicSize * 2f;
			Vector2 vector = new Vector2(camera.aspect * num, num);
			Vector2 one = Vector2.one;
			if (vector.x >= vector.y)
			{
				one *= vector.x * _pixelPerUnit / _width;
			}
			else
			{
				one *= vector.y * _pixelPerUnit / _height;
			}
			base.transform.localScale = one;
		}

		public void Play(Info info)
		{
			FullScreen();
			_info = info;
			_fadedPercent = 0f;
			_spriteRenderer.sortingLayerID = info.sortingLayer;
			if (info.sortingOrder == Info.SortingOrder.Frontmost)
			{
				_spriteRenderer.sortingOrder = 32767;
			}
			else
			{
				_spriteRenderer.sortingOrder = -32768;
			}
			_spriteRenderer.color = info.color;
			StartCoroutine(CPlay());
		}

		private IEnumerator CPlay()
		{
			yield return CFadeIn(_info.fadeIn, _info.fadeInDuration);
			yield return Chronometer.global.WaitForSeconds(_info.duration);
			yield return CFadeOut(_info.fadeOut, _info.fadeOutDuration);
		}

		public void FadeOut()
		{
			StopAllCoroutines();
			StartCoroutine(CFadeOut(_info.fadeOut, _info.fadeOutDuration));
		}

		private IEnumerator CFadeIn(AnimationCurve curve, float duration)
		{
			Color color = _spriteRenderer.color;
			float alpha = _info.color.a;
			if (duration > 0f)
			{
				while (_fadedPercent < 1f)
				{
					color.a = alpha * Mathf.LerpUnclamped(0f, 1f, curve.Evaluate(_fadedPercent));
					_spriteRenderer.color = color;
					yield return null;
					_fadedPercent += Chronometer.global.deltaTime / duration;
				}
			}
			_fadedPercent = 1f;
			color.a = alpha;
			_spriteRenderer.color = color;
		}

		private IEnumerator CFadeOut(AnimationCurve curve, float duration)
		{
			Color color = _spriteRenderer.color;
			float alpha = _info.color.a;
			if (duration > 0f)
			{
				while (_fadedPercent > 0f)
				{
					color.a = alpha * Mathf.LerpUnclamped(0f, 1f, curve.Evaluate(_fadedPercent));
					_spriteRenderer.color = color;
					yield return null;
					_fadedPercent -= Chronometer.global.deltaTime / duration;
				}
			}
			_fadedPercent = 0f;
			_poolObject.Despawn();
		}
	}
}
