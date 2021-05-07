using Characters.Abilities.Customs;
using UnityEngine;

namespace Characters.Operations.Customs.BombSkul
{
	public class AddDamageStack : Operation
	{
		[SerializeField]
		private BombSkulPassiveComponent _bombSkulPassive;

		[SerializeField]
		private int _damageStacks;

		public override void Run()
		{
			_bombSkulPassive.AddDamageStack(_damageStacks);
		}
	}
}
