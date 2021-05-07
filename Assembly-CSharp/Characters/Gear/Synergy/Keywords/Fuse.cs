using System.Collections;
using Characters.Operations;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Fuse : Keyword
	{
		[SerializeField]
		private int[] _damageByLevel = new int[6] { 0, 10, 15, 25, 40, 60 };

		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		public override Key key => Key.Fuse;

		protected override IList valuesByLevel => _damageByLevel;

		protected override void Initialize()
		{
		}

		protected override void UpdateBonus()
		{
		}

		protected override void OnAttach()
		{
			base.character.playerComponents.inventory.weapon.onSwap += SpawnOperationRunner;
		}

		protected override void OnDetach()
		{
			base.character.playerComponents.inventory.weapon.onSwap -= SpawnOperationRunner;
		}

		private void SpawnOperationRunner()
		{
			OperationInfos operationInfos = _operationRunner.Spawn().operationInfos;
			AttackDamage component = operationInfos.GetComponent<AttackDamage>();
			int num3 = (component.minAttackDamage = (component.maxAttackDamage = _damageByLevel[base.level]));
			operationInfos.transform.SetPositionAndRotation(base.transform.position, Quaternion.identity);
			operationInfos.Run(base.character);
		}
	}
}
