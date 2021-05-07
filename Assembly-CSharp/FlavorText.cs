using Characters.Gear;
using UnityEngine;
using UnityEngine.UI;

public class FlavorText : MonoBehaviour
{
	[SerializeField]
	private Text _text;

	private Gear _gear;

	private void Awake()
	{
		_gear = GetComponentInParent<Gear>();
		if (!_gear.hasFlavor)
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			_text.text = _gear.flavor;
		}
	}
}
