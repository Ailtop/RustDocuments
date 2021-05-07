using Characters.Abilities.Customs;
using UnityEngine;

namespace Characters.Operations.Customs.BombSkul
{
	public class RiskyUpgrade : Operation
	{
		[SerializeField]
		private BombSkulPassiveComponent _bombSkulPassive;

		public override void Run()
		{
			_bombSkulPassive.RiskyUpgrade();
		}
	}
}
