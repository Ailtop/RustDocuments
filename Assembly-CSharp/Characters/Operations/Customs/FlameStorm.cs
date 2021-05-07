using System.Collections;
using Characters.AI;
using Characters.Projectiles;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public sealed class FlameStorm : CharacterOperation
	{
		[SerializeField]
		private AIController _ai;

		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		[SerializeField]
		private Projectile _projectile;

		[SerializeField]
		private Transform _spawnPointContainer;

		[SerializeField]
		private int _emptyCount = 2;

		[SerializeField]
		private float _fireDelay = 1.5f;

		private int[] _numbers;

		private IAttackDamage _attackDamage;

		private void Awake()
		{
			_numbers = new int[_spawnPointContainer.childCount];
			for (int i = 0; i < _spawnPointContainer.childCount; i++)
			{
				_numbers[i] = i;
			}
		}

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<IAttackDamage>();
		}

		public override void Run(Character owner)
		{
			_numbers.Shuffle();
			for (int i = 0; i < _spawnPointContainer.childCount - _emptyCount; i++)
			{
				int index = _numbers[i];
				Vector3 position = _spawnPointContainer.GetChild(index).position;
				OperationInfos operationInfos = _operationRunner.Spawn().operationInfos;
				operationInfos.transform.position = position;
				operationInfos.Run(owner);
			}
			int index2 = _numbers[Random.Range(0, _spawnPointContainer.childCount - _emptyCount)];
			StartCoroutine(CFire(owner, _spawnPointContainer.GetChild(index2)));
		}

		private IEnumerator CFire(Character owner, Transform fireTransform)
		{
			yield return Chronometer.global.WaitForSeconds(_fireDelay);
			float direction = ((!(_ai.target.transform.position.x > fireTransform.position.x)) ? 180f : 0f);
			_projectile.reusable.Spawn(fireTransform.position).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, direction);
		}
	}
}
