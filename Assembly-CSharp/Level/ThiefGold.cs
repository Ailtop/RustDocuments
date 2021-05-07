using UnityEngine;

namespace Level
{
	public class ThiefGold : MonoBehaviour
	{
		public delegate void OnDespawn(double goldAmount, Vector3 position);

		[SerializeField]
		private CurrencyParticle _goldParticle;

		public static event OnDespawn onDespawn;

		private void OnDisable()
		{
			ThiefGold.onDespawn?.Invoke(_goldParticle.currencyAmount, base.transform.position);
		}
	}
}
