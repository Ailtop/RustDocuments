using System;
using System.Collections;
using Characters.AI.Behaviours.Attacks;
using Level.Chapter4;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Pope
{
	public sealed class DivineCross : Behaviour
	{
		[Serializable]
		private class Point
		{
			[SerializeField]
			internal Transform attackPoint;

			[SerializeField]
			internal Transform firePoint;
		}

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveHandler))]
		private MoveHandler _moveHandler;

		[SerializeField]
		private PlatformContainer _platformContainer;

		private Platform[] _platforms;

		[Header("Points")]
		[SerializeField]
		private Point _crossPoint;

		[SerializeField]
		private Point[] _points;

		private void Awake()
		{
			_platforms = new Platform[_points.Length + 1];
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _moveHandler.CMove(controller);
			SettleOnDestination();
			yield return _attack.CRun(controller);
			base.result = Result.Success;
		}

		private void SettleOnDestination()
		{
			_platformContainer.RandomTakeTo(_platforms);
			for (int i = 0; i < _points.Length; i++)
			{
				_points[i].attackPoint.position = _platforms[i].transform.position;
				_points[i].firePoint.position = _platforms[i].GetFirePosition();
			}
			_crossPoint.attackPoint.position = _platformContainer.center.transform.position;
			_crossPoint.firePoint.position = _platformContainer.center.GetFirePosition();
		}
	}
}
