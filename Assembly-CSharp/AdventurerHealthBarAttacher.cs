using Characters;
using Scenes;
using UnityEngine;

public class AdventurerHealthBarAttacher : MonoBehaviour
{
	[SerializeField]
	private AdventurerHealthBarUIController.AdventurerClass _adventurerClass;

	[SerializeField]
	private Character _character;

	private void Start()
	{
		_character.health.onDiedTryCatch += OnDied;
	}

	public void Show()
	{
		Scene<GameBase>.instance.uiManager.adventurerHealthBarUIController.Initialize(_character, _adventurerClass);
		Scene<GameBase>.instance.uiManager.adventurerHealthBarUIController.ShowHealthBarOf(_adventurerClass);
	}

	private void OnDestroy()
	{
		Scene<GameBase>.instance.uiManager.adventurerHealthBarUIController.HideDeadUIOf(_adventurerClass);
		Scene<GameBase>.instance.uiManager.adventurerHealthBarUIController.HideHealthBarOf(_adventurerClass);
		_character.health.onDiedTryCatch -= OnDied;
	}

	private void OnDied()
	{
		Scene<GameBase>.instance.uiManager.adventurerHealthBarUIController.ShowDeadUIOf(_adventurerClass);
		_character.health.onDiedTryCatch -= OnDied;
	}
}
