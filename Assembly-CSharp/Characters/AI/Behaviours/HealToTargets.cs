using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using FX;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class HealToTargets : Behaviour
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

		public override IEnumerator CRun(AIController controller)
		{
			while (_healMotion.running)
			{
				yield return null;
			}
			base.result = Result.Done;
		}

		public void SetTarget(List<Character> targets)
		{
		}
	}
}
