using Characters.Gear.Quintessences;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
	public class QuintessenceOption : MonoBehaviour
	{
		[SerializeField]
		private Image _thumnailIcon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _rarity;

		[SerializeField]
		private TMP_Text _cooldown;

		[Space]
		[SerializeField]
		private TMP_Text _flavor;

		[SerializeField]
		private TMP_Text _passive;

		[Space]
		[SerializeField]
		private TMP_Text _activeName;

		[SerializeField]
		private TMP_Text _activeDescription;

		public void Set(Quintessence essence)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			_thumnailIcon.enabled = true;
			_thumnailIcon.sprite = essence.thumbnail;
			_thumnailIcon.transform.localScale = Vector3.one * 3f;
			_thumnailIcon.SetNativeSize();
			_name.text = essence.displayName;
			_rarity.text = Lingua.GetLocalizedString(string.Format("{0}/{1}/{2}", "label", "Rarity", essence.rarity));
			_cooldown.text = essence.cooldown.time.cooldownTime.ToString();
			_flavor.text = (essence.hasFlavor ? essence.flavor : string.Empty);
			_passive.text = essence.description;
			_activeName.text = essence.activeName;
			_activeDescription.text = essence.activeDescription;
		}
	}
}
