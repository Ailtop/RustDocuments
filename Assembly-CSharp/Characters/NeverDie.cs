using UnityEngine;

namespace Characters
{
	public class NeverDie : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		private void Awake()
		{
			_character.onDie += OnDie;
		}

		private void OnDie()
		{
			_character.health.ResetToMaximumHealth();
		}
	}
}
