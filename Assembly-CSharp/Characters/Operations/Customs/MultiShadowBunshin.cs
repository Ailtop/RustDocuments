using System.Collections;
using System.Collections.Generic;
using FX;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class MultiShadowBunshin : CharacterOperation
	{
		[SerializeField]
		private EffectInfo _spawnEffect;

		[SerializeField]
		private OperationRunner _origin;

		[SerializeField]
		private OperationRunner _fake;

		[SerializeField]
		private int _totalCount;

		[SerializeField]
		private int _originCount;

		[SerializeField]
		private float _delay;

		private HashSet<int> _originIndics;

		private void Awake()
		{
			_originIndics = new HashSet<int>();
		}

		public override void Run(Character owner)
		{
			UpdateOriginIndics();
			StartCoroutine(CRun(owner));
		}

		private IEnumerator CRun(Character owner)
		{
			Bounds platform = owner.movement.controller.collisionState.lastStandingCollider.bounds;
			for (int i = 0; i < _totalCount; i++)
			{
				Vector3 euler = Vector2.zero;
				bool num = i >= _totalCount / 2;
				Vector2 vector;
				if (num)
				{
					vector = new Vector2(Random.Range(platform.center.x, platform.max.x), platform.max.y);
					euler.z = (180f - euler.z) % 360f;
				}
				else
				{
					vector = new Vector2(Random.Range(platform.min.x, platform.center.x), platform.max.y);
				}
				OperationRunner operationRunner = ((!_originIndics.Contains(i)) ? _fake : _origin);
				OperationInfos operationInfos = operationRunner.Spawn().operationInfos;
				operationInfos.transform.SetPositionAndRotation(vector, Quaternion.Euler(euler));
				if (num)
				{
					operationInfos.transform.localScale = new Vector3(1f, -1f, 1f);
				}
				else
				{
					operationInfos.transform.localScale = new Vector3(1f, 1f, 1f);
				}
				_spawnEffect.Spawn(vector);
				operationInfos.Run(owner);
				yield return owner.chronometer.master.WaitForSeconds(_delay);
			}
		}

		private void UpdateOriginIndics()
		{
			_originIndics.Clear();
			for (int i = 0; i < _originCount; i++)
			{
				int item = Random.Range(0, _totalCount);
				while (_originIndics.Contains(item))
				{
					item = Random.Range(0, _totalCount);
				}
				_originIndics.Add(item);
			}
		}
	}
}
