using Characters.Gear.Weapons.Rockstar;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class SummonRockstarAmp : CharacterOperation
	{
		[SerializeField]
		private Amp _rockstarAmp;

		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private bool _flipX;

		public override void Run(Character owner)
		{
			_rockstarAmp.InstantiateAmp(_spawnPosition.position, _flipX);
		}
	}
}
