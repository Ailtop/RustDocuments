using UnityEngine;

namespace Characters.AI
{
	public class Boss : MonoBehaviour
	{
		[SerializeField]
		private BossNameDisplay _bossNameDisplay;

		[SerializeField]
		private AIController _boss;

		public void ShowAppearanceText()
		{
			_bossNameDisplay.ShowAppearanceText();
		}

		public void HideAppearanceText()
		{
			_bossNameDisplay.HideAppearanceText();
		}

		private void OnDestroy()
		{
			HideAppearanceText();
		}
	}
}
