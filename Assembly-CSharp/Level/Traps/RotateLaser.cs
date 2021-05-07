using System;
using System.Collections;
using Characters;
using Characters.Operations.Attack;
using Characters.Utils;
using UnityEngine;

namespace Level.Traps
{
	public sealed class RotateLaser : MonoBehaviour
	{
		[Serializable]
		private class Rotate
		{
			[SerializeField]
			internal float delay;

			[SerializeField]
			internal float amount;

			[SerializeField]
			internal float delta;
		}

		[SerializeField]
		private Character _owner;

		[SerializeField]
		private SweepAttack[] _attackOperations;

		[SerializeField]
		private Transform _body;

		[SerializeField]
		private Rotate _rotate;

		[SerializeField]
		private float _loopTime;

		[SerializeField]
		private AnimationClip _endAnimation;

		private HitHistoryManager _hitHistoryManager;

		private int _direction;

		private float _speed;

		private void Awake()
		{
			_hitHistoryManager = new HitHistoryManager(99999);
			SweepAttack[] attackOperations = _attackOperations;
			foreach (SweepAttack obj in attackOperations)
			{
				obj.Initialize();
				obj.collisionDetector.hits = _hitHistoryManager;
			}
			_owner.health.onDied += Hide;
		}

		private void OnEnable()
		{
			ResetSetting();
			StartCoroutine(CStart(_owner.chronometer.master));
		}

		private IEnumerator CStart(Chronometer chronometer)
		{
			yield return chronometer.WaitForSeconds(_rotate.delay);
			StartCoroutine(CLoop(chronometer));
		}

		private IEnumerator CLoop(Chronometer chronometer)
		{
			SweepAttack[] attackOperations = _attackOperations;
			for (int i = 0; i < attackOperations.Length; i++)
			{
				attackOperations[i].Run(_owner);
			}
			float elapsed = 0f;
			while (elapsed < _loopTime)
			{
				if (_speed < _rotate.amount)
				{
					_speed += _rotate.delta;
				}
				_body.transform.Rotate(Vector3.forward, (float)_direction * _speed * chronometer.deltaTime);
				elapsed += chronometer.deltaTime;
				yield return null;
			}
			StartCoroutine(CEnd(chronometer));
		}

		private IEnumerator CEnd(Chronometer chronometer)
		{
			yield return chronometer.WaitForSeconds(_endAnimation.length);
			Hide();
		}

		private void ResetSetting()
		{
			_speed = 0f;
			_body.rotation = Quaternion.identity;
			_direction = (MMMaths.RandomBool() ? 1 : (-1));
			_hitHistoryManager.Clear();
		}

		private void Hide()
		{
			base.gameObject.SetActive(false);
		}
	}
}
