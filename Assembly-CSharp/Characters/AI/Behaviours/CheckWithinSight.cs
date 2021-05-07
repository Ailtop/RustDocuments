using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class CheckWithinSight : Behaviour
	{
		[SerializeField]
		private Collider2D _sightCollider;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			while (base.result == Result.Doing)
			{
				yield return null;
				if (!(controller.target != null))
				{
					Character character = controller.FindClosestPlayerBody(_sightCollider);
					if (character != null)
					{
						controller.target = character;
						controller.FoundEnemy();
						base.result = Result.Done;
					}
				}
			}
		}
	}
}
