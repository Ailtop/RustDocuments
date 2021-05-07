using UnityEngine;
using UnityEngine.UI;

public class QuintessenceDesc : MonoBehaviour
{
	[SerializeField]
	private Text _effectOfUse;

	[SerializeField]
	private Text _description;

	public string text
	{
		get
		{
			return _description.text;
		}
		set
		{
			_description.text = value;
		}
	}

	private void Awake()
	{
		_effectOfUse.text = Lingua.GetLocalizedString("quintessence_effectOfUse");
	}
}
