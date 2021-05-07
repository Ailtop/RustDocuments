using TMPro;
using UnityEngine;

[SerializeField]
public class VersionDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	private void Awake()
	{
		_text.text = Application.version;
		Object.Destroy(this);
	}
}
