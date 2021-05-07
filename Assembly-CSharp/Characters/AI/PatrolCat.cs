using System.Collections;
using Characters.AI.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public class PatrolCat : AIController
	{
		[SerializeField]
		private bool _justIdle;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		[Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _moveToDestination;

		[SerializeField]
		[MinMaxSlider(5f, 30f)]
		private Vector2 _distanceRange;

		[SerializeField]
		private Transform _minPoint;

		[SerializeField]
		private Transform _maxPoint;

		protected override void OnEnable()
		{
			if (!_justIdle)
			{
				StartCoroutine(CProcess());
			}
		}

		protected override IEnumerator CProcess()
		{
			while (true)
			{
				yield return _idle.CRun(this);
				SetDestination();
				yield return _moveToDestination.CRun(this);
			}
		}

		private void SetDestination()
		{
			float num = Random.Range(_distanceRange.x, _distanceRange.y);
			int num2 = (MMMaths.RandomBool() ? 1 : (-1));
			switch (num2)
			{
			case 1:
				if (character.transform.position.x + (float)num2 * num >= _maxPoint.position.x)
				{
					num2 *= -1;
				}
				break;
			case -1:
				if (character.transform.position.x + (float)num2 * num <= _minPoint.position.x)
				{
					num2 *= -1;
				}
				break;
			}
			base.destination = new Vector2(character.transform.position.x + (float)num2 * num, character.transform.position.y);
		}
	}
}
