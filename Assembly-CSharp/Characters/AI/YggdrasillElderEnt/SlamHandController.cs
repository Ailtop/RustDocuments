using System.Collections;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class SlamHandController : MonoBehaviour
	{
		[SerializeField]
		private float _targetingDelay = 0.64f;

		[SerializeField]
		private Health _owenrHealth;

		[SerializeField]
		private SlamHand _left;

		[SerializeField]
		private SlamHand _right;

		private SlamHand _current;

		private void Awake()
		{
			_owenrHealth.onDie += DisableHands;
		}

		public void Ready()
		{
			_left.ActiavteHand();
			_right.ActiavteHand();
			StartCoroutine(CsetDestination());
		}

		public void Slam()
		{
			StartCoroutine(_current.CSlam());
		}

		public void Recover()
		{
			StartCoroutine(_current.CRecover());
		}

		public void Vibrate()
		{
			StartCoroutine(_current.CVibrate());
		}

		public void DisableHands()
		{
			_left.DeactivateHand();
			_right.DeactivateHand();
		}

		private void SetHand()
		{
			float x = Singleton<Service>.Instance.levelManager.player.transform.position.x;
			bool flag = Mathf.Abs(x - _left.transform.position.x) < Mathf.Abs(x - _right.transform.position.x);
			_current = (flag ? _left : _right);
		}

		private IEnumerator CsetDestination()
		{
			yield return Chronometer.global.WaitForSeconds(_targetingDelay);
			SetHand();
			SetDestination();
		}

		private void SetDestination()
		{
			Transform transform = Singleton<Service>.Instance.levelManager.player.transform;
			_current.destination = new Vector3(transform.position.x, Map.Instance.bounds.max.y);
		}
	}
}
