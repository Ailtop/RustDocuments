using System;
using System.Collections;
using Characters.Actions;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Warrior
{
	public class PowerWave : Behaviour
	{
		[Serializable]
		private class WaveConfig
		{
			[SerializeField]
			[UnityEditor.Subcomponent(typeof(OperationInfos))]
			internal OperationInfos waveAttack;

			[SerializeField]
			internal Collider2D range;

			[SerializeField]
			[Range(1f, 30f)]
			internal int count;

			[SerializeField]
			internal float distance;

			[SerializeField]
			internal float interval;

			[SerializeField]
			internal float startDistance = 1f;
		}

		[SerializeField]
		private Characters.Actions.Action _ready;

		[SerializeField]
		private Characters.Actions.Action _attack;

		[SerializeField]
		private WaveConfig _waveConfig;

		private void Awake()
		{
			_waveConfig.waveAttack.Initialize();
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_ready.TryStart();
			while (_ready.running)
			{
				yield return null;
			}
			StartCoroutine(CWave(controller.character));
			_attack.TryStart();
			while (_attack.running)
			{
				yield return null;
			}
			base.result = Result.Done;
		}

		private IEnumerator CWave(Character owner)
		{
			int direction = ((owner.lookingDirection == Character.LookingDirection.Right) ? 1 : (-1));
			float startPointX = owner.transform.position.x + (float)direction * _waveConfig.startDistance;
			float extentX = _waveConfig.range.bounds.extents.x;
			for (int i = 0; i < _waveConfig.count; i++)
			{
				_waveConfig.waveAttack.gameObject.SetActive(true);
				Vector2 vector = new Vector2(startPointX + (extentX + _waveConfig.distance) * (float)i * (float)direction, owner.transform.position.y);
				_waveConfig.range.transform.position = vector;
				_waveConfig.waveAttack.Run(owner);
				yield return owner.chronometer.animation.WaitForSeconds(_waveConfig.interval);
			}
		}
	}
}
