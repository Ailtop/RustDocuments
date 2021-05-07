using Services;
using Singletons;
using UnityEngine;

namespace Characters
{
	public class EffectOnEvade : MonoBehaviour
	{
		[GetComponent]
		[SerializeField]
		private Character _character;

		private const string _floatingTextKey = "floating/evade";

		private const string _floatingTextColor = "#a3a3a3";

		public void Awake()
		{
			_character.onEvade += SpawnFloatingText;
		}

		private void SpawnFloatingText(ref Damage damage)
		{
			Vector2 vector = MMMaths.RandomPointWithinBounds(_character.collider.bounds);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnEvade(Lingua.GetLocalizedString("floating/evade"), vector, "#a3a3a3");
		}
	}
}
