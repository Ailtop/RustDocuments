using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Pause
{
	public class Selection : Selectable
	{
		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private PointerDownHandler _left;

		[SerializeField]
		private PointerDownHandler _right;

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

		protected override void Awake()
		{
			base.Awake();
			_left.onPointerDown = MoveLeft;
			_right.onPointerDown = MoveRight;
		}

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

		public override void OnMove(AxisEventData eventData)
		{
			switch (eventData.moveDir)
			{
			case MoveDirection.Left:
				MoveLeft();
				break;
			case MoveDirection.Right:
				MoveRight();
				break;
			default:
				base.OnMove(eventData);
				break;
			}
		}

		public void MoveLeft()
		{
			value--;
		}

		public void MoveRight()
		{
			value++;
		}
	}
}
