using System;
using System.Collections;
using FX;
using UnityEngine;

namespace Characters.Operations.Summon
{
	public class SummonMultipleOperationRunners : CharacterOperation
	{
		[Serializable]
		private class SummonOption
		{
			[Serializable]
			public class Reorderable : ReorderableArray<SummonOption>
			{
			}

			[Tooltip("오퍼레이션 프리팹")]
			public OperationRunner operationRunner;

			[Tooltip("오퍼레이션이 스폰되는 시간")]
			public float timeToSpawn;

			public Transform spawnPosition;

			public CustomFloat scale = new CustomFloat(1f);

			public CustomAngle angle;

			public PositionNoise noise;

			[Tooltip("주인이 바라보고 있는 방향에 따라 X축으로 플립")]
			public bool flipXByLookingDirection;

			[Tooltip("체크하면 주인에 부착되며, 같이 움직임")]
			public bool attachToOwner;

			[Tooltip("체크하면 캐릭터의 Attach오브젝트에 부착되며, 같이 움직임, 플립안됨")]
			public bool notFlipOnOwner;

			public bool copyAttackDamage;
		}

		[SerializeField]
		[Tooltip("발동 시점에 미리 위치를 받아옵니다. 캐릭터가 이동해도 위치가 바뀌지 않게해야할 때 유용합니다.")]
		private bool _preloadPosition;

		[SerializeField]
		private SummonOption.Reorderable _summonOptions;

		private AttackDamage _attackDamage;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<AttackDamage>();
		}

		public override void Run(Character owner)
		{
			StartCoroutine(CRun(owner));
		}

		private IEnumerator CRun(Character owner)
		{
			Vector3[] preloadedPositions = new Vector3[_summonOptions.values.Length];
			if (_preloadPosition)
			{
				for (int i = 0; i < preloadedPositions.Length; i++)
				{
					Transform spawnPosition = _summonOptions.values[i].spawnPosition;
					if (spawnPosition == null)
					{
						preloadedPositions[i] = base.transform.position;
					}
					else
					{
						preloadedPositions[i] = spawnPosition.position;
					}
				}
			}
			int optionIndex = 0;
			float time = 0f;
			SummonOption[] options = _summonOptions.values;
			while (optionIndex < options.Length)
			{
				for (; optionIndex < options.Length && time >= options[optionIndex].timeToSpawn; optionIndex++)
				{
					SummonOption summonOption = options[optionIndex];
					Vector3 position = ((!_preloadPosition) ? ((summonOption.spawnPosition == null) ? base.transform.position : summonOption.spawnPosition.position) : preloadedPositions[optionIndex]);
					position += summonOption.noise.Evaluate();
					Vector3 euler = new Vector3(0f, 0f, summonOption.angle.value);
					bool num = summonOption.flipXByLookingDirection && owner.lookingDirection == Character.LookingDirection.Left;
					if (num)
					{
						euler.z = (180f - euler.z) % 360f;
					}
					OperationRunner operationRunner = summonOption.operationRunner.Spawn();
					OperationInfos operationInfos = operationRunner.operationInfos;
					operationInfos.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
					if (summonOption.copyAttackDamage && _attackDamage != null)
					{
						operationRunner.attackDamage.minAttackDamage = _attackDamage.minAttackDamage;
						operationRunner.attackDamage.maxAttackDamage = _attackDamage.maxAttackDamage;
					}
					if (num)
					{
						operationInfos.transform.localScale = new Vector3(1f, -1f, 1f);
					}
					else
					{
						operationInfos.transform.localScale = new Vector3(1f, 1f, 1f);
					}
					operationInfos.Run(owner);
					if (summonOption.attachToOwner)
					{
						operationInfos.transform.parent = base.transform;
					}
					if (summonOption.notFlipOnOwner && owner.attach != null)
					{
						operationInfos.transform.parent = owner.attach.transform;
					}
				}
				yield return null;
				time += owner.chronometer.animation.deltaTime;
			}
		}

		public override void Stop()
		{
			base.Stop();
			StopAllCoroutines();
		}
	}
}
