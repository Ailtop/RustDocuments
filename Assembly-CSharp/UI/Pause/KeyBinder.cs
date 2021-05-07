using InControl;
using UnityEngine;
using UnityEngine.UI;
using UserInput;

namespace UI.Pause
{
	public class KeyBinder : MonoBehaviour
	{
		[SerializeField]
		private Button _button;

		[SerializeField]
		private Image _image;

		private PlayerAction _action;

		private BindingSource _bindingSource;

		public void Initialize(PlayerAction action, PressNewKey pressNewKey)
		{
			_003C_003Ec__DisplayClass4_0 _003C_003Ec__DisplayClass4_ = new _003C_003Ec__DisplayClass4_0();
			_003C_003Ec__DisplayClass4_.pressNewKey = pressNewKey;
			_003C_003Ec__DisplayClass4_.action = action;
			_003C_003Ec__DisplayClass4_._003C_003E4__this = this;
			_button.onClick.AddListener(_003C_003Ec__DisplayClass4_._003CInitialize_003Eg__OnClick_007C0);
			_action = _003C_003Ec__DisplayClass4_.action;
			_action.OnBindingsChanged += UpdateKeyImageAndBindingSource;
			UpdateKeyImageAndBindingSource();
		}

		private void OnDisable()
		{
			_action.OnBindingsChanged -= UpdateKeyImageAndBindingSource;
		}

		private void Update()
		{
			UpdateKeyImageAndBindingSource();
		}

		public void UpdateKeyImageAndBindingSource()
		{
			foreach (BindingSource binding in _action.Bindings)
			{
				if (KeyMap.SimplifyBindingSourceType(binding.BindingSourceType) == KeyMapper.Map.SimplifiedLastInputType)
				{
					_bindingSource = binding;
					break;
				}
			}
			Sprite sprite;
			if (Resource.instance.TryGetKeyIcon(_bindingSource, out sprite, true))
			{
				_image.sprite = sprite;
				_image.SetNativeSize();
			}
		}
	}
}
