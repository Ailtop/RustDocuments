using Characters.Operations;
using UnityEngine;

namespace Characters
{
	[RequireComponent(typeof(PoolObject), typeof(OperationInfos))]
	public class OperationRunner : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private PoolObject _poolObject;

		[SerializeField]
		[Information("소환할 때 Copy Attack Damage 옵션 사용할 경우 반드시 설정해줘야 함. 이 때 min max는 어차피 덮어씌워지므로 무관", InformationAttribute.InformationType.Info, false)]
		private AttackDamage _attackDamage;

		[SerializeField]
		[GetComponent]
		private OperationInfos _operationInfos;

		public OperationInfos operationInfos => _operationInfos;

		public PoolObject poolObject => _poolObject;

		public AttackDamage attackDamage => _attackDamage;

		private void Awake()
		{
			_operationInfos.Initialize();
			_operationInfos.onEnd += _poolObject.Despawn;
		}

		public OperationRunner Spawn()
		{
			return _poolObject.Spawn().GetComponent<OperationRunner>();
		}
	}
}
