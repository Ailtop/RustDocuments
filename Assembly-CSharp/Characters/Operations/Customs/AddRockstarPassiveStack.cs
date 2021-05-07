using Characters.Abilities.Customs;
using Characters.Player;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class AddRockstarPassiveStack : Operation
	{
		[SerializeField]
		[Tooltip("비워두면 자동으로 찾고, 못찾으면 그냥 실행 안함(서먼으로 사용할 때 유용)")]
		private RockstarPassiveComponent _rockstarPassive;

		[SerializeField]
		private int _amount;

		public override void Run()
		{
			if (_rockstarPassive == null)
			{
				WeaponInventory weapon = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon;
				_rockstarPassive = weapon.polymorphOrCurrent.GetComponent<RockstarPassiveComponent>();
				if (_rockstarPassive == null)
				{
					return;
				}
			}
			_rockstarPassive.AddStack(_amount);
		}
	}
}
