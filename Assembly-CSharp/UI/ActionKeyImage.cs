using System;
using InControl;
using UnityEngine;
using UnityEngine.UI;
using UserInput;

namespace UI
{
	public class ActionKeyImage : MonoBehaviour
	{
		[SerializeField]
		private Image _image;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private string _actionName;

		[SerializeField]
		private bool _outline;

		private PlayerAction _action;

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
			UpdateImage();
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

		private void OnLastInputTypeChanged(BindingSourceType bindingSourceType)
		{
			UpdateImage();
		}

		private void UpdateImage()
		{
			if (_image != null)
			{
				_image.SetNativeSize();
			}
			BindingSourceType simplifiedLastInputType = KeyMapper.Map.SimplifiedLastInputType;
			foreach (BindingSource binding in _action.Bindings)
			{
				BindingSourceType bindingSourceType = KeyMap.SimplifyBindingSourceType(binding.BindingSourceType);
				if (simplifiedLastInputType == bindingSourceType)
				{
					Sprite keyIconOrDefault = Resource.instance.GetKeyIconOrDefault(binding, _outline);
					if (_image != null)
					{
						_image.sprite = keyIconOrDefault;
						_image.SetNativeSize();
					}
					if (_spriteRenderer != null)
					{
						_spriteRenderer.sprite = keyIconOrDefault;
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
	}
}
