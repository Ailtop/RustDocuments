using Characters.Gear.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SkillDesc : MonoBehaviour
	{
		[SerializeField]
		private Image _image;

		[SerializeField]
		private Text _text;

		private SkillInfo _info;

		public SkillInfo info
		{
			get
			{
				return _info;
			}
			set
			{
				_info = value;
				_image.sprite = _info.cachedIcon;
				_image.preserveAspect = true;
				_text.text = _info.displayName;
			}
		}
	}
}
