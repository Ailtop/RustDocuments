using UnityEngine;

namespace Characters
{
	internal class WitchBonusAttacher : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		private void Awake()
		{
			WitchBonus.instance.Apply(_character);
		}
	}
}
