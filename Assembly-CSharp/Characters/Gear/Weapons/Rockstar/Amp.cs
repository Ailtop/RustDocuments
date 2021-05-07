using System;
using System.Collections.Generic;
using Characters.Operations;
using UnityEngine;

namespace Characters.Gear.Weapons.Rockstar
{
	public class Amp : MonoBehaviour
	{
		[Serializable]
		private class Timing
		{
			[SerializeField]
			private OperationInfos _operation;

			[SerializeField]
			private float _timing;

			public OperationInfos operation => _operation;

			public float timming => _timing;

			public void Initialize()
			{
				_operation.Initialize();
			}

			public void Run(Character owner)
			{
				_operation.gameObject.SetActive(true);
				_operation.Run(owner);
			}
		}

		[SerializeField]
		[GetComponentInParent(false)]
		private Weapon _weapon;

		[Tooltip("앰프 프리팹")]
		[SerializeField]
		private OperationRunner _ampOriginal;

		[Tooltip("몇 박자마다 반복 할 것인지 기입")]
		[SerializeField]
		private int _beat = 1;

		[Tooltip("Beat에서 지정 한 박자 내에서 아래 지정한 백분률 지점마다 정해준 OperationInfos를 실행하게 됨\n예를 들어 Beat가 2인 상태로 0, 0.5 두 지점에서 동일한 OpeartionInfos를 실행하면 한 박자에 한 번씩 실행 됨")]
		[SerializeField]
		private Timing[] _timings;

		private List<OperationRunner> _ampObjects = new List<OperationRunner>();

		public int beat
		{
			get
			{
				return _beat;
			}
			private set
			{
				_beat = value;
			}
		}

		public bool ampExists => _ampObjects.Count > 0;

		public event Action onInstantiate;

		private void Awake()
		{
			List<OperationInfos> list = new List<OperationInfos>();
			Timing[] timings = _timings;
			foreach (Timing timing in timings)
			{
				if (!list.Contains(timing.operation))
				{
					list.Add(timing.operation);
					timing.operation.Initialize();
				}
			}
		}

		public void InstantiateAmp(Vector3 position, bool flipX)
		{
			OperationRunner operationRunner = _ampOriginal.Spawn();
			operationRunner.transform.SetPositionAndRotation(position, Quaternion.identity);
			int num = ((!((_weapon.owner.lookingDirection == Character.LookingDirection.Left) ^ flipX)) ? 1 : (-1));
			operationRunner.transform.localScale = new Vector3(num, 1f, 1f);
			operationRunner.GetComponent<SpriteRenderer>().flipX = flipX;
			operationRunner.operationInfos.Run(_weapon.owner);
			_ampObjects.Add(operationRunner);
			this.onInstantiate?.Invoke();
		}

		public void PlayAmpBeat(int index)
		{
			for (int num = _ampObjects.Count - 1; num >= 0; num--)
			{
				OperationRunner operationRunner = _ampObjects[num];
				if (operationRunner == null || !operationRunner.gameObject.activeSelf)
				{
					_ampObjects.Remove(operationRunner);
				}
				else
				{
					base.transform.position = operationRunner.transform.position;
					float num2 = ((_weapon.owner.lookingDirection == Character.LookingDirection.Right) ? 1f : (-1f));
					num2 *= operationRunner.transform.localScale.x;
					base.transform.localScale = new Vector3(num2, 1f, 1f);
					_timings[index].Run(_weapon.owner);
				}
			}
		}

		public float[] GetTimings()
		{
			float[] array = new float[_timings.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _timings[i].timming / (float)beat;
			}
			return array;
		}
	}
}
