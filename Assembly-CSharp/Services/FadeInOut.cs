using System.Collections;
using UnityEngine;

namespace Services
{
	public class FadeInOut : MonoBehaviour
	{
		private bool _fading;

		private Texture2D _texture;

		private GUIStyle _backgroundStyle = new GUIStyle();

		private Color _color = Color.black;

		private void Awake()
		{
			_texture = new Texture2D(1, 1);
			_backgroundStyle.normal.background = _texture;
		}

		private void OnGUI()
		{
			if (_fading)
			{
				GUI.Label(new Rect(-10f, -10f, Screen.width + 10, Screen.height + 10), _texture, _backgroundStyle);
			}
		}

		private void SetFadeAlpha(float alpha)
		{
			_color.a = alpha;
			_texture.SetPixel(0, 0, _color);
			_texture.Apply();
		}

		public void SetFadeColor(Color color)
		{
			_color = color;
		}

		public void FadeIn()
		{
			StartCoroutine(CFadeIn());
		}

		public IEnumerator CFadeIn()
		{
			_fading = true;
			float t = 0f;
			SetFadeAlpha(1f);
			yield return null;
			for (; t < 1f; t += Time.unscaledDeltaTime * 2f)
			{
				SetFadeAlpha(1f - t);
				yield return null;
			}
			SetFadeAlpha(0f);
			_fading = false;
		}

		public void FadeOut()
		{
			StartCoroutine(CFadeOut());
		}

		public IEnumerator CFadeOut()
		{
			_fading = true;
			float t = 0f;
			SetFadeAlpha(0f);
			yield return null;
			for (; t < 1f; t += Time.unscaledDeltaTime * 2f)
			{
				SetFadeAlpha(t);
				yield return null;
			}
			SetFadeAlpha(1f);
		}
	}
}
