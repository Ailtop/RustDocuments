using System;
using System.Collections;
using Scenes;
using UnityEngine;

namespace Level
{
	public class ParallaxBackground : MonoBehaviour
	{
		[Serializable]
		private class Element
		{
			[Serializable]
			internal class Reorderable : ReorderableArray<Element>
			{
			}

			[SerializeField]
			private SpriteRenderer _spriteRenderer;

			[SerializeField]
			private bool _randomize = true;

			[SerializeField]
			private Vector2 _distance;

			[SerializeField]
			private float _hotizontalAutoScroll;

			private Vector2 _spriteSize;

			private SpriteRenderer[] _rendererInstances;

			private Vector2[] _origin;

			private Vector2 _translated;

			private float _mostLeft;

			private float _mostRight;

			internal void Initialize()
			{
				_spriteSize = _spriteRenderer.sprite.bounds.size;
				int num = Mathf.FloorToInt(60f / _spriteSize.x * 2f);
				_origin = new Vector2[num];
				_rendererInstances = new SpriteRenderer[num];
				_rendererInstances[0] = _spriteRenderer;
				for (int i = 1; i < _rendererInstances.Length; i++)
				{
					_rendererInstances[i] = UnityEngine.Object.Instantiate(_spriteRenderer, _spriteRenderer.transform.parent);
				}
				_mostRight = (float)(_rendererInstances.Length - 1) * _spriteSize.x / 2f;
				_mostLeft = 0f - _mostRight;
				for (int j = 0; j < _rendererInstances.Length; j++)
				{
					_origin[j] = new Vector2(_mostLeft + _spriteSize.x * (float)j - 0.03125f * (float)j, _spriteSize.y / 2f - Camera.main.orthographicSize);
					_rendererInstances[j].transform.localPosition = _origin[j];
				}
				if (_randomize)
				{
					_translated.x += UnityEngine.Random.Range(0f, _spriteSize.x);
				}
			}

			internal void Update(Vector2 delta, float deltaTime)
			{
				delta.x += _hotizontalAutoScroll * deltaTime;
				delta = Vector2.Scale(delta, _distance);
				_translated -= delta;
				if (_translated.x < _mostLeft)
				{
					_translated.x = _mostRight;
				}
				if (_translated.x > _mostRight)
				{
					_translated.x = _mostLeft;
				}
				for (int i = 0; i < _rendererInstances.Length; i++)
				{
					SpriteRenderer obj = _rendererInstances[i];
					Vector2 vector = _origin[i] + _translated;
					obj.transform.localPosition = vector;
				}
			}

			internal void SetAlpha(float alpha)
			{
				SpriteRenderer[] rendererInstances = _rendererInstances;
				foreach (SpriteRenderer obj in rendererInstances)
				{
					Color color = obj.color;
					Color color3 = (obj.color = new Color(color.r, color.g, color.b, alpha));
				}
			}
		}

		private const float _screenWidth = 1920f;

		private const float _pixelPerUnit = 32f;

		private const float _pixel = 0.03125f;

		[SerializeField]
		private Element.Reorderable _elements;

		private CameraController _cameraController;

		private void Awake()
		{
			Element[] values = _elements.values;
			for (int i = 0; i < values.Length; i++)
			{
				values[i].Initialize();
			}
			_cameraController = Scene<GameBase>.instance.cameraController;
			UpdateElements(_cameraController.delta, Chronometer.global.deltaTime);
		}

		private void Update()
		{
			UpdateElements(_cameraController.delta, Chronometer.global.deltaTime);
		}

		public void Initialize(float originHeight)
		{
			UpdateElements(new Vector3(0f, originHeight, 0f), 0f);
		}

		private void UpdateElements(Vector3 delta, float deltaTime)
		{
			Element[] values = _elements.values;
			for (int i = 0; i < values.Length; i++)
			{
				values[i].Update(delta, deltaTime);
			}
		}

		private void SetFadeAlpha(float alpha)
		{
			Element[] values = _elements.values;
			for (int i = 0; i < values.Length; i++)
			{
				values[i].SetAlpha(alpha);
			}
		}

		public void FadeIn()
		{
			StartCoroutine(CFadeIn());
		}

		public IEnumerator CFadeIn()
		{
			float t = 0f;
			SetFadeAlpha(1f);
			yield return null;
			for (; t < 1f; t += Time.unscaledDeltaTime * 0.3f)
			{
				SetFadeAlpha(1f - t);
				yield return null;
			}
			SetFadeAlpha(0f);
		}

		public void FadeOut()
		{
			StartCoroutine(CFadeOut());
		}

		public IEnumerator CFadeOut()
		{
			float t = 0f;
			SetFadeAlpha(0f);
			yield return null;
			for (; t < 1f; t += Time.unscaledDeltaTime * 0.3f)
			{
				SetFadeAlpha(t);
				yield return null;
			}
			SetFadeAlpha(1f);
		}
	}
}
