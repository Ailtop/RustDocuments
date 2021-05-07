using Characters.Movements;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class Knockback : TargetedCharacterOperation
	{
		[SerializeField]
		[Information("이 값을 지정해주면 오퍼레이션 소유 캐릭터 대신 해당 트랜스폼을 기준으로 넉백합니다.", InformationAttribute.InformationType.Info, false)]
		private Transform _transfromOverride;

		[SerializeField]
		private PushInfo _pushInfo = new PushInfo(false, false);

		public PushInfo pushInfo => _pushInfo;

		public override void Run(Character owner, Character target)
		{
			if (_transfromOverride == null)
			{
				target.movement.push.ApplyKnockback(owner, _pushInfo);
			}
			else
			{
				target.movement.push.ApplyKnockback(_transfromOverride, _pushInfo);
			}
		}
	}
}
