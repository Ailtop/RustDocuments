using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class NpcConfirmText : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Text _text;

		private bool _focus;

		public string text
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

		public bool focus
		{
			get
			{
				return _focus;
			}
			set
			{
				_focus = value;
				_text.color = (_focus ? Color.white : Color.gray);
			}
		}
	}
}
