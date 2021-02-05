using UnityEngine;

public class GameModeObjectToggle : BaseMonoBehaviour
{
	public string[] gameModeTags;

	public GameObject[] toToggle;

	public void Awake()
	{
		SetToggle(false);
		BaseGameMode.GameModeChanged += OnGameModeChanged;
	}

	public void OnDestroy()
	{
		BaseGameMode.GameModeChanged -= OnGameModeChanged;
	}

	public void OnGameModeChanged(BaseGameMode newGameMode)
	{
		bool toggle = ShouldBeVisible(newGameMode);
		SetToggle(toggle);
	}

	public void SetToggle(bool wantsOn)
	{
		GameObject[] array = toToggle;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(wantsOn);
		}
	}

	public bool ShouldBeVisible(BaseGameMode newGameMode)
	{
		if (newGameMode == null)
		{
			return false;
		}
		if (gameModeTags.Length == 0)
		{
			return true;
		}
		if (newGameMode.HasAnyGameModeTag(gameModeTags))
		{
			return true;
		}
		return false;
	}
}
