using Scenes;
using UnityEngine;

public class StageInfoDisplay : MonoBehaviour
{
	[SerializeField]
	private string nameKey;

	[SerializeField]
	private string stageNumber;

	[SerializeField]
	private string subNameKey;

	private void Start()
	{
		Scene<GameBase>.instance.uiManager.stageName.Show(Lingua.GetLocalizedString(nameKey), Lingua.GetLocalizedString(stageNumber), Lingua.GetLocalizedString(subNameKey));
	}
}
