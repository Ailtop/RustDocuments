using UnityEngine;

public class AdventurerHealthBar : MonoBehaviour
{
	[SerializeField]
	private GameObject _deadPortrait;

	public void ShowDeadPortrait()
	{
		_deadPortrait.SetActive(true);
	}

	public void HideDeadPortrait()
	{
		_deadPortrait.SetActive(false);
	}
}
