using System.Collections;
using UnityEngine;

namespace Characters.AI.Adventurer.Magician
{
	public class MagicianPlatform : MonoBehaviour
	{
		[SerializeField]
		private bool _left;

		[SerializeField]
		private SpriteRenderer _renderer;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private float _lifeTime;

		private MagicianPlatformController _controller;

		public void Initialize(MagicianPlatformController controller)
		{
			_controller = controller;
		}

		public void Show()
		{
			base.gameObject.SetActive(true);
			_collider.enabled = true;
			StartCoroutine(CStartLifeCycle());
		}

		private void Hide()
		{
			base.gameObject.SetActive(false);
			_collider.enabled = false;
			_controller.AddPlatform(this, _left);
		}

		private IEnumerator CStartLifeCycle()
		{
			FadeOut();
			yield return Chronometer.global.WaitForSeconds(_lifeTime);
			FadeIn();
		}

		private void FadeOut()
		{
			StartCoroutine(CFadeOut());
		}

		private IEnumerator CFadeOut()
		{
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

		private void FadeIn()
		{
			StartCoroutine(CFadeIn());
		}

		private IEnumerator CFadeIn()
		{
			float t = 0f;
			SetFadeAlpha(1f);
			yield return null;
			for (; t < 1f; t += Time.unscaledDeltaTime * 2f)
			{
				SetFadeAlpha(1f - t);
				yield return null;
			}
			SetFadeAlpha(0f);
			Hide();
		}

		private void SetFadeAlpha(float alpha)
		{
			Color color = _renderer.color;
			color.a = alpha;
			_renderer.color = color;
		}
	}
}
