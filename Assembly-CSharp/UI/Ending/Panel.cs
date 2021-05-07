using Characters.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Ending
{
	public class Panel : MonoBehaviour
	{
		[SerializeField]
		private Image _focus;

		[SerializeField]
		private UnityEngine.UI.Button _tumblbug;

		[SerializeField]
		private UnityEngine.UI.Button _twitter;

		[SerializeField]
		private UnityEngine.UI.Button _openTrade;

		[SerializeField]
		private UnityEngine.UI.Button _tumblr;

		[SerializeField]
		private UnityEngine.UI.Button _newGame;

		[SerializeField]
		private UnityEngine.UI.Button _quit;

		private void Awake()
		{
			_tumblbug.onClick.AddListener(delegate
			{
				Application.OpenURL("https://tumblbug.com/skul");
			});
			_twitter.onClick.AddListener(delegate
			{
				Application.OpenURL("https://twitter.com/Skul_game");
			});
			_openTrade.onClick.AddListener(delegate
			{
				Application.OpenURL("https://otrade.co/funding/334");
			});
			_tumblr.onClick.AddListener(delegate
			{
				Application.OpenURL("https://skulthegame.tumblr.com/");
			});
			_newGame.onClick.AddListener(delegate
			{
				base.gameObject.SetActive(false);
				SceneManager.LoadScene(0);
			});
			_quit.onClick.AddListener(Application.Quit);
		}

		private void OnEnable()
		{
			PlayerInput.blocked.Attach(this);
			Chronometer.global.AttachTimeScale(this, 0f);
			EventSystem.current.SetSelectedGameObject(_tumblbug.gameObject);
			_tumblbug.Select();
		}

		private void OnDisable()
		{
			PlayerInput.blocked.Detach(this);
			Chronometer.global.DetachTimeScale(this);
		}

		private void Update()
		{
			Transform transform = EventSystem.current.currentSelectedGameObject?.transform;
			if (!(transform == null))
			{
				RectTransform component = transform.GetComponent<RectTransform>();
				_focus.rectTransform.position = transform.position;
				Vector2 sizeDelta = component.sizeDelta;
				sizeDelta.x /= _focus.transform.localScale.x;
				sizeDelta.y /= _focus.transform.localScale.y;
				sizeDelta.x -= 6f;
				sizeDelta.y -= 6f;
				_focus.rectTransform.sizeDelta = sizeDelta;
			}
		}
	}
}
