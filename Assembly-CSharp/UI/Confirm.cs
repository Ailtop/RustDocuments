using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UserInput;

namespace UI
{
	public class Confirm : Dialogue
	{
		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private Button _yes;

		[SerializeField]
		private Button _no;

		private float _elaspedTime;

		private GameObject _lastSelectedGameObject;

		public override bool closeWithPauseKey => true;

		public void Open(string text, Action action)
		{
			if (_text != null)
			{
				_text.text = text;
			}
			_yes.onClick.RemoveAllListeners();
			_yes.onClick.AddListener(delegate
			{
				if (_elaspedTime > 0.3f)
				{
					Close();
					action();
				}
			});
			_no.onClick.RemoveAllListeners();
			_no.onClick.AddListener(delegate
			{
				if (_elaspedTime > 0.3f)
				{
					Close();
				}
			});
			Open();
		}

		public void Open(string text, Action onYes, Action onNo)
		{
			if (_text != null)
			{
				_text.text = text;
			}
			_yes.onClick.RemoveAllListeners();
			_yes.onClick.AddListener(delegate
			{
				if (_elaspedTime > 0.3f)
				{
					Close();
					onYes();
				}
			});
			_no.onClick.RemoveAllListeners();
			_no.onClick.AddListener(delegate
			{
				if (_elaspedTime > 0.3f)
				{
					Close();
					onNo();
				}
			});
			Open();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Chronometer.global.AttachTimeScale(this, 0f);
			_elaspedTime = 0f;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Chronometer.global.DetachTimeScale(this);
		}

		private void Update()
		{
			EventSystem current = EventSystem.current;
			if (current.currentSelectedGameObject == null)
			{
				current.SetSelectedGameObject(_lastSelectedGameObject);
			}
			else
			{
				_lastSelectedGameObject = current.currentSelectedGameObject;
			}
			if (KeyMapper.Map.Pause.WasPressed)
			{
				_no.onClick.Invoke();
			}
			_elaspedTime += Time.unscaledDeltaTime;
		}
	}
}
