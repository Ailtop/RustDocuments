using UnityEngine;

namespace UI.Boss
{
	public class BossUIContainer : MonoBehaviour
	{
		[SerializeField]
		private BossAppearnaceText _appearnaceText;

		[SerializeField]
		private GameObject _container;

		public BossAppearnaceText appearnaceText => _appearnaceText;
	}
}
