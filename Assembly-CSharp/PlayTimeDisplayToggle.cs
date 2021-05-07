using Data;
using UnityEngine;

public class PlayTimeDisplayToggle : MonoBehaviour
{
	[SerializeField]
	private GameObject _display;

	private bool _cachedShowTimer;

	private void Awake()
	{
		Refresh();
	}

	private void Refresh()
	{
		_cachedShowTimer = GameData.Settings.showTimer;
		_display.SetActive(GameData.Settings.showTimer);
	}

	private void Update()
	{
		if (_cachedShowTimer != GameData.Settings.showTimer)
		{
			Refresh();
		}
	}

	public void Toggle()
	{
		GameData.Settings.showTimer = !GameData.Settings.showTimer;
		Refresh();
	}
}
