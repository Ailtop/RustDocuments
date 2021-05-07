using System.Collections;
using Characters.Controllers;
using Scenes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class LetterBox : MonoBehaviour
	{
		private const float _defaultAnimationDuration = 0.4f;

		[SerializeField]
		private Image _top;

		[SerializeField]
		private Image _bottom;

		private float _originHeight;

		public static LetterBox instance => Scene<GameBase>.instance.uiManager.letterBox;

		public bool visible
		{
			get
			{
				return base.gameObject.activeSelf;
			}
			set
			{
				base.gameObject.SetActive(value);
			}
		}

		private void Awake()
		{
			_originHeight = _top.rectTransform.sizeDelta.y;
		}

		private void OnDisable()
		{
			PlayerInput.blocked.Detach(this);
		}

		public void Appear(float duration = 0.4f)
		{
			StopAllCoroutines();
			visible = true;
			StartCoroutine(CAppear(duration));
		}

		public void Disappear(float duration = 0.4f)
		{
			if (base.gameObject.activeSelf)
			{
				StartCoroutine(CDisappear(duration));
			}
		}

		public IEnumerator CAppear(float duration = 0.4f)
		{
			PlayerInput.blocked.Attach(this);
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = false;
			visible = true;
			float elapsed = 0f;
			float source = 0f;
			float destination = _originHeight;
			while (true)
			{
				float y = Mathf.Lerp(source, destination, elapsed / duration);
				_top.rectTransform.sizeDelta = new Vector2(_top.rectTransform.sizeDelta.x, y);
				_bottom.rectTransform.sizeDelta = new Vector2(_bottom.rectTransform.sizeDelta.x, y);
				if (!(elapsed > duration))
				{
					elapsed += Chronometer.global.deltaTime;
					yield return null;
					continue;
				}
				break;
			}
		}

		public IEnumerator CDisappear(float duration = 0.4f)
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = true;
			float elapsed = 0f;
			float destination = 0f;
			while (true)
			{
				float y = Mathf.Lerp(_originHeight, destination, elapsed / duration);
				_top.rectTransform.sizeDelta = new Vector2(_top.rectTransform.sizeDelta.x, y);
				_bottom.rectTransform.sizeDelta = new Vector2(_bottom.rectTransform.sizeDelta.x, y);
				if (elapsed > duration)
				{
					break;
				}
				elapsed += Chronometer.global.deltaTime;
				yield return null;
			}
			visible = false;
			PlayerInput.blocked.Detach(this);
		}
	}
}
