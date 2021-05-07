using System.Collections;
using Scenes;
using UnityEngine;

namespace FX
{
	public class GameFadeInOut : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		private Color _color = Color.black;

		private float _pixelPerUnit;

		private float _width;

		private float _height;

		private void Awake()
		{
			_pixelPerUnit = _spriteRenderer.sprite.pixelsPerUnit;
			_width = _spriteRenderer.sprite.rect.width;
			_height = _spriteRenderer.sprite.rect.height;
		}

		public void SetFadeColor(Color color)
		{
			_color = color;
		}

		private void SetFadeAlpha(float alpha)
		{
			_color.a = alpha;
			_spriteRenderer.color = _color;
		}

		private void FullScreen()
		{
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

		public void FadeIn(float speed = 1f)
		{
			Activate();
			StartCoroutine(CFadeIn(speed));
		}

		public void Activate()
		{
			base.gameObject.SetActive(true);
		}

		public void Deactivate()
		{
			base.gameObject.SetActive(false);
		}

		public IEnumerator CFadeIn(float speed)
		{
			FullScreen();
			for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * speed)
			{
				SetFadeAlpha(1f - t);
				yield return null;
			}
			SetFadeAlpha(0f);
		}

		public void FadeOut(float speed = 1f)
		{
			Activate();
			StartCoroutine(CFadeOut(speed));
		}

		public IEnumerator CFadeOut(float speed)
		{
			FullScreen();
			for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * speed)
			{
				SetFadeAlpha(t);
				yield return null;
			}
			SetFadeAlpha(1f);
		}
	}
}
