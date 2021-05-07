using System.Collections;
using UI.Pause;
using UnityEngine;
using UnityEngine.UI;
using UserInput;

namespace SkulStories
{
	public class Narration : MonoBehaviour
	{
		[SerializeField]
		private GameObject _sceneContainer;

		[SerializeField]
		private GameObject _textContainer;

		[SerializeField]
		private NarrationBody _body;

		[SerializeField]
		private Image _blackScreen;

		[SerializeField]
		private Image _enter;

		[SerializeField]
		[PauseEvent.Subcomponent]
		private PauseEvent _pauseEvent;

		[SerializeField]
		private PauseEventSystem _pauseEventSystem;

		public bool skipped { get; set; }

		public bool sceneVisible
		{
			get
			{
				return _sceneContainer.activeSelf;
			}
			set
			{
				if (_sceneContainer.activeSelf != value)
				{
					_sceneContainer.SetActive(value);
					Initialize();
				}
			}
		}

		public bool textVisible
		{
			get
			{
				return _textContainer.activeSelf;
			}
			set
			{
				if (_textContainer.activeSelf != value)
				{
					_textContainer.SetActive(value);
				}
			}
		}

		public bool skippable
		{
			get
			{
				return _body.skippable;
			}
			set
			{
				_body.skippable = value;
			}
		}

		public Image blackScreen => _blackScreen;

		public void Initialize()
		{
			StopAllCoroutines();
			textVisible = false;
			skippable = false;
			ResetPauseType();
		}

		public IEnumerator CShowText(ShowTexts sequence, string text)
		{
			if (!skippable && sceneVisible)
			{
				ShowText();
				yield return _body.CShow(sequence, text);
				Clear();
			}
		}

		public void CombineTexts(string[] texts)
		{
			if (!skippable && sceneVisible)
			{
				ShowText();
				_body.PlaceText(texts);
			}
		}

		private void ShowText()
		{
			StartCoroutine(CRun());
			textVisible = true;
			_enter.enabled = false;
		}

		public IEnumerator CWaitInput()
		{
			yield return Chronometer.global.WaitForSeconds(0.5f);
			do
			{
				yield return null;
			}
			while (!KeyMapper.Map.Attack.WasPressed && !KeyMapper.Map.Jump.WasPressed && !KeyMapper.Map.Submit.WasPressed);
			if (skippable)
			{
				Skip();
			}
		}

		private void Clear()
		{
			if (_body.isClear)
			{
				_enter.enabled = true;
				StartCoroutine(CWaitInput());
			}
		}

		private IEnumerator CRun()
		{
			while (!skippable)
			{
				yield return null;
			}
			Clear();
		}

		private void Skip()
		{
			skipped = true;
			textVisible = false;
		}

		private void ResetPauseType()
		{
			if (sceneVisible)
			{
				_pauseEventSystem.PushEvent(_pauseEvent);
			}
			else
			{
				_pauseEventSystem.PopEvent();
			}
		}
	}
}
