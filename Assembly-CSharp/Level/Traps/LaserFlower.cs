using System.Collections;
using Characters;
using Characters.Actions;
using UnityEngine;

namespace Level.Traps
{
	public class LaserFlower : MonoBehaviour
	{
		private enum FireDirection
		{
			Up,
			Down,
			Right,
			Left
		}

		[SerializeField]
		private Character _character;

		[SerializeField]
		private GameObject _horizontalBody;

		[SerializeField]
		private GameObject _verticalBody;

		[SerializeField]
		[Range(0f, 1f)]
		private float _startTiming;

		[SerializeField]
		private float _interval = 4f;

		[SerializeField]
		private int _laserSize = 3;

		[SerializeField]
		private Action _attackAction;

		[SerializeField]
		private Action _idleAction;

		[SerializeField]
		private Transform _attackRangeTransform;

		[SerializeField]
		private Transform _effectHead;

		[SerializeField]
		private Transform _effectBody;

		private FireDirection _fireDirection;

		private void OnEnable()
		{
			_attackRangeTransform.localScale = new Vector3(1f, _laserSize + 1, 1f);
			_effectBody.localScale = new Vector3(1f, _laserSize, 1f);
			Vector3 position = _effectBody.position;
			Vector3 position2 = Vector3.zero;
			float z = base.transform.localRotation.eulerAngles.z;
			if (!0f.Equals(z))
			{
				if (!180f.Equals(z))
				{
					if (!90f.Equals(z))
					{
						if (270f.Equals(z))
						{
							position2 = new Vector3(position.x + (float)_laserSize, position.y, position.z);
						}
					}
					else
					{
						position2 = new Vector3(position.x - (float)_laserSize, position.y, position.z);
					}
				}
				else
				{
					position2 = new Vector3(position.x, position.y - (float)_laserSize, position.z);
				}
			}
			else
			{
				position2 = new Vector3(position.x, position.y + (float)_laserSize, position.z);
			}
			_effectHead.position = position2;
		}

		private void Awake()
		{
			StartCoroutine(CAttack());
		}

		private IEnumerator CAttack()
		{
			yield return Chronometer.global.WaitForSeconds(_startTiming * _interval);
			while (true)
			{
				_attackAction.TryStart();
				yield return Chronometer.global.WaitForSeconds(_interval);
				_idleAction.TryStart();
			}
		}
	}
}
