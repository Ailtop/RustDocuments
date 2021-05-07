using System.Collections;
using Characters.Actions;
using Characters.Operations;
using UnityEngine;

namespace Characters.AI.Behaviours.Chimera
{
	public class SubjectDrop : Behaviour
	{
		[Header("Ready")]
		[SerializeField]
		private OperationInfos _readyOperations;

		[Header("Roar")]
		[SerializeField]
		private OperationInfos _roarOperations;

		[Header("Fire")]
		[SerializeField]
		private SequentialAction _fireSequencialAction;

		[SerializeField]
		private OperationInfos[] _fireOperationInfos;

		[Header("End")]
		[SerializeField]
		private OperationInfos _endOperations;

		[SerializeField]
		private float _coolTime;

		[SerializeField]
		private float _term = 2f;

		[SerializeField]
		[Range(1f, 10f)]
		private float _range;

		[SerializeField]
		private float _height;

		[SerializeField]
		private Transform points;

		[SerializeField]
		private Transform _startPoint;

		private Coroutine _coolDown;

		public bool canUse { get; set; } = true;


		private void Awake()
		{
			_readyOperations.Initialize();
			_roarOperations.Initialize();
			_endOperations.Initialize();
			for (int i = 0; i < _fireOperationInfos.Length; i++)
			{
				_fireOperationInfos[i].Initialize();
			}
		}

		public void Ready(Character character)
		{
			_readyOperations.gameObject.SetActive(true);
			_readyOperations.Run(character);
		}

		public void Roar(Character character)
		{
			_roarOperations.gameObject.SetActive(true);
			_roarOperations.Run(character);
		}

		public void End(Character character)
		{
			_endOperations.gameObject.SetActive(true);
			_endOperations.Run(character);
		}

		public void Run(AIController controller)
		{
			if (_coolDown != null)
			{
				StopCoroutine(_coolDown);
			}
			StartCoroutine(CRun(controller));
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_coolDown = StartCoroutine(CoolDown(controller.character.chronometer.animation));
			SetPoints();
			_fireOperationInfos.Shuffle();
			for (int i = 0; i < _fireOperationInfos.Length; i++)
			{
				_fireOperationInfos[i].gameObject.SetActive(true);
				_fireOperationInfos[i].Run(controller.character);
				yield return controller.character.chronometer.animation.WaitForSeconds(0.5f);
			}
			base.result = Result.Done;
		}

		private void SetPoints()
		{
			for (int i = 0; i < points.childCount; i++)
			{
				float num = Random.Range(0f, _range);
				points.GetChild(i).position = new Vector2(_startPoint.transform.position.x + num + (_range + _term) * (float)i, _height);
			}
		}

		private IEnumerator CoolDown(Chronometer chronometer)
		{
			canUse = false;
			yield return chronometer.WaitForSeconds(_coolTime);
			canUse = true;
		}
	}
}
