using Scenes;
using UI;
using UnityEngine;

namespace Characters
{
	public class BossHealthbarAttacher : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		private BossHealthbarController.Type _type;

		private void Start()
		{
			if (_character != null)
			{
				Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.Open(_type, _character);
			}
		}
	}
}
