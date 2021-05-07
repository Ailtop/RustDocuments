using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Fly : Move
	{
		[MinMaxSlider(0f, 10f)]
		[SerializeField]
		private Vector2 _duration;

		[SerializeField]
		private Collider2D _flyableZoneCollider;

		private Bounds _flyableZone;

		private void Awake()
		{
			_flyableZone = _flyableZoneCollider.bounds;
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			base.result = Result.Doing;
			StartCoroutine(CExpire(controller, _duration));
			if (wander)
			{
				direction = Random.insideUnitSphere;
			}
			while (base.result == Result.Doing)
			{
				yield return null;
				character.movement.move = direction;
				if (Mathf.Abs(_flyableZone.min.x - character.transform.position.x) < 1f && direction.x < 0f)
				{
					direction.Set(0f - direction.x, direction.y);
				}
				if (Mathf.Abs(_flyableZone.max.x - character.transform.position.x) < 1f && direction.x > 0f)
				{
					direction.Set(0f - direction.x, direction.y);
				}
				if (Mathf.Abs(_flyableZone.min.y - character.transform.position.y) < 1f && direction.y < 0f)
				{
					direction.Set(direction.x, 0f - direction.y);
				}
				if (Mathf.Abs(_flyableZone.max.y - character.transform.position.y) < 1f && direction.y > 0f)
				{
					direction.Set(direction.x, 0f - direction.y);
				}
			}
			idle.CRun(controller);
		}
	}
}
