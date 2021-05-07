using Characters.Gear;
using UnityEngine;
using UnityEngine.UI;

public class GearInfo : MonoBehaviour
{
	private Gear _gear;

	[SerializeField]
	private Text _rarity;

	[SerializeField]
	private Text _name;

	[SerializeField]
	private Image _icon;

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_gear = GetComponentInParent<Gear>();
		Text rarity = _rarity;
		Rarity rarity2 = _gear.rarity;
		rarity.text = ((object)(Rarity)(ref rarity2)).ToString();
		_name.text = _gear.displayName;
		_icon.sprite = _gear.dropped.GetComponent<SpriteRenderer>().sprite;
	}
}
