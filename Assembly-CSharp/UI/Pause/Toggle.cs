using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Pause
{
	public class Toggle : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
	{
		[SerializeField]
		private TMP_Text _text;

		private IList<string> _texts;

		private int _value;

		public int value
		{
			get
			{
				return _value;
			}
			set
			{
				SetValueWithoutNotify(value);
				this.onValueChanged?.Invoke(_value);
			}
		}

		public string text => _texts[value];

		public event Action<int> onValueChanged;

		private void ValidateValue()
		{
			if (_value < 0)
			{
				_value = _texts.Count - 1;
			}
			else if (_value >= _texts.Count)
			{
				_value %= _texts.Count;
			}
		}

		public void SetTexts(IList<string> texts)
		{
			_texts = texts;
			ValidateValue();
			_text.text = _texts[_value];
		}

		public void SetValueWithoutNotify(int value)
		{
			_value = value;
			ValidateValue();
			_text.text = _texts[_value];
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			value++;
		}

		public void OnSubmit(BaseEventData eventData)
		{
			value++;
		}
	}
}
