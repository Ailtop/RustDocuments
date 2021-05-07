using System.Collections;
using Characters.Actions;
using FX;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class HealToTarget : Behaviour
	{
		[SerializeField]
		[MinMaxSlider(0f, 100f)]
		private Vector2 _amount;

		[SerializeField]
		private Action _healMotion;

		[SerializeField]
		private EffectInfo _info;

		[SerializeField]
		[Range(1f, 10f)]
		private int _count = 1;

		[SerializeField]
		[FrameTime]
		private float _time;

		[SerializeField]
		[FrameTime]
		private float _delay = 0.1f;

		private Character _target;

		public override IEnumerator CRun(AIController controller)
		{
			if (_target == null)
			{
				yield break;
			}
			base.result = Result.Doing;
			_healMotion.TryStart();
			int elapsed = 0;
			while (_healMotion.running)
			{
				yield return null;
				_time += controller.character.chronometer.master.deltaTime;
				if (_time >= (float)elapsed)
				{
					break;
				}
			}
			_info.Spawn(_target.transform.position);
			for (int i = 0; i < _count; i++)
			{
				float num = Random.Range(_amount.x, _amount.y);
				_target.health.Heal(num);
				yield return Chronometer.global.WaitForSeconds(_delay);
			}
			while (_healMotion.running)
			{
				yield return null;
			}
			base.result = Result.Done;
		}

		public void SetTarget(Character target)
		{
			_target = target;
		}
	}
}
