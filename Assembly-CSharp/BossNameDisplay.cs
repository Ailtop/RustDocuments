using Scenes;
using UI.Boss;
using UnityEngine;

public class BossNameDisplay : MonoBehaviour
{
	[SerializeField]
	private string _nameKey;

	[SerializeField]
	private string _subNameKey;

	[SerializeField]
	private string _chapterNameKey;

	public void ShowAppearanceText()
	{
		Scene<GameBase>.instance.uiManager.bossUI.appearnaceText.Appear(Lingua.GetLocalizedString(_nameKey), Lingua.GetLocalizedString(_subNameKey));
	}

	public void HideAppearanceText()
	{
		Scene<GameBase>.instance.uiManager.bossUI.appearnaceText.Disappear();
	}

	public void ShowAndHideAppearanceText()
	{
		BossUIContainer bossUI = Scene<GameBase>.instance.uiManager.bossUI;
		bossUI.StartCoroutine(bossUI.appearnaceText.ShowAndHideText(Lingua.GetLocalizedString(_nameKey), Lingua.GetLocalizedString(_subNameKey)));
	}

	private void OnDestroy()
	{
		HideAppearanceText();
	}
}
