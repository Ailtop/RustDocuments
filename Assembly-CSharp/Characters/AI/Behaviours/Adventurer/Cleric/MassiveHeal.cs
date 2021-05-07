using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Adventurer;
using FX;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Cleric
{
	public class MassiveHeal : Behaviour
	{
		[SerializeField]
		private Commander _commander;

		[SerializeField]
		private Action _healMotion;

		[SerializeField]
		[MinMaxSlider(0f, 1f)]
		private Vector2 _amount;

		[SerializeField]
		private EffectInfo _info;

		[SerializeField]
		[Range(1f, 10f)]
		private int _count = 1;

		[SerializeField]
		[FrameTime]
		private float _healTime;

		[SerializeField]
		[FrameTime]
		private float _delay = 0.1f;

		[SerializeField]
		private EffectInfo _leoniaEffectInfo;

		[SerializeField]
		private Transform _leoniaEffectPoint;

		private void Awake()
		{
			if (_commander == null)
			{
				_commander = GetComponentInParent<Commander>();
			}
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			List<Combat> aliveCombats = _commander.alives;
			_healMotion.TryStart();
			float elapsed = 0f;
			while (_healMotion.running)
			{
				yield return null;
				elapsed += controller.character.chronometer.master.deltaTime;
				if (elapsed > _healTime)
				{
					break;
				}
			}
			foreach (Combat item in aliveCombats)
			{
				_info.Spawn(item.who.character.transform.position).transform.parent = item.who.character.transform;
			}
			SetLeoniaEffectPoint(controller.character.movement.controller.collisionState.lastStandingCollider.bounds);
			_leoniaEffectInfo.Spawn(_leoniaEffectPoint.position);
			for (int i = 0; i < _count; i++)
			{
				foreach (Combat item2 in aliveCombats)
				{
					float percent = Random.Range(_amount.x, _amount.y);
					item2.who.character.health.PercentHeal(percent);
				}
				yield return Chronometer.global.WaitForSeconds(_delay);
			}
			while (_healMotion.running)
			{
				yield return null;
			}
			base.result = Result.Done;
		}

		private void SetLeoniaEffectPoint(Bounds bounds)
		{
			_leoniaEffectPoint.position = new Vector2(bounds.center.x, bounds.max.y + 3.5f);
		}
	}
}
