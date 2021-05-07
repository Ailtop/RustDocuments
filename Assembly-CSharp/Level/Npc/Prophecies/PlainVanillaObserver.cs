using Characters;
using UnityEngine;

namespace Level.Npc.Prophecies
{
	public class PlainVanillaObserver : MonoBehaviour
	{
		[SerializeField]
		private Character _yggdrasil;

		private void Awake()
		{
			if (!Prophecy.plainVanilla.canFulfil)
			{
				Object.Destroy(this);
			}
			else
			{
				_yggdrasil.health.onDied += Prophecy.plainVanilla.Fulfil;
			}
		}
	}
}
