using System.Collections;
using System.Collections.Generic;
using Characters.Operations;
using UnityEngine;

namespace Characters.AI.Behaviours.Chimera
{
	public class VenomFall : Behaviour
	{
		[SerializeField]
		private float _coolTime;

		[Header("Ready")]
		[SerializeField]
		private OperationInfos _readyOperations;

		[Header("Roar")]
		[SerializeField]
		private OperationInfos _roarOperations;

		[Header("Fire")]
		[SerializeField]
		private OperationInfos[] _operations;

		[SerializeField]
		private float _term = 2f;

		[SerializeField]
		[Range(1f, 10f)]
		private float _range;

		[SerializeField]
		private Transform points;

		[SerializeField]
		private Transform _startPoint;

		private const int _maxOrder = 4;

		private Queue<int> _order;

		private bool _coolDown = true;

		private void Awake()
		{
			for (int i = 0; i < 4; i++)
			{
				_operations[i].Initialize();
			}
			_readyOperations.Initialize();
			_roarOperations.Initialize();
		}

		private void ShuffleOrder()
		{
			int[] array = new int[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = i;
			}
			array.Shuffle();
			_order = new Queue<int>(4);
			for (int j = 0; j < 4; j++)
			{
				_order.Enqueue(array[j]);
			}
			SetPoints();
		}

		public void Ready(Character character)
		{
			ShuffleOrder();
			_readyOperations.gameObject.SetActive(true);
			_readyOperations.Run(character);
		}

		public void Roar(Character character)
		{
			_roarOperations.gameObject.SetActive(true);
			_roarOperations.Run(character);
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			int num = _order.Dequeue();
			StartCoroutine(CoolDown(controller.character.chronometer.animation));
			_operations[num].gameObject.SetActive(true);
			_operations[num].Run(character);
			if (_order.Count == 1)
			{
				int num2 = _order.Dequeue();
				_operations[num2].gameObject.SetActive(true);
				_operations[num2].Run(character);
			}
			base.result = Result.Done;
			yield break;
		}

		private void SetPoints()
		{
			for (int i = 0; i < points.childCount; i++)
			{
				float num = Random.Range(0f, _range);
				points.GetChild(i).position = new Vector2(_startPoint.transform.position.x + num + (_range + _term) * (float)i, _startPoint.transform.position.y);
			}
		}

		private IEnumerator CoolDown(Chronometer chronometer)
		{
			_coolDown = false;
			yield return chronometer.WaitForSeconds(_coolTime);
			_coolDown = true;
		}

		public bool CanUse()
		{
			return _coolDown;
		}
	}
}
