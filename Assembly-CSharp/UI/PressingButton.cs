using System;
using System.Collections;
using FX;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInput;

namespace UI
{
	public class PressingButton : MonoBehaviour
	{
		private const float _pressingTime = 1f;

		[Space]
		[SerializeField]
		private bool _detectPressingSelf;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private Image _iconOutline;

		[SerializeField]
		private TMP_Text _text;

		[Space]
		[SerializeField]
		private string _actionName;

		[Space]
		[SerializeField]
		private PlaySoundInfo _pressingSound;

		private PlayerAction _action;

		private CoroutineReference _pressing;

		public string description
		{
			get
			{
				return _text.text;
			}
			set
			{
				_text.text = value;
			}
		}

		public event Action onPressed;

		private void Awake()
		{
			_action = FindAction();
			if (_action == null)
			{
				throw new Exception("Couldn't found key " + _actionName);
			}
			KeyMapper.Map.OnSimplifiedLastInputTypeChanged += OnLastInputTypeChanged;
			_action.OnBindingsChanged += UpdateImage;
		}

		private void OnEnable()
		{
			StopPressingSound();
			if (_detectPressingSelf)
			{
				StartCoroutine(CWaitForPressing());
			}
			UpdateImage();
		}

		private void OnDisable()
		{
			StopPressing();
		}

		private void Start()
		{
			UpdateImage();
		}

		private void OnDestroy()
		{
			KeyMapper.Map.OnSimplifiedLastInputTypeChanged -= OnLastInputTypeChanged;
			_action.OnBindingsChanged -= UpdateImage;
		}

		public void PlayPressingSound()
		{
			_pressingSound.Play();
		}

		public void StopPressingSound()
		{
			_pressingSound.Stop();
		}

		public void StopPressing()
		{
			_pressing.Stop();
			_iconOutline.fillAmount = 1f;
			StopPressingSound();
		}

		public void SetPercent(float percent)
		{
			_iconOutline.fillAmount = percent;
		}

		private void OnLastInputTypeChanged(BindingSourceType bindingSourceType)
		{
			UpdateImage();
		}

		private void UpdateImage()
		{
			if (_icon != null)
			{
				_icon.SetNativeSize();
			}
			BindingSourceType simplifiedLastInputType = KeyMapper.Map.SimplifiedLastInputType;
			foreach (BindingSource binding in _action.Bindings)
			{
				BindingSourceType bindingSourceType = KeyMap.SimplifyBindingSourceType(binding.BindingSourceType);
				if (simplifiedLastInputType == bindingSourceType)
				{
					Sprite keyIconOrDefault = Resource.instance.GetKeyIconOrDefault(binding);
					if (_icon != null)
					{
						_icon.sprite = keyIconOrDefault;
						_icon.SetNativeSize();
					}
					keyIconOrDefault = Resource.instance.GetKeyIconOrDefault(binding, true);
					if (_icon != null)
					{
						_iconOutline.sprite = keyIconOrDefault;
						_iconOutline.SetNativeSize();
					}
					break;
				}
			}
		}

		private PlayerAction FindAction()
		{
			foreach (PlayerAction action in KeyMapper.Map.Actions)
			{
				if (action.Name.Equals(_actionName, StringComparison.OrdinalIgnoreCase))
				{
					return action;
				}
			}
			return null;
		}

		private IEnumerator CWaitForPressing()
		{
			while (true)
			{
				if (_action.WasPressed)
				{
					_pressing = this.StartCoroutineWithReference(CPressing());
				}
				yield return null;
			}
		}

		private IEnumerator CPressing()
		{
			PlayPressingSound();
			for (float time = 0f; time < 1f; time += Time.unscaledDeltaTime)
			{
				if (!_action.IsPressed)
				{
					StopPressing();
					yield break;
				}
				yield return null;
				SetPercent(time / 1f);
			}
			StopPressingSound();
			_iconOutline.fillAmount = 1f;
			this.onPressed?.Invoke();
		}
	}
}
