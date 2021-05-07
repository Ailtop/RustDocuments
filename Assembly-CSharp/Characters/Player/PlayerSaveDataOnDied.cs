using Data;
using UnityEngine;

namespace Characters.Player
{
	public class PlayerSaveDataOnDied : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		private void Awake()
		{
			_character.health.onDied += OnDied;
		}

		private void OnDied()
		{
			GameData.Currency.SaveAll();
			GameData.Progress.SaveAll();
		}
	}
}
