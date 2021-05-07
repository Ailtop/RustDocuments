using Characters;
using TMPro;
using UnityEngine;

namespace UI
{
	public class VeteranHealthbarController : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _title;

		[SerializeField]
		private CharacterHealthBar _healthbar;

		[SerializeField]
		private HangingPanelAnimator _animator;

		public void Appear(Character character, string nameKey, string titleKey)
		{
			LocalizeText(_name, nameKey);
			LocalizeText(_title, titleKey);
			_healthbar.Initialize(character);
			_animator.Appear();
		}

		public void Disappear()
		{
			if (_healthbar.gameObject.activeSelf)
			{
				_animator.Disappear();
			}
		}

		private void LocalizeText(TMP_Text ui, string key)
		{
			if (!string.IsNullOrWhiteSpace(key))
			{
				ui.text = Lingua.GetLocalizedString(key);
			}
		}
	}
}
