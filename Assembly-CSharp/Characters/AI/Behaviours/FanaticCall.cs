using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class FanaticCall : Behaviour
	{
		[SerializeField]
		private Transform _spawnPositionParent;

		[SerializeField]
		private Action _action;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			SetSpawnPoint(controller.target);
			if (!_action.TryStart())
			{
				base.result = Result.Fail;
				yield break;
			}
			while (_action.running)
			{
				yield return null;
			}
			base.result = Result.Success;
		}

		private void SetSpawnPoint(Character target)
		{
			Bounds bounds = target.movement.controller.collisionState.lastStandingCollider.bounds;
			foreach (Transform item in _spawnPositionParent)
			{
				float x = Random.Range(bounds.min.x, bounds.max.x);
				float y = bounds.max.y;
				item.position = new Vector2(x, y);
			}
		}

		public bool CanUse()
		{
			return _action.canUse;
		}
	}
}
