using System;
using System.Collections;
using Characters.Operations;
using Characters.Operations.Attack;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Pope
{
	public class EscortOrb : MonoBehaviour
	{
		[Serializable]
		private class Rotate
		{
			[SerializeField]
			internal float speed;
		}

		[Serializable]
		private class Fire
		{
			[SerializeField]
			[Subcomponent(typeof(OperationInfos))]
			private OperationInfos operationInfos;

			[SerializeField]
			internal float duration;

			internal void Initialize()
			{
				operationInfos.Initialize();
			}

			internal void Run(Character character)
			{
				operationInfos.gameObject.SetActive(true);
				operationInfos.Run(character);
			}
		}

		[SerializeField]
		private Character _character;

		[SerializeField]
		[Subcomponent(typeof(SweepAttack))]
		private SweepAttack _attack;

		[SerializeField]
		private Fire _fire;

		[SerializeField]
		private Transform _pivot;

		[SerializeField]
		private float _speed;

		private float _elapsed;

		private void Awake()
		{
			_fire.Initialize();
			_attack.Initialize();
		}

		private void OnEnable()
		{
			_attack.Run(_character);
			StartCoroutine(CStartFireLoop());
		}

		public void Initialize(float startRadian)
		{
			_elapsed = startRadian;
		}

		public void Move(float radius)
		{
			Vector3 vector = _pivot.transform.position - _character.transform.position;
			_elapsed += _speed * _character.chronometer.master.deltaTime;
			_character.movement.Move((Vector2)vector + new Vector2(Mathf.Cos(_elapsed), Mathf.Sin(_elapsed)) * radius);
		}

		private IEnumerator CStartFireLoop()
		{
			while (true)
			{
				yield return Chronometer.global.WaitForSeconds(_fire.duration);
				_fire.Run(_character);
			}
		}
	}
}
