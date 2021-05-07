using System.Collections;
using Characters.Gear.Weapons;
using Services;
using Singletons;
using UnityEngine;

namespace Tutorials
{
	public class GiveBasicSkul : MonoBehaviour
	{
		[SerializeField]
		private Weapon _basicSkul;

		private IEnumerator Start()
		{
			yield return null;
			Singleton<Service>.Instance.levelManager.player.CancelAction();
			Run();
			Object.Destroy(this);
		}

		private void Run()
		{
			Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon.ForceEquipAt(_basicSkul.Instantiate(), 0);
		}
	}
}
